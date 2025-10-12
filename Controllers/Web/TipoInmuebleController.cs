using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
namespace Inmobiliaria.Controllers{
    public class TipoInmuebleController : Controller
    {
        private readonly RepositorioTipoInmueble repo;

        public TipoInmuebleController(IConfiguration configuration)
        {
            repo = new RepositorioTipoInmueble(configuration);
        }

        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();
            return View(lista);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TipoInmueble tipoInmueble)
        {
            if (ModelState.IsValid)
            {
                repo.Alta(tipoInmueble);
                return RedirectToAction(nameof(Index));
            }
            return View(tipoInmueble);
        }

        public IActionResult Edit(int id)
        {
            var tipoInmueble = repo.ObtenerPorId(id);
            if (tipoInmueble == null)
            {
                return NotFound();
            }
            return View(tipoInmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, TipoInmueble tipoInmueble)
        {
            if (id != tipoInmueble.IdTipoInmueble)
                return NotFound();

            if (ModelState.IsValid)
            {
                repo.Modificacion(tipoInmueble);
                return RedirectToAction(nameof(Index));
            }
            return View(tipoInmueble);
        }
        public IActionResult Delete(int id)
        {
            var tipoInmueble = repo.ObtenerPorId(id);
            if (tipoInmueble == null)
            {
                return NotFound();
            }
            return View(tipoInmueble);
        }
          [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id,String bandera)
        {
            var tipoInmueble = repo.ObtenerPorId(id);
            if (tipoInmueble == null)
            {
                return NotFound();
            }
            if (id != tipoInmueble.IdTipoInmueble)
                return NotFound();

            if (ModelState.IsValid)
            {
                repo.Baja(tipoInmueble);
                return RedirectToAction(nameof(Index));
            }
            return View(tipoInmueble);
        }
}
}