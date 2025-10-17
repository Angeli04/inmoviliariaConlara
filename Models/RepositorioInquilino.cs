using MySql.Data.MySqlClient;

namespace Inmobiliaria.Models
{
    public class RepositorioInquilino
    {
        private readonly string connectionString;

        public RepositorioInquilino(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public int Alta(Inquilino i)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Inquilino
                    ( Apellido,Nombre, Dni, Telefono, eMail, existe)
                    VALUES ( @apellido,@nombre, @dni, @telefono, @email, @existe);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", i.Nombre);
                    command.Parameters.AddWithValue("@apellido", i.Apellido);
                    command.Parameters.AddWithValue("@dni", i.Dni);
                    command.Parameters.AddWithValue("@telefono", i.Telefono ?? "");
                    command.Parameters.AddWithValue("@email", i.eMail);
                    command.Parameters.AddWithValue("@existe", 1);
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    i.IdInquilino = res;
                    connection.Close();
                }
            }
            return res;
        }

        public int Baja(Inquilino i)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Inquilino
                    SET  Apellido=@apellido,Nombre=@nombre, Dni=@dni, Telefono=@telefono, eMail=@email, existe=@existe
                    WHERE IdInquilino = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@apellido", i.Apellido);
                    command.Parameters.AddWithValue("@nombre", i.Nombre);
                    command.Parameters.AddWithValue("@dni", i.Dni);
                    command.Parameters.AddWithValue("@telefono", i.Telefono ?? "");
                    command.Parameters.AddWithValue("@email", i.eMail);
                    command.Parameters.AddWithValue("@id", i.IdInquilino);
                    command.Parameters.AddWithValue("@existe", 0);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public int Modificacion(Inquilino i)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Inquilino
                    SET  Apellido=@apellido,Nombre=@nombre, Dni=@dni, Telefono=@telefono, eMail=@email, existe=@existe
                    WHERE IdInquilino = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@apellido", i.Apellido);
                    command.Parameters.AddWithValue("@nombre", i.Nombre);
                    command.Parameters.AddWithValue("@dni", i.Dni);
                    command.Parameters.AddWithValue("@telefono", i.Telefono ?? "");
                    command.Parameters.AddWithValue("@email", i.eMail);
                    command.Parameters.AddWithValue("@id", i.IdInquilino);
                    command.Parameters.AddWithValue("@existe", 1);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Inquilino> ObtenerTodos()
        {
            IList<Inquilino> res = new List<Inquilino>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdInquilino, Nombre, Apellido, Dni, Telefono, eMail FROM inquilino WHERE existe=1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Inquilino i = new Inquilino
                        {
                            IdInquilino = reader.GetInt32("IdInquilino"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido"),
                            Dni = reader.GetString("Dni"),
                            Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? "" : reader.GetString("Telefono"),
                            eMail = reader.GetString("eMail")
                        };
                        res.Add(i);
                    }
                    connection.Close();
                }
            }
            return res;
        }
        


        public Inquilino? ObtenerPorId(int id)
        {
            Inquilino? i = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdInquilino, Nombre, Apellido, Dni, Telefono, eMail 
                    FROM Inquilino WHERE IdInquilino=@id && existe=1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        i = new Inquilino
                        {
                            IdInquilino = reader.GetInt32("IdInquilino"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido"),
                            Dni = reader.GetString("Dni"),
                            Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? "" : reader.GetString("Telefono"),
                            eMail = reader.GetString("eMail")
                        };
                    }
                    connection.Close();
                }
            }
            return i;
        }
        public Inquilino? ObtenerPorDni(String dni)
        {
            Inquilino? i = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdInquilino, Nombre, Apellido, Dni, Telefono, eMail 
                    FROM Inquilino WHERE Dni=@dni && existe=1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", dni);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        i = new Inquilino
                        {
                            IdInquilino = reader.GetInt32("IdInquilino"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido"),
                            Dni = reader.GetString("Dni"),
                            Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? "" : reader.GetString("Telefono"),
                            eMail = reader.GetString("eMail")
                        };
                    }
                    connection.Close();
                }
            }
            return i;
        }
         public IList<Inquilino> BuscarPorFraccionApellido(string fraccion)
        {
            IList<Inquilino> res = new List<Inquilino>();

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                            SELECT IdInquilino, Nombre, Apellido
                            FROM Inquilino
                            WHERE Apellido LIKE @fraccion && existe=1
                        ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    // Agregamos los comodines para LIKE
                    command.Parameters.AddWithValue("@fraccion", "%" + fraccion + "%");

                    connection.Open();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Inquilino i = new Inquilino
                        {
                            IdInquilino = reader.GetInt32("IdInquilino"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido")
                        };
                        res.Add(i);
                    }

                    connection.Close();
                }
            }

            return res;
        }
        public IList<Inquilino> ObtenerPaginado(int pageNumber, int pageSize)
                        { 
                            IList<Inquilino> res = new List<Inquilino>();
                            using (var connection = new MySqlConnection(connectionString))
                            {
                                
                                int offset = (pageNumber - 1) * pageSize;
                                string sql = @"SELECT IdInquilino, Nombre, Apellido, Dni, Telefono, eMail 
                                            FROM inquilino 
                                            WHERE existe=1 
                                            ORDER BY IdInquilino
                                            LIMIT @pageSize OFFSET @offset";

                                using (var command = new MySqlCommand(sql, connection))
                                {
                                    command.Parameters.AddWithValue("@pageSize", pageSize);
                                    command.Parameters.AddWithValue("@offset", offset);

                                    connection.Open();
                                    var reader = command.ExecuteReader();
                                    while (reader.Read())
                                    {
                                        Inquilino i = new Inquilino
                                        {
                                            IdInquilino = reader.GetInt32("IdInquilino"),
                                            Nombre = reader.GetString("Nombre"),
                                            Apellido = reader.GetString("Apellido"),
                                            Dni = reader.GetString("Dni"),
                                            Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? "" : reader.GetString("Telefono"),
                                            eMail = reader.GetString("eMail")
                                        };
                                        res.Add(i);
                                    }
                                    connection.Close();
                                }
                            }
                            return res;
                        }

        public int ContarInquilinos()
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        string sql = "SELECT COUNT(*) FROM inquilino WHERE existe=1";
                        using (var command = new MySqlCommand(sql, connection))
                        {
                            connection.Open();
                            return Convert.ToInt32(command.ExecuteScalar());
                        }
                    }
                }
    }    
    
}