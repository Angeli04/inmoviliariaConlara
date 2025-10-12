using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
namespace Inmobiliaria.Controllers{
    public class InmueblesController : Controller
    {
        private readonly RepositorioInmuebles repo;
        private readonly RepositorioTipoInmueble repositorioTipoInmueble;
        private readonly RepositorioPropietario repoPropietario;
        public InmueblesController(IConfiguration configuration)
        {
            repo = new RepositorioInmuebles(configuration);
            repositorioTipoInmueble = new RepositorioTipoInmueble(configuration);
            repoPropietario = new RepositorioPropietario(configuration);
        }

        [Authorize]
        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();

            // Asigna el propietario a cada inmueble
            foreach (var inmueble in lista)
            {
                inmueble.Duenio = repoPropietario.ObtenerPorId(inmueble.IdPropietario);
                inmueble.TipoInmueble = repositorioTipoInmueble.ObtenerPorId(inmueble.IdTipoInmueble)?.Nombre;
            }

            return View(lista);
        }

        [Authorize]
        public IActionResult Create()
        {
            var tiposInmuebles = repositorioTipoInmueble.ObtenerTodos();
            ViewBag.TipoInmuebles = tiposInmuebles;
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Inmuebles inmueble)

        {

            var inm = repo.ObtenerPorDireccion(inmueble.Direccion);
            if (inm != null)
            {
                ModelState.AddModelError("Direccion", "Ya existe un inmueble con esa dirección.");
                ViewBag.TipoInmuebles = repositorioTipoInmueble.ObtenerTodos();
                return View(inmueble);
            }
            if (ModelState.IsValid)
            {
                inmueble.Habilitado = true;
                repo.Alta(inmueble);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.TipoInmuebles = repositorioTipoInmueble.ObtenerTodos();
            return View(inmueble);
        }


        [Authorize]
        public IActionResult Edit(int id)
        {
            var inmueble = repo.ObtenerPorId(id);
            if (inmueble == null)
            {
                return NotFound("No se encontro ningun Inmueble para editar");
            }
            inmueble.Duenio = repoPropietario.ObtenerPorId(inmueble.IdPropietario);
            inmueble.TipoInmueble = repositorioTipoInmueble.ObtenerPorId(inmueble.IdTipoInmueble)?.Nombre;

            ViewBag.TipoInmuebles = repositorioTipoInmueble.ObtenerTodos();
            return View(inmueble);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Inmuebles inmueble)
        {

            if (id != inmueble.IdInmuebles)
            {
                return NotFound("No se encontro ningun inmueble para editar");
            }

            if (ModelState.IsValid)
            {
                repo.Modificacion(inmueble);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.TipoInmuebles = repositorioTipoInmueble.ObtenerTodos();
            return View(inmueble);
        }


        [Authorize(Roles="Administrador")]
        public IActionResult Delete(int id)
        {
            var inmueble = repo.ObtenerPorId(id);
            if (inmueble == null)
            {
                return NotFound();
            }
            return View(inmueble);
        }



        [Authorize(Roles="Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, String bandera)
        {
            var inmueble = repo.ObtenerPorId(id);
            if (inmueble == null)
            {
                return NotFound();
            }
            if (id != inmueble.IdInmuebles)
                return NotFound();

            if (ModelState.IsValid)
            {
                repo.Baja(inmueble);
                return RedirectToAction(nameof(Index));
            }
            return View(inmueble);
        }


        [Authorize]
        public IActionResult BuscarInmueblePorFraccionDireccion(String term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 3)
            {
                return Json(new { success = false, data = new List<object>() });
            }

            var lista = repo.BuscarPorFraccionDireccion(term);
            if (lista == null || lista.Count == 0)
            {
                return Json(new { success = false, message = "No se encontraron Inmuebles." });
            }
            var resultado = lista.Select(i => new
            {
                id = i.IdInmuebles,
                direccion = i.Direccion,
                precio = i.Precio
            });

            return Json(new { success = true, data = resultado });
        }


        
        
        [Authorize]
        public IActionResult PorPropietario(int? id)
        {

            if (id == null || id <= 0)
            {
                return BadRequest();
            }
            var lista = repo.ObtenerPorPropietario(id.Value);

            // Asigna el propietario a cada inmueble
            foreach (var inmueble in lista)
            {
                inmueble.Duenio = repoPropietario.ObtenerPorId(inmueble.IdPropietario);
                inmueble.TipoInmueble = repositorioTipoInmueble.ObtenerPorId(inmueble.IdTipoInmueble)?.Nombre;
            }
           // Console.WriteLine("Propietario ID: " + id);
            ViewBag.Propietario = repoPropietario.ObtenerPorId(id.Value);
            return View("PorPropietario", lista);
        }
        public IActionResult Habilitados()
        {
            var lista = repo.ObtenerTodosDisponibles();

            // Asigna el propietario a cada inmueble
            foreach (var inmueble in lista)
            {
                inmueble.Duenio = repoPropietario.ObtenerPorId(inmueble.IdPropietario);
                inmueble.TipoInmueble = repositorioTipoInmueble.ObtenerPorId(inmueble.IdTipoInmueble)?.Nombre;
            }

            return View("Index", lista);
        }
         [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuscarDesocupados(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var lista = repo.ObtenerTodos();

            // Asigna el propietario a cada inmueble
            foreach (var inmueble in lista)
            {
                inmueble.Duenio = repoPropietario.ObtenerPorId(inmueble.IdPropietario);
                inmueble.TipoInmueble = repositorioTipoInmueble.ObtenerPorId(inmueble.IdTipoInmueble)?.Nombre;
            }
            if (!fechaDesde.HasValue || !fechaHasta.HasValue)
            {
                ModelState.AddModelError("", "Debe ingresar ambas fechas.");
                return View("Index",lista);
            }
            // Validar rango
            if (fechaDesde > fechaHasta)
            {
                ModelState.AddModelError("", "La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.");
                return View("Index",lista);
            }

            
                var desocupados = repo.BuscarDesocupados(fechaDesde, fechaHasta);
                 foreach (var inmueble in desocupados)
            {
                inmueble.Duenio = repoPropietario.ObtenerPorId(inmueble.IdPropietario);
                inmueble.TipoInmueble = repositorioTipoInmueble.ObtenerPorId(inmueble.IdTipoInmueble)?.Nombre;
            }

                return View("Index", desocupados);
            
        }

}
}