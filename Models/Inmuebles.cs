using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InmobiliariaConlara.Models; // Necesario para [ForeignKey]

namespace Inmobiliaria.Models // O InmobiliariaConlara.Models
{
  public class Inmuebles
  {
    [Display(Name = "Codigo Interno")]
    public int IdInmuebles { get; set; }

    [Display(Name = "Dirección")]
    [Required(ErrorMessage = "La dirección es requerida")]
    [StringLength(100, ErrorMessage = "La dirección no puede superar los 100 caracteres")]
    public string? Direccion { get; set; }

    [Required(ErrorMessage = "La Cantidad de Ambientes es obligatorio")]
    [Range(1, 20, ErrorMessage = "La cantidad de ambientes debe estar entre 1 y 20")]
    public int Ambientes { get; set; }

    [Required(ErrorMessage = "La superficie es obligatoria")]
    public int Superficie { get; set; }

    [Required(ErrorMessage = "La latitud es obligatoria")]
    public decimal Latitud { get; set; }

    [Required(ErrorMessage = "La longitud es obligatoria")]
    public decimal Longitud { get; set; }

    // --- CAMBIOS AQUÍ ---
    [Display(Name = "Dueño")]
    [Required(ErrorMessage = "El propietario es obligatorio")]
    [ForeignKey(nameof(Duenio))] // Crea la relación con el objeto 'Duenio'
    public int IdUsuario { get; set; } // Cambiamos IdPropietario por IdUsuario

    [Display(Name = "Dueño")]
    public Usuario? Duenio { get; set; } // Cambiamos el tipo de Propietario a Usuario
    // --- FIN DE LOS CAMBIOS ---

    [Display(Name = "Tipo de Inmueble")]
    [Required(ErrorMessage = "El tipo de inmueble es obligatorio")]
    public int IdTipoInmueble { get; set; }
    
    // Esta propiedad auxiliar puede que ya no sea necesaria si tienes una navegación al tipo de inmueble
    [Display(Name = "Tipo de Inmueble")]
    public String? TipoInmueble { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio")]
    public decimal Precio { get; set; }
    
    public bool Habilitado { get; set; }
    
    [Required]
    public bool Existe { get; set; }
  }
}