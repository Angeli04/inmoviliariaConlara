using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
namespace Inmobiliaria.Controllers;

public class AuditoriaController : Controller
{
    private readonly RepositorioPagos repositorioPagos;
    private readonly RepositorioMultas repositorioMulta;
    private readonly RepositorioContratos repositorioContrato;
    private readonly RepositorioInquilino repositorioInquilino;
        private readonly RepositorioInmuebles repositorioInmueble;
    public AuditoriaController(IConfiguration configuration)
    {
        repositorioPagos = new RepositorioPagos(configuration);
        repositorioMulta = new RepositorioMultas(configuration);
        repositorioContrato = new RepositorioContratos(configuration);
        repositorioInquilino = new RepositorioInquilino(configuration);
        repositorioInmueble = new RepositorioInmuebles(configuration);
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Index()
    {
        return View();
    }


    [Authorize(Roles = "Administrador")]
    public IActionResult Pagos()
    {
        var pagos = repositorioPagos.ObtenerTodosTodos();
        return View(pagos);
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Contratos()
    {
        var contratos = repositorioContrato.ObtenerTodosTodos();
                    foreach (var contrato in contratos)
            {
                if (contrato.FechaHasta < DateTime.Now)
                {
                    contrato.Vigente = false;
                    repositorioContrato.Modificacion(contrato);
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
                var inm = repositorioInmueble.ObtenerPorId(contrato.IdInmuebles);
                contrato.DireccionInmueble = inm != null ? inm.Direccion : "";
                contrato.Precio = inm != null ? inm.Precio : 0;
            }
        return View(contratos);
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Multas()
    {
        var multas = repositorioMulta.ObtenerTodosTodos();
            foreach (var multa in multas)
            {
                var contrato = repositorioContrato.obtenerDireccionPrecioInmueblePorIdContrato(multa.IdContrato);
                multa.DireccionInmueble = contrato != null ? contrato.DireccionInmueble : "";
                multa.ImporteCuota = contrato != null ? (contrato.Precio ?? 0) : 0;
            }
        return View(multas);
    }


}