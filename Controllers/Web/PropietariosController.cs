using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria.Controllers
{
    public class PropietariosController : Controller
    {
        private readonly RepositorioPropietario repo;

        public PropietariosController(IConfiguration configuration)
        {
            repo = new RepositorioPropietario(configuration);
        }

        [Authorize]
        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();
            return View(lista);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Propietario propietario)
        {
            var p=repo.ObtenerPorDni(propietario.Dni);
            if (p != null)
            {
                ModelState.AddModelError("Dni", "Ya existe un propietario con este DNI.");
                return View(propietario);
            }
            if (ModelState.IsValid)
            {
                repo.Alta(propietario);
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        [Authorize]
        public IActionResult Edit(int id)
        {
            var propietario = repo.ObtenerPorId(id);
            if (propietario == null)
            {
                return NotFound();
            }
            return View(propietario);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Propietario propietario)
        {

             var existente = repo.ObtenerPorDni(propietario.Dni);
            if (existente != null && existente.IdPropietario != propietario.IdPropietario)
            {
                ModelState.AddModelError("Dni", "Ya existe otro propietario con este DNI.");
                return View(propietario);
            }

            if (id != propietario.IdPropietario)
                return NotFound();

            if (ModelState.IsValid)
            {
                repo.Modificacion(propietario);
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        [Authorize(Roles="Administrador")]
        public IActionResult Delete(int id)
        {
            var propietario = repo.ObtenerPorId(id);
            if (propietario == null)
            {
                return NotFound();
            }
            return View(propietario);
        }

        [Authorize(Roles="Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {

            var propietario = repo.ObtenerPorId(id);

            repo.Baja(propietario);
            return RedirectToAction(nameof(Index));
        }



        [Authorize]
        [HttpGet]
        public IActionResult BuscarPropietarioPorFraccionApellido( String term)
        {
              if (string.IsNullOrEmpty(term) || term.Length < 3)
                    {
                        return Json(new { success = false,message = "Ingrese al menos tres caracteres", data = new List<object>() });
                    }

                 var lista = repo.BuscarPorFraccionApellido(term);
                    if (lista == null || lista.Count == 0)
                        {
                            return Json(new { success = false, message = "No se encontraron Propietarios.", data = new List<object>() });
                        }
                 var resultado = lista.Select(p => new
                        {
                            id = p.IdPropietario,
                            texto = p.ToString() // Usa el override
                        });

                return Json(new { success = true, data = resultado });
        }  
    }
}
