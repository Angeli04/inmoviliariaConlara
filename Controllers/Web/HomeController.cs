using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using InmobiliariaConlara.Models;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria.Controllers
{
    [Authorize] // 🔒 por defecto todas las acciones requieren login
    public class HomeController : Controller
    {
        [AllowAnonymous] // 🚪 cualquiera puede entrar, esté logueado o no
        public IActionResult Restringido()
        {
            return View();
        } 

        [Authorize(Roles = "Administrador")] // 👑 solo admins
        public IActionResult SoloAdmin()
        {
            return Content("📌 Solo el administrador puede ver esta página.");
        }

        [Authorize] // 👨‍💼 empleados y admins
        public IActionResult SoloEmpleado()
        {
            return Content("📌 Empleados y administradores pueden ver esta página.");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string? mensaje)
        {
            if (!string.IsNullOrEmpty(mensaje))
            {
                TempData["Error"] = mensaje;
            }

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
