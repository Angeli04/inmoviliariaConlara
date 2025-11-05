namespace Inmobiliaria.Models
{
    public class InquilinoAlquilerDto
    {
        // Datos del Inquilino
        public int IdInquilino { get; set; }
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public string Dni { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }

        // Datos del Inmueble
        public string DireccionInmueble { get; set; }
    }
}
