/*using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoInmuebleApiController : ControllerBase
    {
        private readonly RepositorioTipoInmueble _repositorio;

        public TipoInmuebleApiController(IConfiguration configuration)
        {

            _repositorio = new RepositorioTipoInmueble(configuration);
        }

        // ruta: api/TipoInmuebleApi/listar
        [HttpGet("listar")]
        [Authorize(Policy = "esPropietario")]
        public IActionResult Listar()
        {
            var res = _repositorio.ObtenerTodos();
            return Ok(res);
        }



    }
}
*/