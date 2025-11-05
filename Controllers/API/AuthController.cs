using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Inmobiliaria.Models;
using InmobiliariaConlara.Models; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace InmobiliariaConlara.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase 
    {
        private readonly RepositorioUsuario _repositorioUsuario;
        private readonly IConfiguration _configuration;

        private readonly RepositorioInmuebles _repositorioInmuebles;

        private readonly RepositorioTipoInmueble _repositorioTipoInmueble;

        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly RepositorioInquilino _repositorioInquilino;





        public AuthController(RepositorioUsuario repositorioUsuario, IConfiguration configuration, RepositorioInmuebles repositorioInmuebles, RepositorioTipoInmueble repositorioTipoInmueble, IWebHostEnvironment hostingEnvironment, RepositorioInquilino repositorioInquilino)
        {
            _repositorioUsuario = repositorioUsuario;
            _configuration = configuration;
            _repositorioInmuebles = repositorioInmuebles;
            _repositorioTipoInmueble = repositorioTipoInmueble;
            _hostingEnvironment = hostingEnvironment;
            _repositorioInquilino = repositorioInquilino;
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

        // ruta: /api/Auth/listarInmuebles
        [HttpGet("listarInmuebles")]
        [Authorize(Policy = "EsPropietarioApp")]
        public IActionResult ListarInmuebles()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var idUsuario = Convert.ToInt32(userIdClaim.Value);
            var inmuebles = _repositorioInmuebles.ObtenerInmueblesCompletosPorPropietario(idUsuario);

            return Ok(inmuebles);
        }

        // ruta: /api/Auth/verInmueble/{id}
        [HttpGet("verInmueble/{id}")]
        [Authorize(Policy = "EsPropietarioApp")]
        public IActionResult VerInmueble(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var inmueble = _repositorioInmuebles.ObtenerInmueblesCompletosPorIdApi(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            return Ok(inmueble);
        }

        //ruta: /api/Auth/Habilitacion
        [HttpPut("Habilitacion")]
        [Authorize(Policy = "EsPropietarioApp")]
        public IActionResult Habilitacion([FromBody] Inmuebles inmueble)
        {
            bool habilitado = inmueble.Habilitado;

            int id = inmueble.IdInmuebles;
            Inmuebles inmuebleActualizado = _repositorioInmuebles.Habilitar(id, habilitado);


            return Ok(inmuebleActualizado);
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

        // ruta: /api/Auth/listarTipos
        [HttpGet("listarTipos")]
        [Authorize(Policy = "EsPropietarioApp")]
        public IActionResult ListarTipos()
        {
            var tipos = _repositorioTipoInmueble.ObtenerTodos();
            return Ok(tipos);
        }

        // ruta: /api/Auth/altaInmueble
        [HttpPost("altaInmueble")]
        [Authorize(Policy = "EsPropietarioApp")]
        public async Task<IActionResult> AltaInmuebleAsync([FromForm] IFormFile imagen, [FromForm] String inmuebleJson)
        {
            if (imagen == null || string.IsNullOrEmpty(inmuebleJson))
            {
                return BadRequest();
            }
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                Inmuebles? inmueble = JsonSerializer.Deserialize<Inmuebles>(inmuebleJson, options);

                if (inmueble == null)
                {
                    return BadRequest("El inmueble no es válido");
                }

                string wwwRootPath = _hostingEnvironment.WebRootPath;
                string uploadPath = Path.Combine(wwwRootPath, "Uploads");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string extension = Path.GetExtension(imagen.FileName);
                string nombreArchivoUnico = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(uploadPath, nombreArchivoUnico);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(fileStream);
                }

                inmueble.ImagenUrl = $"/Uploads/{nombreArchivoUnico}";

                var idUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                inmueble.IdUsuario = idUsuario;
                int idCreado = _repositorioInmuebles.Alta(inmueble);
                inmueble.IdInmuebles = idCreado;
                return Ok(inmueble);
            }
            catch
            {
                return BadRequest();
            }
        }

        // ruta: /api/Auth/actualizarInmueble
        [HttpPost("actualizarInmueble")]
        [Authorize(Policy = "EsPropietarioApp")]
        public async Task<IActionResult> ActualizarInmueble([FromForm] IFormFile? imagen, [FromForm] String inmuebleJson)
        {
            if (string.IsNullOrEmpty(inmuebleJson))
            {
                return BadRequest();
            }
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                Inmuebles? inmueble = JsonSerializer.Deserialize<Inmuebles>(inmuebleJson, options);

                if (inmueble == null)
                {
                    return BadRequest("El inmueble no es válido");
                }

                if (imagen != null)
                {
                    string wwwRootPath = _hostingEnvironment.WebRootPath;
                    string uploadPath = Path.Combine(wwwRootPath, "Uploads");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string extension = Path.GetExtension(imagen.FileName);
                    string nombreArchivoUnico = Guid.NewGuid().ToString() + extension;
                    string filePath = Path.Combine(uploadPath, nombreArchivoUnico);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(fileStream);
                    }

                    inmueble.ImagenUrl = $"/Uploads/{nombreArchivoUnico}";
                }

                var idUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                inmueble.IdUsuario = idUsuario;
                _repositorioInmuebles.Modificacion(inmueble);
                return Ok(inmueble);
            }
            catch
            {
                return BadRequest();
            }
        }

        // ruta: /api/Auth/misInquilinos
        [HttpGet("misInquilinos")]
        [Authorize(Policy = "EsPropietarioApp")]
        public IActionResult misInquilinos()
        {
            try
            {
                var idPropietario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var inquilinos = _repositorioInquilino.ObtenerInquilinosDePropietario(idPropietario);
                return Ok(inquilinos);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
