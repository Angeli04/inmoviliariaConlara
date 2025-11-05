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
        string sql = @"INSERT INTO Inmuebles
                       (Direccion, Ambientes, Superficie, Latitud, Longitud, 
                        IdUsuario, IdTipoInmueble, Precio, Habilitado, Existe, 
                        ImagenUrl) -- 1. AÑADIR LA COLUMNA AQUÍ
                       VALUES
                       (@direccion, @ambientes, @superficie, @latitud, @longitud, 
                        @idUsuario, @idTipoInmueble, @precio, @habilitado, @existe,
                        @imagenUrl); -- 2. AÑADIR EL PARÁMETRO AQUÍ
                       SELECT LAST_INSERT_ID();";
        
        using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@direccion", inmueble.Direccion);
            command.Parameters.AddWithValue("@ambientes", inmueble.Ambientes);
            command.Parameters.AddWithValue("@superficie", inmueble.Superficie); 
            command.Parameters.AddWithValue("@latitud", inmueble.Latitud);
            command.Parameters.AddWithValue("@longitud", inmueble.Longitud);
            command.Parameters.AddWithValue("@idUsuario", inmueble.IdUsuario);
            command.Parameters.AddWithValue("@idTipoInmueble", inmueble.IdTipoInmueble);
            command.Parameters.AddWithValue("@precio", inmueble.Precio);
            command.Parameters.AddWithValue("@habilitado", inmueble.Habilitado);
            command.Parameters.AddWithValue("@existe", 1); 

        
            command.Parameters.AddWithValue("@imagenUrl", (object)inmueble.ImagenUrl ?? DBNull.Value);

            connection.Open();
            res = Convert.ToInt32(command.ExecuteScalar());
            inmueble.IdInmuebles = res;
          
        }
    }
    return res;
}

        public int Baja(Inmuebles inmueble)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                
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
                    command.Parameters.AddWithValue("@idUsuario", inmueble.IdUsuario);
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
                
                string sql = @"UPDATE Inmuebles SET 
                                direccion=@direccion, ambientes=@ambientes, superficie=@superficie, latitud=@latitud, longitud=@longitud, idUsuario=@idUsuario, IdTipoInmueble=@IdTipoInmueble, precio=@precio, habilitado=@habilitado, existe=@existe, ImagenUrl=@imagenUrl
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
                    command.Parameters.AddWithValue("@imagenUrl", inmueble.ImagenUrl);
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
                            IdUsuario = Convert.ToInt32(reader["idUsuario"]), 
                            IdTipoInmueble = Convert.ToInt32(reader["IdTipoInmueble"]),
                            Precio = Convert.ToDecimal(reader["precio"]),
                            Habilitado = Convert.ToBoolean(reader["habilitado"]),
                            Duenio = new Usuario 
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

        public IList<Inmuebles> ObtenerPorPropietario(int idUsuario)
        {
            var res = new List<Inmuebles>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT * FROM Inmuebles
                             WHERE idUsuario = @idUsuario AND existe=1
                             ORDER BY Direccion";
                using (var command = new MySqlCommand(sql, connection))
                {
                    
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
                            IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                            IdTipoInmueble = Convert.ToInt32(reader["IdTipoInmueble"]),
                            Precio = Convert.ToDecimal(reader["precio"]),
                            Habilitado = Convert.ToBoolean(reader["habilitado"]),
                            Duenio = new Usuario 
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
                            IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                            IdTipoInmueble = Convert.ToInt32(reader["IdTipoInmueble"]),
                            Precio = Convert.ToDecimal(reader["precio"]),
                            Habilitado = Convert.ToBoolean(reader["habilitado"]),
                            Duenio = new Usuario 
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
                            IdUsuario = Convert.ToInt32(reader["idUsuario"]),
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

public List<Inmuebles> ObtenerInmueblesCompletosPorPropietario(int idPropietario)
{
    var listaInmuebles = new List<Inmuebles>();

    using (var connection = new MySqlConnection(connectionString))
    {

        string sql = @"SELECT i.IdInmuebles, i.Direccion, i.Ambientes, i.Superficie, 
                              i.Latitud, i.Longitud, i.IdTipoInmueble, 
                              t.Nombre as TipoInmueble, 
                              i.Precio, i.Habilitado, i.ImagenUrl
                       FROM Inmuebles i
                       JOIN tipoinmueble t ON i.IdTipoInmueble = t.idTipoInmueble
                       WHERE i.IdUsuario = @idPropietario";

        using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@idPropietario", idPropietario);
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var inmueble = new Inmuebles
                    {
                        IdInmuebles = reader.GetInt32("IdInmuebles"),
                        Direccion = reader.GetString("Direccion"),
                        Ambientes = reader.GetInt32("Ambientes"),
                        Superficie = reader.GetInt32("Superficie"),
                        Latitud = reader.GetDecimal("Latitud"),
                        Longitud = reader.GetDecimal("Longitud"),
                        IdTipoInmueble = reader.GetInt32("IdTipoInmueble"),
                        TipoInmueble = reader.GetString("TipoInmueble"),                      
                        Precio = reader.GetDecimal("Precio"),
                        Habilitado = reader.GetBoolean("Habilitado"),
                        ImagenUrl = reader.IsDBNull(reader.GetOrdinal("ImagenUrl")) 
                                    ? null 
                                    : reader.GetString("ImagenUrl")
                    };
                    listaInmuebles.Add(inmueble);
                }
            }
        }
    }
    return listaInmuebles;
}

        public Inmuebles? ObtenerInmueblesCompletosPorIdApi(int idInmueble)
        {
            Inmuebles? inmueble = null;
            using (var connection = new MySqlConnection(connectionString))
            {
               
                string sql = @"SELECT i.IdInmuebles, i.Direccion, i.Ambientes, i.Superficie, 
                              i.Latitud, i.Longitud, i.IdTipoInmueble, 
                              t.Nombre as TipoInmueble, 
                              i.Precio, i.Habilitado, i.ImagenUrl
                       FROM Inmuebles i
                       JOIN tipoinmueble t ON i.IdTipoInmueble = t.idTipoInmueble
                       WHERE i.IdInmuebles = @idInmueble";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInmueble", idInmueble);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read()) 
                        {
                            inmueble = new Inmuebles
                            {
                                IdInmuebles = reader.GetInt32("IdInmuebles"),
                                Direccion = reader.GetString("Direccion"),
                                Ambientes = reader.GetInt32("Ambientes"),
                                Superficie = reader.GetInt32("Superficie"),
                                Latitud = reader.GetDecimal("Latitud"),
                                Longitud = reader.GetDecimal("Longitud"),
                                IdTipoInmueble = reader.GetInt32("IdTipoInmueble"),
                                TipoInmueble = reader.GetString("TipoInmueble"),
                                Precio = reader.GetDecimal("Precio"),
                                Habilitado = reader.GetBoolean("Habilitado"),
                                ImagenUrl = reader.IsDBNull(reader.GetOrdinal("ImagenUrl"))
                                    ? null
                                    : reader.GetString("ImagenUrl")
                            };
                        }
                    }
                }
            }
            return inmueble;
        }

        
        public Inmuebles Habilitar(int id, bool habilitado){
            using (var connection = new MySqlConnection(connectionString))
            {
                String sql = @"UPDATE Inmuebles SET Habilitado = @habilitado WHERE IdInmuebles = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@habilitado", habilitado);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            Inmuebles inmueble = ObtenerPorId(id);
            return inmueble;
        }
    }
}