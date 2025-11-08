namespace Inmobiliaria.Models;

public class ContratoDetalleDto
{
    public int IdContrato { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public int Vigente { get; set; }
    public int CantidadCuotas { get; set; }

    // Datos combinados
    public string NombreInquilino { get; set; } = "";
    public string DireccionInmueble { get; set; } = "";
}

