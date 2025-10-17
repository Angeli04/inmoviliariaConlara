using Inmobiliaria.Models;
using InmobiliariaConlara.Models;
using MySql.Data.MySqlClient;

using MySql.Data.MySqlClient;
using System.Data;

namespace Inmobiliaria.Models
{
    public class RepositorioInmuebles
    {
        private readonly string connectionString;

        public RepositorioInmuebles(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public int Alta(Inmuebles inmueble)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                // <-- CAMBIO: Se actualiza idPropietario por idUsuario en la consulta
                string sql = @"INSERT INTO Inmuebles
                        ( direccion, ambientes, superficie, latitud, longitud, idUsuario, IdTipoInmueble, precio, habilitado, existe)
                        VALUES ( @direccion, @ambientes, @superficie, @latitud, @longitud, @idUsuario, @IdTipoInmueble, @precio, @habilitado, @existe);
                        SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@direccion", inmueble.Direccion);
                    command.Parameters.AddWithValue("@ambientes", inmueble.Ambientes);
                    command.Parameters.AddWithValue("@superficie", inmueble.Superficie);
                    command.Parameters.AddWithValue("@latitud", inmueble.Latitud);
                    command.Parameters.AddWithValue("@longitud", inmueble.Longitud);
                    command.Parameters.AddWithValue("@idUsuario", inmueble.IdUsuario); // <-- CAMBIO
                    command.Parameters.AddWithValue("@IdTipoInmueble", inmueble.IdTipoInmueble);
                    command.Parameters.AddWithValue("@precio", inmueble.Precio);
                    command.Parameters.AddWithValue("@habilitado", 1);
                    command.Parameters.AddWithValue("@existe", 1);
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    inmueble.IdInmuebles = res; // <-- CORRECCIÓN: Asignar a IdInmuebles
                    connection.Close();
                }
            }
            return res;
        }

        public int Baja(Inmuebles inmueble)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                // <-- CAMBIO: Se actualiza idPropietario por idUsuario en la consulta
                string sql = @"UPDATE Inmuebles SET 
                                direccion=@direccion, ambientes=@ambientes, superficie=@superficie, latitud=@latitud, longitud=@longitud, idUsuario=@idUsuario, IdTipoInmueble=@IdTipoInmueble, precio=@precio, habilitado=@habilitado, existe=@existe
                                WHERE IdInmuebles = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", inmueble.IdInmuebles);
                    command.Parameters.AddWithValue("@direccion", inmueble.Direccion);
                    command.Parameters.AddWithValue("@ambientes", inmueble.Ambientes);
                    command.Parameters.AddWithValue("@superficie", inmueble.Superficie);
                    command.Parameters.AddWithValue("@latitud", inmueble.Latitud);
                    command.Parameters.AddWithValue("@longitud", inmueble.Longitud);
                    command.Parameters.AddWithValue("@idUsuario", inmueble.IdUsuario); // <-- CAMBIO
                    command.Parameters.AddWithValue("@idTipoInmueble", inmueble.IdTipoInmueble);
                    command.Parameters.AddWithValue("@precio", inmueble.Precio);
                    command.Parameters.AddWithValue("@habilitado", inmueble.Habilitado);
                    command.Parameters.AddWithValue("@existe", 0);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public int Modificacion(Inmuebles inmueble)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                // <-- CAMBIO: Se actualiza idPropietario por idUsuario en la consulta
                string sql = @"UPDATE Inmuebles SET 
                                direccion=@direccion, ambientes=@ambientes, superficie=@superficie, latitud=@latitud, longitud=@longitud, idUsuario=@idUsuario, IdTipoInmueble=@IdTipoInmueble, precio=@precio, habilitado=@habilitado, existe=@existe
                                WHERE IdInmuebles = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", inmueble.IdInmuebles);
                    command.Parameters.AddWithValue("@direccion", inmueble.Direccion);
                    command.Parameters.AddWithValue("@ambientes", inmueble.Ambientes);
                    command.Parameters.AddWithValue("@superficie", inmueble.Superficie);
                    command.Parameters.AddWithValue("@latitud", inmueble.Latitud);
                    command.Parameters.AddWithValue("@longitud", inmueble.Longitud);
                    command.Parameters.AddWithValue("@idUsuario", inmueble.IdUsuario); // <-- CAMBIO
                    command.Parameters.AddWithValue("@idTipoInmueble", inmueble.IdTipoInmueble);
                    command.Parameters.AddWithValue("@precio", inmueble.Precio);
                    command.Parameters.AddWithValue("@habilitado", inmueble.Habilitado);
                    command.Parameters.AddWithValue("@existe", 1);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Inmuebles> ObtenerTodos()
        {
            var res = new List<Inmuebles>();
            using (var connection = new MySqlConnection(connectionString))
            {
                // <-- CAMBIO: Se pide idUsuario y se hace JOIN para obtener el nombre del dueño.
                string sql = @"SELECT i.*, u.Nombre, u.Apellido 
                             FROM Inmuebles i 
                             JOIN Usuario u ON i.idUsuario = u.idUsuario 
                             WHERE i.existe = 1
                             ORDER BY i.Direccion";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var inmueble = new Inmuebles
                        {
                            IdInmuebles = Convert.ToInt32(reader["IdInmuebles"]),
                            Direccion = reader["Direccion"].ToString() ?? string.Empty,
                            Ambientes = Convert.ToInt32(reader["Ambientes"]),
                            Superficie = Convert.ToInt32(reader["Superficie"]),
                            Latitud = Convert.ToDecimal(reader["Latitud"]),
                            Longitud = Convert.ToDecimal(reader["Longitud"]),
                            IdUsuario = Convert.ToInt32(reader["idUsuario"]), // <-- CAMBIO
                            IdTipoInmueble = Convert.ToInt32(reader["IdTipoInmueble"]),
                            Precio = Convert.ToDecimal(reader["precio"]),
                            Habilitado = Convert.ToBoolean(reader["habilitado"]),
                            Duenio = new Usuario // <-- CAMBIO: Se popula el objeto dueño.
                            {
                                IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                                Nombre = reader["Nombre"].ToString(),
                                Apellido = reader["Apellido"].ToString()
                            }
                        };
                        res.Add(inmueble);
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public Inmuebles? ObtenerPorId(int id)
        {
            Inmuebles? inmueble = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                // <-- CAMBIO: Se pide idUsuario y se hace JOIN para obtener el nombre del dueño.
                string sql = @"SELECT i.*, u.Nombre, u.Apellido
                             FROM Inmuebles i
                             JOIN Usuario u ON i.idUsuario = u.idUsuario
                             WHERE i.IdInmuebles = @id AND i.existe=1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        inmueble = new Inmuebles
                        {
                            IdInmuebles = Convert.ToInt32(reader["IdInmuebles"]),
                            Direccion = reader["Direccion"].ToString() ?? string.Empty,
                            Ambientes = Convert.ToInt32(reader["Ambientes"]),
                            Superficie = Convert.ToInt32(reader["Superficie"]),
                            Latitud = Convert.ToDecimal(reader["Latitud"]),
                            Longitud = Convert.ToDecimal(reader["Longitud"]),
                            IdUsuario = Convert.ToInt32(reader["idUsuario"]), // <-- CAMBIO
                            IdTipoInmueble = Convert.ToInt32(reader["IdTipoInmueble"]),
                            Precio = Convert.ToDecimal(reader["precio"]),
                            Habilitado = Convert.ToBoolean(reader["habilitado"]),
                            Duenio = new Usuario // <-- CAMBIO: Se popula el objeto dueño.
                            {
                                IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                                Nombre = reader["Nombre"].ToString(),
                                Apellido = reader["Apellido"].ToString()
                            }
                        };
                    }
                    connection.Close();
                }
            }
            return inmueble;
        }

        // <-- CAMBIO RADICAL EN ESTE MÉTODO -->
        public IList<Inmuebles> ObtenerPorPropietario(int idUsuario)
        {
            var res = new List<Inmuebles>();
            using (var connection = new MySqlConnection(connectionString))
            {
                // Se actualiza el WHERE para buscar por idUsuario
                string sql = @"SELECT * FROM Inmuebles
                             WHERE idUsuario = @idUsuario AND existe=1
                             ORDER BY Direccion";
                using (var command = new MySqlCommand(sql, connection))
                {
                    // Se actualiza el nombre del parámetro
                    command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var inmueble = new Inmuebles
                        {
                            IdInmuebles = Convert.ToInt32(reader["IdInmuebles"]),
                            Direccion = reader["Direccion"].ToString() ?? string.Empty,
                            Ambientes = Convert.ToInt32(reader["Ambientes"]),
                            Superficie = Convert.ToInt32(reader["Superficie"]),
                            Latitud = Convert.ToDecimal(reader["Latitud"]),
                            Longitud = Convert.ToDecimal(reader["Longitud"]),
                            IdUsuario = Convert.ToInt32(reader["idUsuario"]), // <-- CAMBIO
                            IdTipoInmueble = Convert.ToInt32(reader["IdTipoInmueble"]),
                            Precio = Convert.ToDecimal(reader["precio"]),
                            Habilitado = Convert.ToBoolean(reader["habilitado"])
                        };
                        res.Add(inmueble);
                    }
                    connection.Close();
                }
            }
            return res;
        }
        
        public Inmuebles? ObtenerPorDireccion(String dir)
{
    Inmuebles? inmueble = null;
    using (var connection = new MySqlConnection(connectionString))
    {
        // <-- CAMBIO: Se actualiza la columna en el SELECT y se añade JOIN
        string sql = @"SELECT i.*, u.Nombre, u.Apellido 
                     FROM Inmuebles i
                     JOIN Usuario u ON i.idUsuario = u.idUsuario
                     WHERE i.Direccion = @dir AND i.existe = 1";
        using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@dir", dir);
            connection.Open();
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                inmueble = new Inmuebles
                {
                    IdInmuebles = Convert.ToInt32(reader["IdInmuebles"]),
                    Direccion = reader["Direccion"].ToString() ?? string.Empty,
                    Ambientes = Convert.ToInt32(reader["Ambientes"]),
                    Superficie = Convert.ToInt32(reader["Superficie"]),
                    Latitud = Convert.ToDecimal(reader["Latitud"]),
                    Longitud = Convert.ToDecimal(reader["Longitud"]),
                    IdUsuario = Convert.ToInt32(reader["idUsuario"]), // <-- CAMBIO
                    IdTipoInmueble = Convert.ToInt32(reader["IdTipoInmueble"]),
                    Precio = Convert.ToDecimal(reader["precio"]),
                    Habilitado = Convert.ToBoolean(reader["habilitado"]),
                    Duenio = new Usuario // <-- Se popula el dueño
                    {
                        IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                        Nombre = reader["Nombre"].ToString(),
                        Apellido = reader["Apellido"].ToString()
                    }
                };
            }
            connection.Close();
        }
    }
    return inmueble;
}

public IList<Inmuebles> BuscarPorFraccionDireccion(string fraccion)
{
    IList<Inmuebles> res = new List<Inmuebles>();
    using (var connection = new MySqlConnection(connectionString))
    {
        // En este método no se pedían datos del propietario, así que no hay cambios.
        string sql = @"
                        SELECT IdInmuebles, Direccion, Precio, Habilitado
                        FROM Inmuebles
                        WHERE direccion LIKE @fraccion AND existe=1";

        using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@fraccion", "%" + fraccion + "%");
            connection.Open();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetBoolean("Habilitado"))
                {
                    Inmuebles i = new Inmuebles
                    {
                        IdInmuebles = reader.GetInt32("IdInmuebles"),
                        Direccion = reader.GetString("Direccion"),
                        Precio = reader.GetDecimal("Precio"),
                        Habilitado = reader.GetBoolean("Habilitado")
                    };
                    res.Add(i);
                }
            }
            connection.Close();
        }
    }
    return res;
}

public IList<Inmuebles> ObtenerTodosDisponibles()
{
    var res = new List<Inmuebles>();
    using (var connection = new MySqlConnection(connectionString))
    {
        // <-- CAMBIO: Se actualiza la columna en el SELECT y se añade JOIN
        string sql = @"SELECT i.*, u.Nombre, u.Apellido 
                     FROM Inmuebles i
                     JOIN Usuario u ON i.idUsuario = u.idUsuario
                     WHERE i.habilitado=1 AND i.existe = 1
                     ORDER BY i.Direccion";
        using (var command = new MySqlCommand(sql, connection))
        {
            connection.Open();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var inmueble = new Inmuebles
                {
                    IdInmuebles = Convert.ToInt32(reader["IdInmuebles"]),
                    Direccion = reader["Direccion"].ToString() ?? string.Empty,
                    Ambientes = Convert.ToInt32(reader["Ambientes"]),
                    Superficie = Convert.ToInt32(reader["Superficie"]),
                    Latitud = Convert.ToDecimal(reader["Latitud"]),
                    Longitud = Convert.ToDecimal(reader["Longitud"]),
                    IdUsuario = Convert.ToInt32(reader["idUsuario"]), // <-- CAMBIO
                    IdTipoInmueble = Convert.ToInt32(reader["IdTipoInmueble"]),
                    Precio = Convert.ToDecimal(reader["precio"]),
                    Habilitado = Convert.ToBoolean(reader["habilitado"]),
                    Duenio = new Usuario // <-- Se popula el dueño
                    {
                        IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                        Nombre = reader["Nombre"].ToString(),
                        Apellido = reader["Apellido"].ToString()
                    }
                };
                res.Add(inmueble);
            }
            connection.Close();
        }
    }
    return res;
}

public IList<Inmuebles> BuscarDesocupados(DateTime? fechaDesde, DateTime? fechaHasta)
{
    var res = new List<Inmuebles>();
    using (var connection = new MySqlConnection(connectionString))
    {
        // <-- CAMBIO: Se actualiza la columna en el SELECT
        string sql = @"SELECT i.IdInmuebles, i.Direccion, i.Ambientes, i.Superficie, i.Latitud, i.Longitud, i.idUsuario, i.IdTipoInmueble, i.precio, i.habilitado
                        FROM Inmuebles i
                        LEFT JOIN Contratos c 
                            ON i.IdInmuebles = c.IdInmuebles 
                            AND c.vigente = 1
                            AND (
                                (c.fechaDesde <= @fechaHasta) AND (c.fechaHasta >= @fechaDesde)
                            )
                        WHERE c.IdInmuebles IS NULL AND i.existe = 1
                        ORDER BY i.Direccion";
        /* NOTA: Simplifiqué tu lógica de solapamiento de fechas. La condición
           (inicio_A <= fin_B) AND (fin_A >= inicio_B) es la forma estándar
           de detectar si dos rangos de tiempo se superponen. */

        using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@fechaDesde", fechaDesde ?? DateTime.MinValue);
            command.Parameters.AddWithValue("@fechaHasta", fechaHasta ?? DateTime.MaxValue);
            connection.Open();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var inmueble = new Inmuebles
                {
                    IdInmuebles = Convert.ToInt32(reader["IdInmuebles"]),
                    Direccion = reader["Direccion"].ToString() ?? string.Empty,
                    Ambientes = Convert.ToInt32(reader["Ambientes"]),
                    Superficie = Convert.ToInt32(reader["Superficie"]),
                    Latitud = Convert.ToDecimal(reader["Latitud"]),
                    Longitud = Convert.ToDecimal(reader["Longitud"]),
                    IdUsuario = Convert.ToInt32(reader["idUsuario"]), // <-- CAMBIO
                    IdTipoInmueble = Convert.ToInt32(reader["IdTipoInmueble"]),
                    Precio = Convert.ToDecimal(reader["precio"]),
                    Habilitado = Convert.ToBoolean(reader["habilitado"])
                };
                res.Add(inmueble);
            }
            connection.Close();
        }
    }
    return res;
}
    }
}