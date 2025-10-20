using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InmobiliariaConlara.Models; // Asegúrate que el namespace sea el correcto
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace InmobiliariaConlara.Controllers.API
{
    [ApiController]
    // 1. CAMBIO DE RUTA: Ahora la ruta base será /api/Auth
    [Route("api/[controller]")]
    public class AuthController : ControllerBase // 2. CAMBIO DE NOMBRE
    {
        private readonly RepositorioUsuario _repositorioUsuario;
        private readonly IConfiguration _configuration;

        public AuthController(RepositorioUsuario repositorioUsuario, IConfiguration configuration)
        {
            _repositorioUsuario = repositorioUsuario;
            _configuration = configuration;
        }

        // ruta: /api/Auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromForm] LoginRequest request)
        {
            try
            {
                var user = _repositorioUsuario.Login(request.Email, request.Password);

                if (user == null)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                if (user.Rol != (int)enRoles.Propietario)
                {
                    return Unauthorized(new { message = "Acceso denegado. Solo los propietarios pueden usar la aplicación móvil." });
                }

                var token = GenerarToken(user);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error inesperado.", error = ex.Message });
            }
        }

        // ruta: /api/Auth/perfil
        [HttpGet("perfil")]
        [Authorize(Policy = "EsPropietarioApp")]
        public IActionResult Perfil()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var user = _repositorioUsuario.ObtenerPorId(Convert.ToInt32(userIdClaim.Value));
            if (user == null)
            {
                return NotFound();
            }

            var perfil = new
            {
                user.IdUsuario,
                user.Nombre,
                user.Apellido,
                user.Dni,
                user.Telefono,
                user.Email,
                user.Avatar
            };

            return Ok(perfil);
        }

        private string GenerarToken(Usuario usuario)
        {
            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                throw new InvalidOperationException("La clave JWT no está configurada en appsettings.json");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.RolNombre)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // ruta: /api/Auth/actualizar
        [HttpPost("actualizar")]
        [Authorize(Policy = "EsPropietarioApp")]
        public IActionResult Actualizar([FromBody] Usuario usuarioActualizado)
        
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                int idUsuario = Convert.ToInt32(userIdClaim.Value);

                usuarioActualizado.IdUsuario = idUsuario;

                int resultado = _repositorioUsuario.ActualizarPerfilDesdeApp(usuarioActualizado);

                if (resultado > 0)
                {
                    return Ok(usuarioActualizado);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch
            {
                return StatusCode(500, new { message = "Ocurrió un error inesperado." });
            }
        }
    }
}
