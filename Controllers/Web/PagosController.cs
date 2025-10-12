using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
namespace Inmobiliaria.Controllers{
    public class PagosController : Controller
    {
        private readonly RepositorioPagos repo;
        private readonly RepositorioMultas repositorioMultas;
        private readonly RepositorioContratos repositorioContratos;
         private readonly RepositorioInmuebles repositorioInmuebles;
        public PagosController(IConfiguration configuration)
        {
            repo = new RepositorioPagos(configuration);
            repositorioMultas = new RepositorioMultas(configuration);
            repositorioContratos = new RepositorioContratos(configuration);
            repositorioInmuebles = new RepositorioInmuebles(configuration);
        }

        

        [Authorize]
        public IActionResult Index(int pageNumber = 1, int pageSize = 5)

        {
           
           var lista = repo.ObtenerPaginado(pageNumber, pageSize);
                var totalRegistros = repo.ContarPagos();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / pageSize);

               
            // Asigna al pago la direccion del inmueble 
            foreach (var pago in lista)
            {
                var contrato = repositorioContratos.obtenerDireccionPrecioInmueblePorIdContrato(pago.IdContratos);

                pago.DireccionInmueble = contrato != null ? contrato.DireccionInmueble : "";
                // pago.Importe = contrato != null ? (contrato.Precio ?? 0) : 0;
            }
              ViewBag.PaginaActual = pageNumber;
              ViewBag.TotalPaginas = totalPaginas;
            return View(lista);
        }

        


        [Authorize]
        public IActionResult Create(int id,bool multa)

        {
            Pagos pago=new Pagos();

            if (multa)
            {
                var mult = repositorioMultas.ObtenerPorId(id);
                if (mult == null)
                {
                    return NotFound("No se encontro ninguna multa para pagar");
                }
                var contrato = repositorioContratos.ObtenerPorId(mult.IdContrato);

                if (contrato == null)
                {
                    return NotFound("No se encontro ningun contrato para ese pago");
                }
                var inm = repositorioInmuebles.ObtenerPorId(contrato.IdInmuebles);
                if (inm == null)
                {
                    return NotFound("No se encontro ningun Inmueble");
                }

                mult.DireccionInmueble = inm != null ? inm.Direccion : "";
                // armar el pago acorde a la multa pero ligado a un contrato
                pago.IdContratos = contrato.IdContrato;
                pago.DireccionInmueble = mult.DireccionInmueble;
                pago.Importe = mult.ImporteMulta.Value;
                pago.FechaPago = DateTime.Now;
                pago.NumeroCuota = contrato.CantidadCuotas + 1;
                pago.MesPago = mult.FechaMulta.Month;
                pago.Concepto = "Multa por retiro anticipado";
                if (!mult.Pagada)
                {
                    return NotFound("Esta multa ya fue pagada");
                }
                pago.Multa = true;

            }
            else
            {
                var contrato = repositorioContratos.ObtenerPorId(id);

                if (contrato == null)
                {
                    return NotFound("No se encontro ningun contrato para ese pago");
                }
                var inm = repositorioInmuebles.ObtenerPorId(contrato.IdInmuebles);
                if (inm == null)
                {
                    return NotFound("No se encontro ningun Inmueble");
                }
                contrato.DireccionInmueble = inm != null ? inm.Direccion : "";
                // pago = new Pagos();
                pago.IdContratos = id;
                pago.DireccionInmueble = contrato.DireccionInmueble;
                pago.Importe = contrato.Monto;
                pago.FechaPago = DateTime.Now;
                pago.NumeroCuota = contrato.CuotasPagas + 1;
                pago.MesPago = contrato.MesInicio + pago.NumeroCuota - 1;
                if (pago.MesPago > 12) pago.MesPago = pago.MesPago - 12;
                pago.Concepto = "Alquiler mes :" + ((enMeses)pago.MesPago).ToString();
                if (pago.NumeroCuota > contrato.CantidadCuotas)
                {
                    return NotFound("Ya fueron realizados todo los pagos para este contrato");
                }
                pago.Multa = false;
            }
            
          
            return View(pago);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pagos pago)
        {

            if (ModelState.IsValid)
            {
                if (!pago.Multa.Value)
                {
                    var contr = repositorioContratos.ObtenerPorId(pago.IdContratos);
                    if (contr == null)
                    {
                        return NotFound("No se encontro ningun contrato para realizar el pago");
                    }
                    pago.UsuariAlta = int.Parse(User.FindFirst("UserId")?.Value);
                    repo.Alta(pago);
                    contr.CuotasPagas = pago.NumeroCuota;
                    repositorioContratos.Modificacion(contr);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var contrato = repositorioContratos.ObtenerPorId(pago.IdContratos);
                    if (contrato == null)
                    {
                        return NotFound("No se encontro ningun contrato para realizar el pago");
                    }
                    var multa = repositorioMultas.ObtenerPorIdContrato(contrato.IdContrato);
                    if (multa == null)
                    {
                        return NotFound("No se encontro ninguna multa para realizar el pago");
                    }
                    pago.UsuariAlta = int.Parse(User.FindFirst("UserId")?.Value);
                    repo.Alta(pago);
                    multa.Pagada = false;
                    repositorioMultas.Modificacion(multa);
                    return RedirectToAction(nameof(Index));
               }   

            }
    
            return View(pago);
        }


        [Authorize]
        public IActionResult Edit(int id)
        {
            var pago=repo.ObtenerPorId(id);
            if(pago==null){
                return NotFound("No se encontro ningun pago");
            }
             var contrato=repositorioContratos.ObtenerPorId(pago.IdContratos);

            if (contrato == null)
            {
                return NotFound("No se encontro ningun contrato ligado a ese pago");
            }
             var inm = repositorioInmuebles.ObtenerPorId(contrato.IdInmuebles);
            contrato.DireccionInmueble = inm != null ? inm.Direccion : "";
           var pa=new Pagos();
            pa.IdPagos = pago.IdPagos;
              pa.IdContratos=contrato.IdContrato;
              pa.DireccionInmueble=contrato.DireccionInmueble;
              pa.Importe=pago.Importe;
              pa.FechaPago=pago.FechaPago;
              pa.NumeroCuota = pago.NumeroCuota;
              pa.MesPago=pago.MesPago;
              pa.Concepto=pago.Concepto ;
            
            return View(pa);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Pagos pago)
        {


            if (id != pago.IdPagos)
            {
                return NotFound("Hay una inconsistecia en el pago enviado");
            }


            if (ModelState.IsValid)
            {

                repo.Modificacion(pago);
                return RedirectToAction(nameof(Index));
            }
            
            return View(pago);
        }


        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var pago = repo.ObtenerPorId(id);
            if (pago == null)
            {
                return NotFound("No se encontro ningun pago");
            }
            return View(pago);
        }

        [Authorize(Roles ="Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, String bandera)

        {
            var pago = repo.ObtenerPorId(id);
            var contrato = repositorioContratos.ObtenerPorId(pago.IdContratos);
            repositorioContratos.RestarCuotaPaga(contrato);
            if (pago == null)
            {
                return NotFound("No se encontro ningun pago");
            }
            if (id != pago.IdPagos)
            {
                 return NotFound("Hay una inconsistencia en el pago");
            }
               

            if (ModelState.IsValid)
            {
                pago.UsuarioBaja = int.Parse(User.FindFirst("UserId")?.Value);
                repo.Baja(pago);
                return RedirectToAction(nameof(Index));
            }
            return View(pago);
        }


        [Authorize]
        public IActionResult PorInquilino(int? id)
        {
            if (id == null || id <= 0)
            {
                return BadRequest();
            }

            var lista = repo.obtenerPorInquilino(id.Value);

            //Console.WriteLine($"ID: {id.Value} - Cantidad de pagos: {lista.Count}");

            // Asigna al pago la direccion del inmueble 
            foreach (var pago in lista)
            {
                var contrato = repositorioContratos.obtenerDireccionPrecioInmueblePorIdContrato(pago.IdContratos);

                pago.DireccionInmueble = contrato != null ? contrato.DireccionInmueble : "";
                pago.Importe = contrato != null ? (contrato.Precio ?? 0) : 0;
            }
            ViewBag.IdInquilino = id.Value;
            return View("PorInquilino", lista);
        }

    }

}