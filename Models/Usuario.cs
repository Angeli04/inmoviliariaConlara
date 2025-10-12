
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaConlara.Models
{
	public enum enRoles
	{

		Administrador = 1,
		Empleado = 2,
		Propietario = 3,
	}

	public class Usuario
	{
		[Key]
		[Display(Name = "Código Personal")]
		public int IdUsuario { get; set; }

		[Required(ErrorMessage = "El Nombre es obligatorio")]
		[StringLength(100, ErrorMessage = "El Nombre no puede superar los 100 caracteres")]
		public string? Nombre { get; set; }

		[Required(ErrorMessage = "El apellido es obligatorio")]
		[StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres")]
		public string? Apellido { get; set; }
		[Required, EmailAddress]
		public string? Email { get; set; }
		[Required, DataType(DataType.Password)]
		public string? Clave { get; set; }
		public string Avatar { get; set; } = "";
		[NotMapped]//Para EF
		public IFormFile? AvatarFile { get; set; }

		public int Rol { get; set; }
		[NotMapped]//Para EF
		public string RolNombre => Rol > 0 ? ((enRoles)Rol).ToString() : "";


		public static IDictionary<int, string> ObtenerRoles()
		{
			SortedDictionary<int, string> roles = new SortedDictionary<int, string>();
			Type tipoEnumRol = typeof(enRoles);
			foreach (var valor in Enum.GetValues(tipoEnumRol))
			{
				roles.Add((int)valor, Enum.GetName(tipoEnumRol, valor));
			}
			return roles;
		}
		
		public int Existe { get; set; }

	}
}