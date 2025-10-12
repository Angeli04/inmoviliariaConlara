using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using InmobiliariaConlara.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace InmobiliariaConlara.Controllers
{
    public class AccountController : Controller
    {
        private readonly RepositorioUsuario repositorio;
        private readonly IConfiguration _config;
        private const string GlobalSalt = "MiSaltSecreto123";

        public AccountController(RepositorioUsuario repo, IConfiguration config)
        {
            repositorio = repo;
            _config = config;
        }

        // ----------------- LOGIN WEB -----------------
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = repositorio.Login(email, password);

            if (user == null)
            {
                ViewBag.Error = "Credenciales inválidas";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Rol == 1 ? "Administrador" :
                                         user.Rol == 2 ? "Empleado" : "Propietario"),
                new Claim("UserId", user.IdUsuario.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                });

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Perfil()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            var user = repositorio.ObtenerPorEmail(email);
            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        // ----------------- LOGIN API MÓVIL -----------------
        [HttpPost("api/login")]
        [AllowAnonymous]
        public IActionResult ApiLogin([FromBody] LoginRequest request)
        {
            var user = repositorio.Login(request.Email, request.Password);
            if (user == null)
                return Unauthorized(new { message = "Credenciales inválidas" });

            // Solo Propietarios pueden usar la app móvil
            if (user.Rol != (int)enRoles.Propietario)
                return Forbid("Solo los Propietarios pueden acceder desde la app móvil");

            var token = GenerarToken(user);

            // 🔹 Solo devolvemos el token como string
            return Ok(token);
        }

        // ----------------- MÉTODO AUXILIAR JWT -----------------
        private string GenerarToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
                new Claim("id", usuario.IdUsuario.ToString()),
                new Claim("rol", usuario.RolNombre ?? "Propietario")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(4),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // ----------------- MODELO LOGIN REQUEST -----------------
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
