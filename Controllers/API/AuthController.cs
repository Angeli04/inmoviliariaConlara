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

        private readonly RepositorioContratos _repositorioContratos;

        private readonly RepositorioPagos _repositorioPagos;

        public AuthController(RepositorioUsuario repositorioUsuario, IConfiguration configuration, RepositorioInmuebles repositorioInmuebles, RepositorioTipoInmueble repositorioTipoInmueble, IWebHostEnvironment hostingEnvironment, RepositorioInquilino repositorioInquilino, RepositorioContratos repositorioContratos, RepositorioPagos repositorioPagos)
        {
            _repositorioUsuario = repositorioUsuario;
            _configuration = configuration;
            _repositorioInmuebles = repositorioInmuebles;
            _repositorioTipoInmueble = repositorioTipoInmueble;
            _hostingEnvironment = hostingEnvironment;
            _repositorioInquilino = repositorioInquilino;
            _repositorioContratos = repositorioContratos;
            _repositorioPagos = repositorioPagos;

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

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var idPropietario = Convert.ToInt32(userIdClaim.Value);


            bool habilitado = inmueble.Habilitado;
            int idInmueble = inmueble.IdInmuebles;


            Inmuebles inmuebleActualizado = _repositorioInmuebles.Habilitar(idInmueble, habilitado, idPropietario);


            if (inmuebleActualizado == null)
            {
                return NotFound(new { message = "No se encontró el inmueble o no tiene permisos sobre él." });
            }

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
                inmueble.Habilitado = false;
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // ruta: /api/Auth/contratosVigentes
        [HttpGet("contratosVigentes")]
        [Authorize(Policy = "EsPropietarioApp")]
        public IActionResult ContratosVigentes()
        {
            try
            {
                var idPropietario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var contratos = _repositorioContratos.ObtenerContratosVigentesApi(idPropietario);
                return Ok(contratos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ruta: api/Auth/porcontrato/{idContrato}
        [HttpGet("porcontrato/{idContrato}")]
        [Authorize(Policy = "EsPropietarioApp")]
        public IActionResult PorContrato(int idContrato)
        {
            try
            {
                var idPropietario = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var contrato = _repositorioContratos.ObtenerPorId(idContrato);
                if (contrato == null)
                {
                    return Forbid("No tiene permisos para ver este contrato");
                }
                var pagos = _repositorioPagos.ObtenerPagosPorContrato(idContrato);
                return Ok(pagos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        // ruta: /api/Auth/cambiar-clave
        [HttpPut("cambiar-clave")]
        [Authorize(Policy = "EsPropietarioApp")]
        public IActionResult CambiarClave([FromBody] CambioClaveRequest request)
        {
            try
            {
        
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }
                var idUsuario = Convert.ToInt32(userIdClaim.Value);
                bool exito = _repositorioUsuario.CambiarClave(
                    idUsuario, 
                    request.ClaveActual, 
                    request.ClaveNueva
                );

                if (exito)
                {
                    return Ok(new { message = "Contraseña actualizada correctamente." });
                }
                else
                {

                    return BadRequest(new { message = "La contraseña actual es incorrecta." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error inesperado.", error = ex.Message });
            }
        }


    }
}
