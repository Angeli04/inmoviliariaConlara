using Inmobiliaria.Models; // Asegúrate que el namespace sea el correcto
using InmobiliariaConlara.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class InmueblesController : Controller
{
    // Usamos readonly para asegurar que solo se asignen en el constructor
    private readonly RepositorioInmuebles repo;
    private readonly RepositorioTipoInmueble repositorioTipoInmueble;
    private readonly RepositorioUsuario repositorioUsuario;

    // --- CAMBIO CLAVE: Inyección de Dependencias ---
    // En lugar de crear 'new', los repositorios se "inyectan" automáticamente.
    // Asegúrate de tenerlos registrados en tu archivo Program.cs o Startup.cs
    public InmueblesController(RepositorioInmuebles repo, RepositorioTipoInmueble repoTipos, RepositorioUsuario repoUsuarios)
    {
        this.repo = repo;
        this.repositorioTipoInmueble = repoTipos;
        this.repositorioUsuario = repoUsuarios;
    }

    [Authorize]
    public IActionResult Index()
    {
        // El método ObtenerTodos() ya devuelve los inmuebles con su dueño.
        var lista = repo.ObtenerTodos();
        return View(lista);
    }

    [Authorize]
    public IActionResult Create()
    {
        ViewBag.TipoInmuebles = repositorioTipoInmueble.ObtenerTodos();
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
            return NotFound("No se encontró ningún Inmueble para editar");
        }
        // La línea que asignaba el dueño aquí ya no es necesaria. repo.ObtenerPorId ya lo hace.
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
            return NotFound("No se encontró ningún inmueble para editar");
        }

        if (ModelState.IsValid)
        {
            repo.Modificacion(inmueble);
            return RedirectToAction(nameof(Index));
        }

        ViewBag.TipoInmuebles = repositorioTipoInmueble.ObtenerTodos();
        return View(inmueble);
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Delete(int id)
    {
        var inmueble = repo.ObtenerPorId(id);
        if (inmueble == null)
        {
            return NotFound();
        }
        return View(inmueble);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id, Inmuebles inmueble) // Recibe el modelo para el binding
    {
        // Es una buena práctica volver a obtener el objeto para asegurar consistencia
        var inmuebleParaBorrar = repo.ObtenerPorId(id);
        if (inmuebleParaBorrar == null)
        {
            return NotFound();
        }

        repo.Baja(inmuebleParaBorrar); // Se pasa el objeto obtenido de la DB
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public IActionResult BuscarInmueblePorFraccionDireccion(string term)
    {
        if (string.IsNullOrEmpty(term) || term.Length < 3)
        {
            return Json(new { success = false, data = new List<object>() });
        }

        var lista = repo.BuscarPorFraccionDireccion(term);
        if (lista == null || !lista.Any())
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
        
        // CORREGIDO: Se usa el repositorio de usuarios
        ViewBag.Propietario = repositorioUsuario.ObtenerPorId(id.Value);
        
        return View("PorPropietario", lista);
    }

    public IActionResult Habilitados()
    {
        var lista = repo.ObtenerTodosDisponibles();
        return View("Index", lista);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BuscarDesocupados(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        // Lógica de validación primero
        if (!fechaDesde.HasValue || !fechaHasta.HasValue)
        {
            ModelState.AddModelError("", "Debe ingresar ambas fechas.");
        }
        else if (fechaDesde > fechaHasta)
        {
            ModelState.AddModelError("", "La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.");
        }

        // Si hay errores, devolver la lista completa
        if (!ModelState.IsValid)
        {
            var listaCompleta = repo.ObtenerTodos();
            return View("Index", listaCompleta);
        }

        // Si no hay errores, hacer la búsqueda
        var desocupados = repo.BuscarDesocupados(fechaDesde, fechaHasta);
        return View("Index", desocupados);
    }
}