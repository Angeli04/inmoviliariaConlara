using MySql.Data.MySqlClient;

namespace Inmobiliaria.Models
{
    public class RepositorioMultas
    {
        private readonly string connectionString;

        public RepositorioMultas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public int Alta(Multas multa)
        {
            int res = -1;
            //hacer transaccion para modificar habilitado de inmueble a false
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Multa
                    (idContrato,fechaMulta, fechaHastaContrato,nuevaFechaHastaContrato,importeCuota,importeMulta,cuotasAdeudadas,pagada,existe,usuarioAlta,usuarioBaja)
                    VALUES (@idContrato,@fechaMulta, @fechaHastaContrato, @nuevaFechaHastaContrato,@importeCuota,@importeMulta,@cuotasAdeudadas,@pagada,@existe,@usuarioAlta,@usuarioBaja);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(sql, connection))
                {

                    command.Parameters.AddWithValue("@idContrato", multa.IdContrato);
                    command.Parameters.AddWithValue("@fechaMulta", multa.FechaMulta);
                    command.Parameters.AddWithValue("@fechaHastaContrato", multa.FechaHastaContrato);
                    command.Parameters.AddWithValue("@nuevaFechaHastaContrato", multa.NuevaFechaHastaContrato);
                    command.Parameters.AddWithValue("@importeCuota", multa.ImporteCuota);
                    command.Parameters.AddWithValue("@importeMulta", multa.ImporteMulta);
                    command.Parameters.AddWithValue("@cuotasAdeudadas", multa.CuotasAdeudadas);
                    command.Parameters.AddWithValue("@pagada", multa.Pagada);
                    command.Parameters.AddWithValue("@existe", 1);
                    command.Parameters.AddWithValue("@usuarioAlta", multa.UsuariAlta);
                    command.Parameters.AddWithValue("@usuarioBaja", multa.UsuarioBaja);


                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    multa.IdMulta = res;
                    connection.Close();
                }
            }
            return res;
        }

        public int Baja(Multas multa)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE multa SET 
                        idContrato=@idContrato, fechaMulta=@fechaMulta, fechaHastaContrato=@fechaHastaContrato, nuevaFechaHastaContrato=@nuevaFechaHastaContrato, importeCuota=@importeCuota, importeMulta=@importeMulta,cuotasAdeudadas=@cuotasAdeudadas,pagada=@pagada,existe=@existe,usuarioAlta=@usuarioAlta,usuarioBaja=@usuarioBaja
                        WHERE IdMulta = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", multa.IdMulta);
                    command.Parameters.AddWithValue("@idContrato", multa.IdContrato);
                    command.Parameters.AddWithValue("@fechaMulta", multa.FechaMulta);
                    command.Parameters.AddWithValue("@fechaHastaContrato", multa.FechaHastaContrato);
                    command.Parameters.AddWithValue("@nuevaFechaHastaContrato", multa.NuevaFechaHastaContrato);
                    command.Parameters.AddWithValue("@importeCuota", multa.ImporteCuota);
                    command.Parameters.AddWithValue("@importeMulta", multa.ImporteMulta);
                    command.Parameters.AddWithValue("@cuotasAdeudadas", multa.CuotasAdeudadas);
                    command.Parameters.AddWithValue("@pagada", multa.Pagada);
                    command.Parameters.AddWithValue("@existe", 0);
                    command.Parameters.AddWithValue("@usuarioAlta", multa.UsuariAlta);
                    command.Parameters.AddWithValue("@usuarioBaja", multa.UsuarioBaja);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public int Modificacion(Multas multa)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE multa SET 
                        idContrato=@idContrato, fechaMulta=@fechaMulta, fechaHastaContrato=@fechaHastaContrato, nuevaFechaHastaContrato=@nuevaFechaHastaContrato, importeCuota=@importeCuota, importeMulta=@importeMulta,cuotasAdeudadas=@cuotasAdeudadas,pagada=@pagada,existe=@existe,usuarioAlta=@usuarioAlta,usuarioBaja=@usuarioBaja
                        WHERE IdMulta = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", multa.IdMulta);
                    command.Parameters.AddWithValue("@idContrato", multa.IdContrato);
                    command.Parameters.AddWithValue("@fechaMulta", multa.FechaMulta);
                    command.Parameters.AddWithValue("@fechaHastaContrato", multa.FechaHastaContrato);
                    command.Parameters.AddWithValue("@nuevaFechaHastaContrato", multa.NuevaFechaHastaContrato);
                    command.Parameters.AddWithValue("@importeCuota", multa.ImporteCuota);
                    command.Parameters.AddWithValue("@importeMulta", multa.ImporteMulta);
                    command.Parameters.AddWithValue("@cuotasAdeudadas", multa.CuotasAdeudadas);
                    command.Parameters.AddWithValue("@pagada", multa.Pagada);
                    command.Parameters.AddWithValue("@existe", 1);
                    command.Parameters.AddWithValue("@usuarioAlta", multa.UsuariAlta);
                    command.Parameters.AddWithValue("@usuarioBaja", multa.UsuarioBaja);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Multas> ObtenerTodos()
        {
            var res = new List<Multas>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdMulta, IdContrato, fechaMulta, fechaHastaContrato, nuevaFechaHastaContrato, importeCuota, 
                            importeMulta, cuotasAdeudadas, pagada FROM multa WHERE existe=1 AND IdMulta = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var multa = new Multas
                        {
                            IdMulta = Convert.ToInt32(reader["IdMulta"]),
                            IdContrato = Convert.ToInt32(reader["idContrato"]),
                            FechaMulta = Convert.ToDateTime(reader["fechaMulta"]),
                            FechaHastaContrato = Convert.ToDateTime(reader["fechaHastaContrato"]),
                            NuevaFechaHastaContrato = Convert.ToDateTime(reader["nuevaFechaHastaContrato"]),
                            ImporteCuota = Convert.ToDecimal(reader["importeCuota"]),
                            ImporteMulta = Convert.ToDecimal(reader["importeMulta"]),
                            CuotasAdeudadas = Convert.ToInt32(reader["cuotasAdeudadas"]),
                            Pagada = Convert.ToBoolean(reader["pagada"])



                        };
                        res.Add(multa);
                    }
                    connection.Close();
                }
            }
            return res;
        }


        public Multas? ObtenerPorId(int id)
        {


            Multas? multa = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdMulta,IdContrato, fechaMulta, fechaHastaContrato, nuevaFechaHastaContrato, importeCuota, importeMulta,cuotasAdeudadas,pagada
                            FROM multa WHERE existe=1
                            WHERE IdMulta = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        multa = new Multas
                        {
                            IdMulta = Convert.ToInt32(reader["IdMulta"]),
                            IdContrato = Convert.ToInt32(reader["idContrato"]),
                            FechaMulta = Convert.ToDateTime(reader["fechaMulta"]),
                            FechaHastaContrato = Convert.ToDateTime(reader["fechaHastaContrato"]),
                            NuevaFechaHastaContrato = Convert.ToDateTime(reader["nuevaFechaHastaContrato"]),
                            ImporteCuota = Convert.ToDecimal(reader["importeCuota"]),
                            ImporteMulta = Convert.ToDecimal(reader["importeMulta"]),
                            CuotasAdeudadas = Convert.ToInt32(reader["cuotasAdeudadas"]),
                            Pagada = Convert.ToBoolean(reader["pagada"])


                        };
                    }
                    connection.Close();
                }
            }
            return multa;
        }
        public Multas? ObtenerPorIdContrato(int id)
        {


            Multas? multa = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdMulta,IdContrato, fechaMulta, fechaHastaContrato, nuevaFechaHastaContrato, importeCuota, importeMulta,cuotasAdeudadas,pagada
                            FROM multa 
                            WHERE IdContrato = @id && existe=1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        multa = new Multas
                        {
                            IdMulta = Convert.ToInt32(reader["IdMulta"]),
                            IdContrato = Convert.ToInt32(reader["idContrato"]),
                            FechaMulta = Convert.ToDateTime(reader["fechaMulta"]),
                            FechaHastaContrato = Convert.ToDateTime(reader["fechaHastaContrato"]),
                            NuevaFechaHastaContrato = Convert.ToDateTime(reader["nuevaFechaHastaContrato"]),
                            ImporteCuota = Convert.ToDecimal(reader["importeCuota"]),
                            ImporteMulta = Convert.ToDecimal(reader["importeMulta"]),
                            CuotasAdeudadas = Convert.ToInt32(reader["cuotasAdeudadas"]),
                            Pagada = Convert.ToBoolean(reader["pagada"])


                        };
                    }
                    connection.Close();
                }
            }
            return multa;
        }
       
        
        public IList<Multas> ObtenerTodosTodos()
        {
            var res = new List<Multas>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT m.IdMulta,
                                m.idContrato,
                                m.fechaMulta,
                                m.fechaHastaContrato,
                                m.nuevaFechaHastaContrato,
                                m.importeCuota,
                                m.importeMulta,
                                m.cuotasAdeudadas,
                                m.pagada,
                                ua.Email AS EmailUsuarioAlta,
                                ub.Email AS EmailUsuarioBaja
                            FROM multa m
                            LEFT JOIN Usuario ua ON ua.IdUsuario = m.UsuarioAlta
                            LEFT JOIN Usuario ub ON ub.IdUsuario = m.UsuarioBaja
                            ORDER BY m.fechaMulta;
                            ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var multa = new Multas
                        {
                            IdMulta = Convert.ToInt32(reader["IdMulta"]),
                            IdContrato = Convert.ToInt32(reader["idContrato"]),
                            FechaMulta = Convert.ToDateTime(reader["fechaMulta"]),
                            FechaHastaContrato = Convert.ToDateTime(reader["fechaHastaContrato"]),
                            NuevaFechaHastaContrato = Convert.ToDateTime(reader["nuevaFechaHastaContrato"]),
                            ImporteCuota = Convert.ToDecimal(reader["importeCuota"]),
                            ImporteMulta = Convert.ToDecimal(reader["importeMulta"]),
                            CuotasAdeudadas = Convert.ToInt32(reader["cuotasAdeudadas"]),
                            Pagada = Convert.ToBoolean(reader["pagada"]),
                            mailUsuarioAlta = Convert.ToString(reader["EmailUsuarioAlta"]),
                            mailUsuarioBaja = Convert.ToString(reader["EmailUsuarioBaja"])
                        };
                        res.Add(multa);
                    }
                    connection.Close();
                }
            }
            return res;
        }

        }
    
}