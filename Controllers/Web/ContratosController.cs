using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
namespace Inmobiliaria.Controllers;
    public class ContratosController : Controller
    {
        private readonly RepositorioContratos repo;
        private readonly RepositorioInquilino repositorioInquilino;
        private readonly RepositorioInmuebles repositorioInmuebles;
        public ContratosController(IConfiguration configuration)
        {
            repo = new RepositorioContratos(configuration);
            repositorioInquilino = new RepositorioInquilino(configuration);
            repositorioInmuebles = new RepositorioInmuebles(configuration);
        }

        [Authorize]
        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();

            // Asigna el contrato el nombre del inquilino , la direccion del inmueble y el precio del inmueble a cada contrato
            foreach (var contrato in lista)
            {
                if (contrato.FechaHasta < DateTime.Now)
                {
                    contrato.Vigente = false;
                    repo.Modificacion(contrato);
                }
                if (contrato.IdInquilino.HasValue)
                {
                    Inquilino nI = repositorioInquilino.ObtenerPorId(contrato.IdInquilino.Value);
                    contrato.NombreInquilino = nI != null ? nI.ToString() : "";
                }
                else
                {
                    contrato.NombreInquilino = "";
                }
                //  contrato.NombreInquilino=RepositorioInquilino.ObtenerPorId(contrato.IdInquilino).ToString() ?? "";
                var inm = repositorioInmuebles.ObtenerPorId(contrato.IdInmuebles);
                contrato.DireccionInmueble = inm != null ? inm.Direccion : "";
                contrato.Precio = inm != null ? inm.Precio : 0;
            }

            return View(lista);
        }


        [Authorize]
        public IActionResult Create()
        {
            // var contratos = repositorioTipoInmueble.ObtenerTodos();
            //ViewBag.TipoInmuebles = contratos;
            return View();
        }



        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Contratos contrato)
        {


        contrato.Existe = true;
        contrato.UsuariAlta = int.Parse(User.FindFirst("UserId")?.Value);

        if (ModelState.IsValid)
        {
            var contratos = repo.ObtenerTodosPoIdInmueble(contrato.IdInmuebles);
            if (contratos.Count == 0)
            {
                contrato.Vigente = true;
                contrato.MesInicio = contrato.MesInicio + 2;
                if (contrato.MesInicio > 12)
                {
                    contrato.MesInicio = 1;
                }
                repo.Alta(contrato);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var i = repo.verificarContratoSolapado(contrato);
                if (i == 1)
                {
                    ModelState.AddModelError("FechaDesde", "Ya existe un contrato vigente en ese mes");
                    return View(contrato);
                }
                if (i == 2)
                {
                    ModelState.AddModelError("FechaHasta", "Ya existe un contrato vigente en ese mes");
                    return View(contrato);
                }
            }

            contrato.Vigente = true;
            repo.Alta(contrato);
            return RedirectToAction(nameof(Index));

        }
            //ViewBag.TipoInmuebles = repositorioTipoInmueble.ObtenerTodos();
            return View(contrato);
        }


        [Authorize(Roles="Administrador")]
        public IActionResult Edit(int id)
        {
            var contrato = repo.ObtenerPorId(id);

            if (contrato == null)
            {
                return NotFound("No se encontro ningun contrato para editar");
            }
            if (contrato.IdInquilino.HasValue)
            {
                Inquilino nI = repositorioInquilino.ObtenerPorId(contrato.IdInquilino.Value);
                contrato.NombreInquilino = nI != null ? nI.ToString() : "";
            }
            else
            {
                contrato.NombreInquilino = "";
            }
            //  contrato.NombreInquilino=RepositorioInquilino.ObtenerPorId(contrato.IdInquilino).ToString() ?? "";
            var inm = repositorioInmuebles.ObtenerPorId(contrato.IdInmuebles);
            contrato.DireccionInmueble = inm != null ? inm.Direccion : "";
            contrato.Precio = inm != null ? inm.Precio : 0;

            // ViewBag.TipoInmuebles = repositorioTipoInmueble.ObtenerTodos();
            return View(contrato);
        }



        [Authorize(Roles="Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Contratos contrato)
        {


            if (id != contrato.IdContrato)
            {
                return NotFound("Hay una inconsistencia en el contrato");
            }


            if (ModelState.IsValid)
            { 
                 var contratos = repo.ObtenerTodosPoIdInmueble(contrato.IdInmuebles);
                if (contratos.Count == 0)
                {
                     contrato.Vigente = true;
                    repo.Modificacion(contrato);
                    return RedirectToAction(nameof(Index));
                }else
                {
                    var i = repo.verificarContratoSolapado(contrato);
                    if (i == 1)
                    {
                        ModelState.AddModelError("FechaDesde", "Ya existe un contrato vigente en ese mes");
                            return View(contrato);
                    }
                    if (i == 2)
                    {
                         ModelState.AddModelError("FechaHasta", "Ya existe un contrato vigente en ese mes");
                         return View(contrato);
                    }
                 }
                contrato.Vigente = true;
                repo.Modificacion(contrato);
                return RedirectToAction(nameof(Index));
            }
            // ViewBag.TipoInmuebles = repositorioTipoInmueble.ObtenerTodos();
            return View(contrato);
        }

        [Authorize(Roles="Administrador")]
        public IActionResult Delete(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null)
            {
                return NotFound();
            }

            return View(contrato);
        }

        [Authorize(Roles="Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, String bandera)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null)
            {
                return NotFound();
            }
            if (id != contrato.IdContrato)
                return NotFound();

            if (ModelState.IsValid)
            {
                contrato.Existe = false;
                contrato.UsuarioBaja = int.Parse(User.FindFirst("UserId")?.Value);
                repo.Baja(contrato);

                return RedirectToAction(nameof(Index));
            }

            return View(contrato);
        }

        [Authorize]
        public IActionResult Renovar(int id)
        {
            var contrato = repo.ObtenerPorId(id);

            if (contrato == null)
            {
                return NotFound("No se encontro ningun contrato para renovar");
            }
            if (contrato.IdInquilino.HasValue)
            {
                Inquilino nI = repositorioInquilino.ObtenerPorId(contrato.IdInquilino.Value);
                contrato.NombreInquilino = nI != null ? nI.ToString() : "";
            }
            else
            {
                contrato.NombreInquilino = "";
            }
            //  contrato.NombreInquilino=RepositorioInquilino.ObtenerPorId(contrato.IdInquilino).ToString() ?? "";
            var inm = repositorioInmuebles.ObtenerPorId(contrato.IdInmuebles);
            contrato.DireccionInmueble = inm != null ? inm.Direccion : "";
            contrato.Precio = inm != null ? inm.Precio : 0;
            contrato.Monto = inm != null ? inm.Precio : 0;
            contrato.CantidadCuotas = 0;
            contrato.CuotasPagas = 0;
            contrato.FechaDesde=contrato.FechaHasta;
            
            //contrato.FechaHasta = DateTime.Now; 
            // ViewBag.TipoInmuebles = repositorioTipoInmueble.ObtenerTodos();
            return View(contrato);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Renovar(int id, Contratos contrato)
        {

            if (ModelState.IsValid)
            { 
                 var contratos = repo.ObtenerTodosPoIdInmueble(contrato.IdInmuebles);
                 
                if (contratos.Count == 0)
            {
                contrato.Vigente = true;
                contrato.Existe = true;
                contrato.UsuariAlta = int.Parse(User.FindFirst("UserId")?.Value);
                repo.Alta(contrato);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var i = repo.verificarContratoSolapado(contrato);
                if (i == 1)
                {
                    ModelState.AddModelError("FechaDesde", "Ya existe un contrato vigente en ese mes");
                    return View(contrato);
                }
                if (i == 2)
                {
                    ModelState.AddModelError("FechaHasta", "Ya existe un contrato vigente en ese mes");
                    return View(contrato);
                }
            }
                contrato.Existe = true;
                contrato.UsuariAlta = int.Parse(User.FindFirst("UserId")?.Value); // TODO: intentar simplificar
                contrato.Vigente = true;
                repo.Alta(contrato);
                return RedirectToAction(nameof(Index));
            }
            // ViewBag.TipoInmuebles = repositorioTipoInmueble.ObtenerTodos();
            return View(contrato);
        }



    [Authorize]
    public IActionResult buscarPorFraccionDireccion(String term)
    { if (string.IsNullOrEmpty(term) || term.Length < 3)
        {
            return Json(new { success = false, data = new List<object>() });
        }

        var lista = repo.buscarPorFraccionDireccion(term);

        var resultado = lista.Select(i => new
        {
            id = i.IdContrato,
            Direccion = i.DireccionInmueble,
            Precio = i.Precio
        });

        return Json(new { success = true, data = resultado });
    }   
    /*private int compararFechas(Contratos contrato, IList<Contratos> contratos)
    {
        if (contrato == null) throw new ArgumentNullException(nameof(contrato));

        foreach (var contra in contratos)
        {
            if (contra.IdContrato == contrato.IdContrato) 
                continue; // no comparar con sí mismo

            if (contra.CantidadCuotas <= 0) 
                continue;

            // Empezamos desde la fecha de inicio y avanzamos mes a mes
            DateTime current = contra.FechaDesde.Date;
            for (int i = 0; i < contra.CantidadCuotas; i++)
            {
                if (current.Year == contrato.FechaDesde.Year && current.Month == contrato.FechaDesde.Month)
                    return 1; // coincide con FechaDesde del contrato
                if (current.Year == contrato.FechaHasta.Year && current.Month == contrato.FechaHasta.Month)
                    return 2; // coincide con FechaHasta del contrato

                current = current.AddMonths(1);
            }
        }
        return 3; // sin coincidencias
    }*/

}