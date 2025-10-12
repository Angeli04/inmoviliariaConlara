using System.Security.Claims;
using System.Text;
using InmobiliariaConlara.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;



namespace InmobiliariaConlara.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IWebHostEnvironment environment;
        private readonly RepositorioUsuario repositorio;

        //  SALT HARDCODEADO (para desarrollo)
        private const string GlobalSalt = "MiSaltSecreto123";

        public UsuarioController(IWebHostEnvironment environment, RepositorioUsuario repositorio)
        {
            this.environment = environment;
            this.repositorio = repositorio;
        }


        [Authorize(Roles = "Administrador")]
        // GET: Usuario
        public ActionResult Index()
        {
            var usuarios = repositorio.ObtenerTodos();
            return View(usuarios);
        }

        [Authorize(Roles = "Administrador")]
        // GET: Usuario/Details/5
        public ActionResult Details(int id)
        {
            var e = repositorio.ObtenerPorId(id);
            return View(e);
        }


        [Authorize(Roles = "Administrador")]
        // GET: Usuario/Create
        public ActionResult Create()
        {
            ViewBag.Roles = Usuario.ObtenerRoles();
            return View();
        }

[Authorize(Roles = "Administrador")]
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Create(Usuario u)
{
    if (!ModelState.IsValid)
    {
        ViewBag.Roles = Usuario.ObtenerRoles();
        return View();
    }

    //  Generar salt fijo
    byte[] saltBytes = Encoding.ASCII.GetBytes(GlobalSalt);

    //  Hashear contraseña
    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: u.Clave,
        salt: saltBytes,
        prf: KeyDerivationPrf.HMACSHA1,
        iterationCount: 10000,
        numBytesRequested: 256 / 8));
    u.Clave = hashed;

    //  Asignar rol de forma segura
    if (!User.IsInRole("Administrador"))
    {
        u.Rol = (int)enRoles.Empleado;
    }

    //  Guardar usuario
    int res = repositorio.Alta(u);

    //  Procesar avatar
    string wwwPath = environment.WebRootPath;
    string uploadPath = Path.Combine(wwwPath, "Uploads");
    if (!Directory.Exists(uploadPath))
        Directory.CreateDirectory(uploadPath);

    if (u.AvatarFile != null && u.IdUsuario > 0)
    {
        string fileName = "avatar_" + u.IdUsuario + Path.GetExtension(u.AvatarFile.FileName);
        string pathCompleto = Path.Combine(uploadPath, fileName);
        using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
            u.AvatarFile.CopyTo(stream);

        u.Avatar = Path.Combine("/Uploads", fileName);
        repositorio.Modificacion(u);
    }
    else if (u.IdUsuario > 0)
    {
        u.Avatar = "/Uploads/avatar_0.png"; // imagen por defecto
        repositorio.Modificacion(u);
    }

    return RedirectToAction(nameof(Index));
}


        [Authorize(Roles = "Administrador")]
        // GET: Usuarios/Edit/5

        public ActionResult Perfil()
        {
            ViewData["Title"] = "Mi perfil";
            var u = repositorio.ObtenerPorEmail(User.Identity.Name);
            ViewBag.Roles = Usuario.ObtenerRoles();
            return View("edit", u);
        }


        [Authorize(Roles = "Administrador,Empleado")]
        public ActionResult Edit(int id)
        {
            ViewData["Title"] = "Editar usuario";
            var u = repositorio.ObtenerPorId(id);
            ViewBag.Roles = Usuario.ObtenerRoles();
            return View(u);
        }


        [Authorize(Roles = "Administrador,Empleado")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Usuario u)
        {
            var vista = nameof(Edit);
            if (!User.IsInRole("Administrador"))
            {
                vista = nameof(Perfil);
                var usuarioActual = repositorio.ObtenerPorEmail(User.Identity.Name);
                if (usuarioActual.IdUsuario != id)
                    return RedirectToAction(nameof(Index), "Home");
            }

            //  Hashear nueva contraseña si se cambió
            if (!string.IsNullOrEmpty(u.Clave))
            {
                byte[] saltBytes = Encoding.ASCII.GetBytes(GlobalSalt);
                u.Clave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: u.Clave,
                    salt: saltBytes,
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));
            }


             // Procesar avatar si se subió uno nuevo
            string wwwPath = environment.WebRootPath;
            string uploadPath = Path.Combine(wwwPath, "Uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            if (u.AvatarFile != null && u.IdUsuario > 0)
            {
                string fileName = "avatar_" + u.IdUsuario + Path.GetExtension(u.AvatarFile.FileName);
                string pathCompleto = Path.Combine(uploadPath, fileName);
                using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
                    u.AvatarFile.CopyTo(stream);

                u.Avatar = Path.Combine("/Uploads", fileName);
            }
            else
            {
                // Si no sube nueva foto, mantenemos la anterior
                var usuarioDb = repositorio.ObtenerPorId(u.IdUsuario);
                u.Avatar = usuarioDb.Avatar;
            }

            repositorio.Modificacion(u);
            if (User.IsInRole("Administrador"))
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction("Perfil", "Account"); 
            }
        }


        [Authorize(Roles="Administrador")]
        public IActionResult Delete(int id)
        {
            var usuario = repositorio.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Usuario usuario)
        {
            try
            {
                
                var ruta = Path.Combine(environment.WebRootPath, "Uploads", $"avatar_{id}" + Path.GetExtension(usuario.Avatar));
                if (System.IO.File.Exists(ruta))
                    System.IO.File.Delete(ruta);
                repositorio.Baja(usuario);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }



        [Authorize(Roles = "Administrador")]
        public IActionResult Avatar()
        {
            var u = repositorio.ObtenerPorEmail(User.Identity.Name);
            string fileName = "avatar_" + u.IdUsuario + Path.GetExtension(u.Avatar);
            string pathCompleto = Path.Combine(environment.WebRootPath, "Uploads", fileName);
            byte[] fileBytes = System.IO.File.ReadAllBytes(pathCompleto);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        [Authorize(Roles = "Administrador")]
        public string AvatarBase64()
        {
            var u = repositorio.ObtenerPorEmail(User.Identity.Name);
            string fileName = "avatar_" + u.IdUsuario + Path.GetExtension(u.Avatar);
            string pathCompleto = Path.Combine(environment.WebRootPath, "Uploads", fileName);
            byte[] fileBytes = System.IO.File.ReadAllBytes(pathCompleto);
            return Convert.ToBase64String(fileBytes);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost("[controller]/[action]/{fileName}")]
        public IActionResult FromBase64([FromBody] string imagen, [FromRoute] string fileName)
        {
            string pathCompleto = Path.Combine(environment.WebRootPath, "Uploads", fileName);
            byte[] bytes = Convert.FromBase64String(imagen);
            System.IO.File.WriteAllBytes(pathCompleto, bytes);
            return Ok();
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Foto()
        {
            var u = repositorio.ObtenerPorEmail(User.Identity.Name);
            var stream = System.IO.File.Open(Path.Combine(environment.WebRootPath, u.Avatar.Substring(1)), FileMode.Open, FileAccess.Read);
            var ext = Path.GetExtension(u.Avatar);
            return new FileStreamResult(stream, $"image/{ext.Substring(1)}");
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Datos()
        {
            var u = repositorio.ObtenerPorEmail(User.Identity.Name);
            string buffer = "Nombre;Apellido;Email" + Environment.NewLine +
                            $"{u.Nombre};{u.Apellido};{u.Email}";
            var stream = new MemoryStream(Encoding.Unicode.GetBytes(buffer));
            var res = new FileStreamResult(stream, "text/plain") { FileDownloadName = "Datos.csv" };
            return res;
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult LoginModal()
        {
            return PartialView("_LoginModal", new Login());
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Login(string returnUrl)
        {
            TempData["returnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login)
        {
            try
            {
                var returnUrl = string.IsNullOrEmpty(TempData["returnUrl"] as string) ? "/Home" : TempData["returnUrl"].ToString();
                if (ModelState.IsValid)
                {
                    //  Salt fijo
                    byte[] saltBytes = Encoding.ASCII.GetBytes(GlobalSalt);

                    //  Hashear clave ingresada
                    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: login.Clave,
                        salt: saltBytes,
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 10000,
                        numBytesRequested: 256 / 8));

                    var e = repositorio.ObtenerPorEmail(login.Usuario);
                    if (e == null || e.Clave != hashed)
                    {
                        ModelState.AddModelError("", "El email o la clave no son correctos");
                        TempData["returnUrl"] = returnUrl;
                        return View();
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, e.Email),
                        new Claim("FullName", e.Nombre + " " + e.Apellido),
                        new Claim(ClaimTypes.Role, e.RolNombre),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    TempData.Remove("returnUrl");
                    return Redirect(returnUrl);
                }
                TempData["returnUrl"] = returnUrl;
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [Authorize(Roles = "Administrador")]
        [Route("salir", Name = "logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("index", "Home");
        }

    }
}
