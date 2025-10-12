using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
namespace Inmobiliaria.Controllers{
    public class MultasController : Controller
    {
        private readonly RepositorioMultas repo;

        private readonly RepositorioContratos repositorioContratos;
        private readonly RepositorioInmuebles repositorioInmuebles;
        public MultasController(IConfiguration configuration)
        {
            repo = new RepositorioMultas(configuration);

            repositorioContratos = new RepositorioContratos(configuration);
            repositorioInmuebles = new RepositorioInmuebles(configuration);
        }
        [Authorize]
        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();


            foreach (var multa in lista)
            {
                var contrato = repositorioContratos.obtenerDireccionPrecioInmueblePorIdContrato(multa.IdContrato);
                multa.DireccionInmueble = contrato != null ? contrato.DireccionInmueble : "";
                multa.ImporteCuota = contrato != null ? (contrato.Precio ?? 0) : 0;
            }

            return View(lista);
        }
        [Authorize]
        public IActionResult Create(int id)
        {
            var contrato = repositorioContratos.ObtenerPorId(id);

            if (contrato == null)
            {
                return NotFound("No se encontro ningun contrato para realizar la multa");
            }

            var inm = repositorioInmuebles.ObtenerPorId(contrato.IdInmuebles);
            contrato.DireccionInmueble = inm != null ? inm.Direccion : "";
            var multa = new Multas();
            multa.IdContrato = id;
            multa.DireccionInmueble = contrato.DireccionInmueble;
            multa.ImporteCuota = contrato.Monto;
            multa.FechaMulta = DateTime.Now;
            multa.FechaHastaContrato = contrato.FechaHasta;


            return View(multa);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Multas multa)
        {
      
            if (ModelState.IsValid)
            {

                var contr = repositorioContratos.ObtenerPorId(multa.IdContrato);
                if(contr==null)
                {
                    return NotFound("No se encontroningun contrato para realizar la multa");
                }
                multa.Pagada = false;
                multa.UsuariAlta = int.Parse(User.FindFirst("UserId")?.Value);
                repo.Alta(multa);
                contr.CuotasPagas =contr.CantidadCuotas;
                repositorioContratos.Modificacion(contr);
                return RedirectToAction(nameof(Index));
                
               
            }
             return View(multa);
        }
        [Authorize]
        public IActionResult Calcular(int id)
        {
            var contrato = repositorioContratos.ObtenerPorId(id);

            if (contrato == null)
            {
                return NotFound("No se encontro ningun contrato para realizar la multa");
            }

            var inm = repositorioInmuebles.ObtenerPorId(contrato.IdInmuebles);
            contrato.DireccionInmueble = inm != null ? inm.Direccion : "";
            var multa = new Multas();
            multa.IdContrato = id;
            multa.DireccionInmueble = contrato.DireccionInmueble;
            multa.ImporteCuota = contrato.Monto;
            multa.FechaMulta = DateTime.Now;
            multa.FechaHastaContrato = contrato.FechaHasta;
            multa.NuevaFechaHastaContrato = contrato.FechaHasta;
            multa.ImporteMulta = 0;
            return View(multa);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Calcular(Multas multa)
        {
            var contrato = repositorioContratos.ObtenerPorId(multa.IdContrato);
            if (contrato == null)
            {
                return NotFound("No se encontro ninguncontratopara realizar la multa");
            }
            decimal? montoProvisorio;
            if (PasoMasDeLaMitad(contrato.FechaDesde, contrato.FechaHasta, multa.NuevaFechaHastaContrato))
            {
                montoProvisorio = contrato.Monto;
            }
            else
            {
                montoProvisorio = contrato.Monto * 2;
            }
            int cantCuotasProvisoria = (multa.NuevaFechaHastaContrato.Year - contrato.FechaDesde.Year) * 12 +
            (multa.NuevaFechaHastaContrato.Month - contrato.FechaDesde.Month)
            + 1; // incluir ambos meses

            int cuotasAdeudadas = cantCuotasProvisoria - contrato.CuotasPagas;
            decimal? prov = montoProvisorio + (cuotasAdeudadas * contrato.Monto);
            multa.ImporteMulta = prov;
            multa.CuotasAdeudadas = cuotasAdeudadas;
        
            ModelState.Clear();
            return View("Calculado", multa);
        }
        [Authorize]
          public IActionResult Edit(int id)
        {
            var multa = repo.ObtenerPorId(id);
            if (multa == null)
            {
                return NotFound("No se encontro ninguna multa");
            }
            var contrato = repositorioContratos.ObtenerPorId(multa.IdContrato);

            if (contrato == null)
            {
                return NotFound("No se encontro ningun contrato ligado a esa multa");
            }
            var inm = repositorioInmuebles.ObtenerPorId(contrato.IdInmuebles);
            contrato.DireccionInmueble = inm != null ? inm.Direccion : "";
            multa.DireccionInmueble = contrato.DireccionInmueble;

            return View(multa);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit( Multas multa)
        {
            var mult = repo.ObtenerPorId(multa.IdMulta);  

            if (mult==null)
            {
                return NotFound("No se encontro nunguna multa para editar");
            }


            if (ModelState.IsValid)
            {

                repo.Modificacion(multa);
                return RedirectToAction(nameof(Index));
            }
            
            return View(multa);
        }
        [Authorize("Administrador")]
        public IActionResult Delete(int id)
        {
            var multa = repo.ObtenerPorId(id);
            if (multa == null)
            {
                return NotFound("No se encontro ninguna multa");
            }
            return View(multa);
        }
        [Authorize(Roles="Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, String bandera)

        {
            var multa = repo.ObtenerPorId(id);
            if (multa == null)
            {
                return NotFound("No se encontro ninguna multa");
            }
            if (id != multa.IdMulta)
            {
                 return NotFound("Hay una inconsistencia en la multa");
            }
               

            if (ModelState.IsValid)
            {
                repo.Baja(multa);
                return RedirectToAction(nameof(Index));
            }
            return View(multa);
        }


        static bool PasoMasDeLaMitad(DateTime ingreso, DateTime egreso, DateTime fecha)
        {
            // Calcular diferencia total en meses
            int totalMeses = (egreso.Year - ingreso.Year) * 12 + egreso.Month - ingreso.Month;

            // Mitad del período en meses
            double mitadMeses = totalMeses / 2.0;

            // Meses transcurridos desde el ingreso hasta la fecha
            int mesesTranscurridos = (fecha.Year - ingreso.Year) * 12 + fecha.Month - ingreso.Month;

            // Comparar si pasó más de la mitad
            return mesesTranscurridos > mitadMeses;
        }
        

    }
    }