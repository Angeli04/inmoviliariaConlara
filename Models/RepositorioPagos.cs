using MySql.Data.MySqlClient;

namespace Inmobiliaria.Models
{
    public class RepositorioPagos
    {
        private readonly string connectionString;

        public RepositorioPagos(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public int Alta(Pagos pago)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Pagos
                    ( idContratos, fechaPago, importe,concepto,numeroCuota,mesPago,existe,usuarioAlta,usuarioBaja)
                    VALUES ( @IdContratos, @fechaPago,@importe,@concepto,@numeroCuota,@mesPago,@existe,@usuarioAlta,@usuarioBaja);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idContratos", pago.IdContratos);
                    command.Parameters.AddWithValue("@fechaPago", pago.FechaPago);
                    command.Parameters.AddWithValue("@importe", pago.Importe);
                    command.Parameters.AddWithValue("@concepto", pago.Concepto);
                    command.Parameters.AddWithValue("@numeroCuota", pago.NumeroCuota);
                    command.Parameters.AddWithValue("@mesPago", pago.MesPago);
                    command.Parameters.AddWithValue("@existe", 1);
                    command.Parameters.AddWithValue("@usuarioAlta", pago.UsuariAlta);
                    command.Parameters.AddWithValue("@usuarioBaja", pago.UsuarioBaja);
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    pago.IdPagos = res;
                    connection.Close();
                }
            }
            return res;
        }

        public int Baja(Pagos pago)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Pagos SET 
                        idContratos=@idContratos, fechaPago=@fechaPago, importe=@importe,concepto=@concepto,numeroCuota=@numeroCuota,mesPago=@mesPago,existe=@existe,usuarioBaja=@usuarioBaja
                        WHERE IdPagos = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", pago.IdPagos);
                    command.Parameters.AddWithValue("@idContratos", pago.IdContratos);
                    command.Parameters.AddWithValue("@fechaPago", pago.FechaPago);
                    command.Parameters.AddWithValue("@importe", pago.Importe);
                    command.Parameters.AddWithValue("@concepto", pago.Concepto);
                    command.Parameters.AddWithValue("@numeroCuota", pago.NumeroCuota);
                    command.Parameters.AddWithValue("@mesPago", pago.MesPago);
                    command.Parameters.AddWithValue("@existe", 0);
                    //command.Parameters.AddWithValue("@usuarioAlta", pago.UsuariAlta);
                    command.Parameters.AddWithValue("@usuarioBaja", pago.UsuarioBaja);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public int Modificacion(Pagos pago)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Pagos SET 
                        idContratos=@idContratos, fechaPago=@fechaPago, importe=@importe,concepto=@concepto,numeroCuota=@numeroCuota,mesPago=@mesPago,existe=@existe,usuarioAlta=@usuarioAlta,usuarioBaja=@usuarioBaja
                        WHERE IdPagos = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", pago.IdPagos);
                    command.Parameters.AddWithValue("@idContratos", pago.IdContratos);
                    command.Parameters.AddWithValue("@fechaPago", pago.FechaPago);
                    command.Parameters.AddWithValue("@importe", pago.Importe);
                    command.Parameters.AddWithValue("@concepto", pago.Concepto);
                    command.Parameters.AddWithValue("@numeroCuota", pago.NumeroCuota);
                    command.Parameters.AddWithValue("@mesPago", pago.MesPago);
                    command.Parameters.AddWithValue("@existe", 1);
                    command.Parameters.AddWithValue("@usuarioAlta", pago.UsuariAlta);
                    command.Parameters.AddWithValue("@usuarioBaja", pago.UsuarioBaja);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Pagos> ObtenerTodos()
        {
            var res = new List<Pagos>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
            SELECT p.IdPagos, p.idContratos, p.fechaPago, p.importe,p.concepto,p.numeroCuota,mesPago,
                   i.Direccion AS DireccionInmueble
            FROM pagos p
            INNER JOIN contratos c ON p.idContratos = c.IdContrato
            INNER JOIN inmuebles i ON c.IdInmuebles = i.idInmuebles
            WHERE p.existe=1
            ORDER BY p.idPagos;
        ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var pago = new Pagos
                        {
                            IdPagos = Convert.ToInt32(reader["IdPagos"]),
                            IdContratos = Convert.ToInt32(reader["idContratos"]),
                            FechaPago = Convert.ToDateTime(reader["fechaPago"]),
                            Importe = Convert.ToDecimal(reader["importe"]),
                            DireccionInmueble = reader["DireccionInmueble"].ToString(),
                            Concepto = reader["concepto"].ToString() ?? string.Empty,
                            NumeroCuota = Convert.ToInt32(reader["numeroCuota"]),
                            MesPago = Convert.ToInt32(reader["mesPago"]),

                        };

                        res.Add(pago);
                    }
                    connection.Close();
                }
            }
            return res;
        }


        public Pagos? ObtenerPorId(int id)

        {


            Pagos? pago = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdPagos,IdContratos,fechaPago, importe,concepto,numeroCuota,mesPago
                            FROM pagos
                            WHERE IdPagos = @id && existe=1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        pago = new Pagos
                        {
                            IdPagos = Convert.ToInt32(reader["IdPagos"]),
                            IdContratos = Convert.ToInt32(reader["idContratos"]),
                            FechaPago = Convert.ToDateTime(reader["fechaPago"]),
                            Importe = Convert.ToDecimal(reader["importe"]),
                            Concepto = reader["concepto"].ToString() ?? string.Empty,
                            NumeroCuota = Convert.ToInt32(reader["numeroCuota"]),
                            MesPago = Convert.ToInt32(reader["mesPago"])
                        };
                    }
                    connection.Close();
                }
            }
            return pago;
        }

        public IList<Pagos> obtenerPorInquilino(int idInquiloino)
        {
            var res = new List<Pagos>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
            SELECT p.idPagos, p.idContratos, p.fechaPago, p.importe,p.concepto,p.numeroCuota,p.mesPago,
                   i.Direccion AS DireccionInmueble
            FROM pagos p
            INNER JOIN contratos c ON p.idContratos = c.IdContrato
            INNER JOIN inmuebles i ON c.IdInmuebles = i.idInmuebles
            WHERE c.idInquilino = @idInquilino && p.existe=1 && p.fechaPago IS NOT NULL AND p.fechaPago != '0000-00-00'
            ORDER BY p.fechaPago;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInquilino", idInquiloino);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var pago = new Pagos
                        {
                            IdPagos = Convert.ToInt32(reader["idPagos"]),
                            IdContratos = Convert.ToInt32(reader["idContratos"]),
                            FechaPago = Convert.ToDateTime(reader["fechaPago"]),
                            Importe = Convert.ToDecimal(reader["importe"]),
                            DireccionInmueble = reader["DireccionInmueble"].ToString(),
                            Concepto = reader["concepto"].ToString() ?? string.Empty,
                            NumeroCuota = Convert.ToInt32(reader["numeroCuota"]),
                            MesPago = Convert.ToInt32(reader["mesPago"])
                        };

                        res.Add(pago);
                    }
                    connection.Close();
                }
                return res;
            }
        }
        public IList<Pagos> ObtenerPaginado(int pageNumber, int pageSize)
        {
            IList<Pagos> res = new List<Pagos>();
            using (var connection = new MySqlConnection(connectionString))
            {

                int offset = (pageNumber - 1) * pageSize;
                string sql = @"
                                        SELECT p.IdPagos, p.idContratos, p.fechaPago, p.importe,p.concepto,p.numeroCuota,mesPago,
                                            i.Direccion AS DireccionInmueble
                                        FROM pagos p
                                        INNER JOIN contratos c ON p.idContratos = c.IdContrato
                                        INNER JOIN inmuebles i ON c.IdInmuebles = i.idInmuebles
                                        WHERE p.existe=1
                                        ORDER BY p.idPagos
                                         LIMIT @pageSize OFFSET @offset";



                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@pageSize", pageSize);
                    command.Parameters.AddWithValue("@offset", offset);

                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Pagos p = new Pagos
                        {
                            IdPagos = Convert.ToInt32(reader["IdPagos"]),
                            IdContratos = Convert.ToInt32(reader["idContratos"]),
                            FechaPago = Convert.ToDateTime(reader["fechaPago"]),
                            Importe = Convert.ToDecimal(reader["importe"]),
                            DireccionInmueble = reader["DireccionInmueble"].ToString(),
                            Concepto = reader["concepto"].ToString() ?? string.Empty,
                            NumeroCuota = Convert.ToInt32(reader["numeroCuota"]),
                            MesPago = Convert.ToInt32(reader["mesPago"]),
                        };
                        res.Add(p);
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public int ContarPagos()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(*) FROM pagos WHERE existe=1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public IList<Pagos> ObtenerTodosTodos()
        {
            var res = new List<Pagos>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT p.IdPagos, 
                        p.idContratos, 
                        p.fechaPago, 
                        p.importe,
                        p.concepto,
                        p.numeroCuota,
                        p.mesPago,
                        ua.Email AS EmailUsuarioAlta,
                        ub.Email AS EmailUsuarioBaja,
                        i.Direccion AS DireccionInmueble
                    FROM pagos p
                    INNER JOIN contratos c ON p.idContratos = c.IdContrato
                    INNER JOIN inmuebles i ON c.IdInmuebles = i.idInmuebles
                    LEFT JOIN Usuario ua ON ua.IdUsuario = p.UsuarioAlta
                    LEFT JOIN Usuario ub ON ub.IdUsuario = p.UsuarioBaja
                    WHERE p.fechaPago IS NOT NULL 
                    AND p.fechaPago != '0000-00-00'
                    ORDER BY p.idPagos;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var pago = new Pagos
                        {
                            IdPagos = Convert.ToInt32(reader["IdPagos"]),
                            IdContratos = Convert.ToInt32(reader["idContratos"]),
                            FechaPago = Convert.ToDateTime(reader["fechaPago"]),
                            Importe = Convert.ToDecimal(reader["importe"]),
                            DireccionInmueble = reader["DireccionInmueble"].ToString(),
                            Concepto = reader["concepto"].ToString() ?? string.Empty,
                            NumeroCuota = Convert.ToInt32(reader["numeroCuota"]),
                            MesPago = Convert.ToInt32(reader["mesPago"]),
                            mailUsuarioAlta = Convert.ToString(reader["EmailUsuarioAlta"]),
                            mailUsuarioBaja = Convert.ToString(reader["EmailUsuarioBaja"])

                        };

                        res.Add(pago);
                    }
                    connection.Close();
                }
            }
            return res;
        }



        public List<Pagos> ObtenerPagosPorContrato(int idContrato) 
        {
            var lista = new List<Pagos>(); // Devuelve una lista de 'Pagos' (plural)
            using (var connection = new MySqlConnection(connectionString))
            {
                // Esta es la consulta SQL que tú proporcionaste (corregida)
                string sql = @"SELECT 
                                p.idPagos, p.idContratos, p.fechaPago, p.importe, 
                                p.concepto, p.numeroCuota, p.mesPago, p.existe
                           FROM pagos p
                           WHERE p.idContratos = @idContratos AND p.existe = 1
                           ORDER BY p.numeroCuota ASC"; 

                using (var command = new MySqlCommand(sql, connection))
                {
                    // Usa el nombre de parámetro correcto
                    command.Parameters.AddWithValue("@idContratos", idContrato);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Crea un nuevo objeto 'Pagos' (plural)
                            lista.Add(new Pagos
                            {
                                IdPagos = reader.GetInt32("idPagos"),
                                IdContratos = reader.GetInt32("idContratos"),
                                FechaPago = reader.GetDateTime("fechaPago"),
                                Importe = reader.GetDecimal("importe"),
                                Concepto = reader.IsDBNull(reader.GetOrdinal("concepto")) ? null : reader.GetString("concepto"),
                                NumeroCuota = reader.GetInt32("numeroCuota"),
                                MesPago = reader.GetInt32("mesPago"),
                                Existe = reader.GetBoolean("existe")
                                
                                // Nota: Los otros campos de tu clase Pagos.cs 
                                // (como UsuariAlta, mailUsuarioAlta, etc.)
                                // no se rellenan aquí porque no los pedimos en el SQL,
                                // y la app de Android no los necesita.
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}
    

        
    

