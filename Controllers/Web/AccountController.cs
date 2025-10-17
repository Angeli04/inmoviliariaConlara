using System.Security.Claims;
using InmobiliariaConlara.Models; // Asegúrate que el namespace sea el correcto
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaConlara.Controllers.Web // O el namespace que estés usando
{
    // 1. Se establece la ruta base para este controlador como "/Account"
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly RepositorioUsuario _repositorioUsuario;

        public AccountController(RepositorioUsuario repositorio)
        {
            _repositorioUsuario = repositorio;
        }

        // 2. Esta acción responde a GET /Account/Login
        [HttpGet("Login")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(); // Esto busca la vista en /Views/Account/Login.cshtml
        }

        // 3. Esta acción responde a POST /Account/Login
        [HttpPost("Login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password, string returnUrl = null)
        {
            // Validar que los datos no estén vacíos
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email y contraseña son requeridos.");
                return View();
            }

            var user = _repositorioUsuario.Login(email, password);

            if (user == null)
            {
                ModelState.AddModelError("", "Credenciales inválidas");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.RolNombre),
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // 4. Esta acción responde a POST /Account/Logout por seguridad
        [HttpPost("Logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}