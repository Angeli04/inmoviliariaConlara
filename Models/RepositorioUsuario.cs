using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using MySql.Data.MySqlClient;

namespace InmobiliariaConlara.Models
{
    public class RepositorioUsuario
    {
        private readonly string connectionString;
        private readonly string GlobalSalt = "MiSaltSecreto123";

        public RepositorioUsuario(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty; ;
        }

        public int Alta(Usuario u)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                // <-- CAMBIO: Se añaden las columnas dni y telefono
                string sql = @"INSERT INTO Usuario
                        (Nombre, Apellido, Dni, Telefono, eMail, Clave, avatar, rol, existe)
                        VALUES (@nombre, @apellido, @dni, @telefono, @email, @clave, @avatar, @rol, @existe);
                        SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", u.Nombre);
                    command.Parameters.AddWithValue("@apellido", u.Apellido);
                    command.Parameters.AddWithValue("@dni", u.Dni); // <-- CAMBIO
                    command.Parameters.AddWithValue("@telefono", u.Telefono); // <-- CAMBIO
                    command.Parameters.AddWithValue("@email", u.Email);
                    command.Parameters.AddWithValue("@clave", u.Clave ?? "");
                    command.Parameters.AddWithValue("@avatar", u.Avatar);
                    command.Parameters.AddWithValue("@rol", u.Rol);
                    command.Parameters.AddWithValue("@existe", 1);
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    u.IdUsuario = res;
                    connection.Close();
                }
            }
            return res;
        }

        public int Baja(Usuario u) // En realidad es un borrado lógico
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Usuario
                        SET existe=@existe
                        WHERE IdUsuario = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@existe", 0);
                    command.Parameters.AddWithValue("@id", u.IdUsuario); // <-- CORRECCIÓN: Faltaba este parámetro
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public int Modificacion(Usuario u)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                // <-- CAMBIO: Se añaden las columnas dni y telefono
                string sql = @"UPDATE Usuario
                        SET Apellido=@apellido, Nombre=@nombre, Dni=@dni, Telefono=@telefono, eMail=@email, clave=@clave, avatar=@avatar, rol=@rol, existe=@existe
                        WHERE IdUsuario = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", u.IdUsuario);
                    command.Parameters.AddWithValue("@apellido", u.Apellido);
                    command.Parameters.AddWithValue("@nombre", u.Nombre);
                    command.Parameters.AddWithValue("@dni", u.Dni); // <-- CAMBIO
                    command.Parameters.AddWithValue("@telefono", u.Telefono); // <-- CAMBIO
                    command.Parameters.AddWithValue("@email", u.Email);
                    command.Parameters.AddWithValue("@clave", u.Clave ?? "");
                    command.Parameters.AddWithValue("@avatar", u.Avatar ?? "");
                    command.Parameters.AddWithValue("@rol", u.Rol);
                    command.Parameters.AddWithValue("@existe", 1);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Usuario> ObtenerTodos()
        {
            IList<Usuario> res = new List<Usuario>();
            using (var connection = new MySqlConnection(connectionString))
            {
                // <-- CAMBIO: Se piden las columnas dni y telefono
                string sql = @"SELECT IdUsuario, Nombre, Apellido, Dni, Telefono, eMail, clave, avatar, rol 
                             FROM usuario 
                             WHERE existe = 1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Usuario u = new Usuario
                        {
                            IdUsuario = reader.GetInt32("IdUsuario"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido"),
                            Dni = reader.GetInt32("Dni"), // <-- CAMBIO
                            Telefono = reader.GetString("Telefono"), // <-- CAMBIO
                            Email = reader.GetString("eMail"),
                            Clave = reader.GetString("clave"),
                            Avatar = reader.GetString("avatar"),
                            Rol = reader.GetInt32("rol"),
                        };
                        res.Add(u);
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public int ActualizarPerfilDesdeApp(Usuario usuario)
    {
        int resultado = -1;
        using (var connection = new MySqlConnection(connectionString)) // Usa tu cadena de conexión
        {
            // La consulta SQL solo actualiza los campos que nos interesan.
            // Ignoramos 'Clave', 'Rol', 'Dni', etc.
            string sql = @"UPDATE usuario 
                           SET nombre = @nombre, 
                               apellido = @apellido, 
                               telefono = @telefono, 
                               eMail = @email,
                               dni = @dni
                           WHERE idUsuario = @idUsuario";
            
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@apellido", usuario.Apellido);
                command.Parameters.AddWithValue("@telefono", usuario.Telefono);
                command.Parameters.AddWithValue("@email", usuario.Email);
                command.Parameters.AddWithValue("@dni", usuario.Dni);
                command.Parameters.AddWithValue("@idUsuario", usuario.IdUsuario);
                
                connection.Open();
                resultado = command.ExecuteNonQuery();
                connection.Close();
            }
        }
        return resultado;
    }

        // <-- NUEVO MÉTODO ESENCIAL -->
        public IList<Usuario> ObtenerPropietarios()
        {
            IList<Usuario> res = new List<Usuario>();
            using (var connection = new MySqlConnection(connectionString))
            {
                // Filtramos por el Rol 3 (Propietario)
                string sql = @"SELECT IdUsuario, Nombre, Apellido, Dni, Telefono, eMail
                             FROM usuario 
                             WHERE existe = 1 AND Rol = 3";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Usuario u = new Usuario
                        {
                            IdUsuario = reader.GetInt32("IdUsuario"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido"),
                            Dni = reader.GetInt32("Dni"),
                            Telefono = reader.GetString("Telefono"),
                            Email = reader.GetString("eMail"),
                        };
                        res.Add(u);
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public Usuario? ObtenerPorId(int id)
        {
            Usuario? u = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                // <-- CAMBIO: Se piden las columnas dni y telefono
                string sql = @"SELECT IdUsuario, Nombre, Apellido, Dni, Telefono, eMail, clave, avatar, rol 
                             FROM usuario 
                             WHERE IdUsuario=@id AND existe = 1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        u = new Usuario
                        {
                            IdUsuario = reader.GetInt32("IdUsuario"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido"),
                            Dni = reader.GetInt32("Dni"), // <-- CAMBIO
                            Telefono = reader.GetString("Telefono"), // <-- CAMBIO
                            Email = reader.GetString("eMail"),
                            Clave = reader.GetString("clave"),
                            Avatar = reader.GetString("avatar"),
                            Rol = reader.GetInt32("rol"),
                        };
                    }
                    connection.Close();
                }
            }
            return u;
        }

        public Usuario? ObtenerPorEmail(String email)
        {
            Usuario? u = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                // <-- CAMBIO: Se piden las columnas dni y telefono
                string sql = @"SELECT IdUsuario, Nombre, Apellido, Dni, Telefono, eMail, clave, avatar, rol 
                             FROM usuario 
                             WHERE eMail=@email AND existe=1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        u = new Usuario()
                        {
                            IdUsuario = reader.GetInt32("IdUsuario"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido"),
                            Dni = reader.GetInt32("Dni"), // <-- CAMBIO
                            Telefono = reader.GetString("Telefono"), // <-- CAMBIO
                            Email = reader.GetString("eMail"),
                            Clave = reader.GetString("clave"),
                            Avatar = reader.GetString("avatar"),
                            Rol = reader.GetInt32("rol"),
                        };
                    }
                    connection.Close();
                }
            }
            return u;
        }

        public Usuario? Login(string email, string password)
        {
            var usuario = ObtenerPorEmail(email);
            if (usuario == null) return null;

            byte[] saltBytes = Encoding.ASCII.GetBytes(GlobalSalt);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));


            if (usuario.Clave != hashed) return null;

            return usuario;

        }

        public Usuario? ObtenerPerfilPorEmail(string email)
        {
            Usuario? u = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdUsuario, Nombre, Apellido, email, avatar, rol 
                            FROM usuario WHERE email=@email && existe = 1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        u = new Usuario
                        {
                            IdUsuario = reader.GetInt32("IdUsuario"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido"),
                            Email = reader.GetString("email"),
                            Avatar = reader.GetString("avatar"),
                            Rol = reader.GetInt32("rol"),
                        };
                    }
                    connection.Close();
                }
            }
            return u;
        }

        public IList<Usuario> BuscarPropietariosPorFraccionNombre(string term)
        {
            IList<Usuario> res = new List<Usuario>();
            using (var connection = new MySqlConnection(connectionString))
            {
                // Busca usuarios con Rol=3 (Propietario) cuyo nombre o apellido coincidan
                string sql = @"SELECT IdUsuario, Nombre, Apellido, Dni 
                            FROM usuario 
                            WHERE existe = 1 AND Rol = 3 AND (Nombre LIKE @term OR Apellido LIKE @term)";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@term", $"%{term}%");
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Usuario u = new Usuario
                        {
                            IdUsuario = reader.GetInt32("IdUsuario"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido"),
                            Dni = reader.GetInt32("Dni"),
                        };
                        res.Add(u);
                    }
                    connection.Close();
                }
            }
            return res;
        }

    }
}