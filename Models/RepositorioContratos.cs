using Inmobiliaria.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace Inmobiliaria.Models
{
    public class RepositorioContratos
    {
        private readonly string connectionString;

        public RepositorioContratos(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public int Alta(Contratos contrato)
        {
            int res = -1;
            //hacer transaccion para modificar habilitado de inmueble a false
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Contratos
                    ( idInquilino, idInmuebles, monto, fechadesde, fechahasta,vigente,cantidadCuotas,cuotasPagas,mesInicio,existe,usuarioAlta,usuarioBaja)
                    VALUES ( @IdInquilino, @idInmuebles,@monto, @fechaDesde, @fechaHasta,@vigente,@cantidadCuotas,@cuotasPagas,@mesInicio,@existe,@usuarioAlta,@usuarioBaja);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInquilino", contrato.IdInquilino);
                    command.Parameters.AddWithValue("@idInmuebles", contrato.IdInmuebles);
                    command.Parameters.AddWithValue("@monto", contrato.Monto);
                    command.Parameters.AddWithValue("@fechaDesde", contrato.FechaDesde);
                    command.Parameters.AddWithValue("@fechaHasta", contrato.FechaHasta);
                    command.Parameters.AddWithValue("@vigente", contrato.Vigente);
                    command.Parameters.AddWithValue("@cantidadCuotas", contrato.CantidadCuotas);
                    command.Parameters.AddWithValue("@cuotasPagas", contrato.CuotasPagas);
                    command.Parameters.AddWithValue("@mesInicio", contrato.MesInicio);
                    command.Parameters.AddWithValue("@existe", contrato.Existe);
                    command.Parameters.AddWithValue("@usuarioAlta", contrato.UsuariAlta);
                    command.Parameters.AddWithValue("@usuarioBaja", contrato.UsuarioBaja);

                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    contrato.IdContrato = res;
                    connection.Close();
                }
            }
            return res;
        }

        public int Baja(Contratos contrato)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Contratos SET 
                        idInquilino=@idInquilino, idInmuebles=@idInmuebles, monto=@monto, fechaDesde=@fechaDesde, fechaHasta=@fechaHasta, vigente=@vigente,cantidadCuotas=@cantidadCuotas,cuotasPagas=@cuotasPagas,mesInicio=@mesInicio, existe=@existe, usuarioBaja=@usuarioBaja
                        WHERE IdContrato = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", contrato.IdContrato);
                    command.Parameters.AddWithValue("@idInquilino", contrato.IdInquilino);
                    command.Parameters.AddWithValue("@idInmuebles", contrato.IdInmuebles);
                    command.Parameters.AddWithValue("@monto", contrato.Monto);
                    command.Parameters.AddWithValue("@fechaDesde", contrato.FechaDesde);
                    command.Parameters.AddWithValue("@fechaHasta", contrato.FechaHasta);
                    command.Parameters.AddWithValue("@vigente", contrato.Vigente);
                    command.Parameters.AddWithValue("@cantidadCuotas", contrato.CantidadCuotas);
                    command.Parameters.AddWithValue("@cuotasPagas", contrato.CuotasPagas);
                    command.Parameters.AddWithValue("@mesInicio", contrato.MesInicio);
                    command.Parameters.AddWithValue("@existe", 0);
                    command.Parameters.AddWithValue("@usuarioBaja", contrato.UsuarioBaja);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public int Modificacion(Contratos contrato)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Contratos SET 
                        idInquilino=@idInquilino, idInmuebles=@idInmuebles, monto=@monto, fechaDesde=@fechaDesde, fechaHasta=@fechaHasta, vigente=@vigente,cantidadCuotas=@cantidadCuotas,
                        cuotasPagas=@cuotasPagas,mesInicio=@mesInicio, existe=@existe, usuarioAlta=@usuarioAlta,usuarioBaja=@usuarioBaja
                        WHERE IdContrato = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", contrato.IdContrato);
                    command.Parameters.AddWithValue("@idInquilino", contrato.IdInquilino);
                    command.Parameters.AddWithValue("@idInmuebles", contrato.IdInmuebles);
                    command.Parameters.AddWithValue("@monto", contrato.Monto);
                    command.Parameters.AddWithValue("@fechaDesde", contrato.FechaDesde);
                    command.Parameters.AddWithValue("@fechaHasta", contrato.FechaHasta);
                    command.Parameters.AddWithValue("@vigente", contrato.Vigente);
                    command.Parameters.AddWithValue("@cantidadCuotas", contrato.CantidadCuotas);
                    command.Parameters.AddWithValue("@cuotasPagas", contrato.CuotasPagas);
                    command.Parameters.AddWithValue("@mesInicio", contrato.MesInicio);
                    command.Parameters.AddWithValue("@existe", contrato.Existe);
                    command.Parameters.AddWithValue("@usuarioAlta", contrato.UsuariAlta);
                    command.Parameters.AddWithValue("@usuarioBaja", contrato.UsuarioBaja);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Contratos> ObtenerTodos()
        {
            var res = new List<Contratos>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdContrato,idInquilino, idInmuebles, monto, fechaDesde, fechaHasta, vigente,cantidadCuotas,cuotasPagas,mesInicio
                            FROM Contratos
                            where vigente=1 && existe = 1
                            ORDER BY fechaDesde";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var contrato = new Contratos
                        {
                            IdContrato = Convert.ToInt32(reader["IdContrato"]),
                            IdInquilino = Convert.ToInt32(reader["idInquilino"]),
                            IdInmuebles = Convert.ToInt32(reader["idInmuebles"]),
                            Monto = Convert.ToDecimal(reader["monto"]),
                            FechaDesde = Convert.ToDateTime(reader["fechaDesde"]),
                            FechaHasta = Convert.ToDateTime(reader["fechaHasta"]),
                            Vigente = Convert.ToBoolean(reader["vigente"]),
                            CantidadCuotas = Convert.ToInt32(reader["cantidadCuotas"]),
                            CuotasPagas = Convert.ToInt32(reader["cuotasPagas"]),
                            MesInicio = Convert.ToInt32(reader["mesInicio"])



                        };
                        res.Add(contrato);
                    }
                    connection.Close();
                }
            }
            return res;
        }
        public IList<Contratos> ObtenerTodosPoIdInmueble(int idInmueble)
        {
            var res = new List<Contratos>();

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdContrato,IdInquilino, idInmuebles, monto, fechaDesde, fechaHasta, vigente,cantidadCuotas,cuotasPagas,mesInicio
                            FROM contratos
                            WHERE IdInmuebles = @id && vigente=1 && contratos.existe = 1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", idInmueble);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var contrato = new Contratos
                        {
                            IdContrato = Convert.ToInt32(reader["IdContrato"]),
                            IdInquilino = Convert.ToInt32(reader["idInquilino"]),
                            IdInmuebles = Convert.ToInt32(reader["idInmuebles"]),
                            Monto = Convert.ToDecimal(reader["monto"]),
                            FechaDesde = Convert.ToDateTime(reader["fechaDesde"]),
                            FechaHasta = Convert.ToDateTime(reader["fechaHasta"]),
                            Vigente = Convert.ToBoolean(reader["vigente"]),
                            CantidadCuotas = Convert.ToInt32(reader["cantidadCuotas"]),
                            CuotasPagas = Convert.ToInt32(reader["cuotasPagas"]),
                            MesInicio = Convert.ToInt32(reader["mesInicio"])



                        };
                        res.Add(contrato);
                    }
                    connection.Close();
                }
            }
            return res;
        }


        public Contratos? ObtenerPorId(int id)
        {


            Contratos? contrato = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdContrato,IdInquilino, idInmuebles, monto, fechaDesde, fechaHasta, vigente,cantidadCuotas,cuotasPagas,mesInicio, usuarioAlta, existe, usuarioBaja
                            FROM contratos
                            WHERE IdContrato = @id && contratos.existe = 1";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        contrato = new Contratos
                        {
                            IdContrato = Convert.ToInt32(reader["IdContrato"]),
                            IdInquilino = Convert.ToInt32(reader["idInquilino"]),
                            IdInmuebles = Convert.ToInt32(reader["idInmuebles"]),
                            Monto = Convert.ToDecimal(reader["monto"]),
                            FechaDesde = Convert.ToDateTime(reader["fechaDesde"]),
                            FechaHasta = Convert.ToDateTime(reader["fechaHasta"]),
                            Vigente = Convert.ToBoolean(reader["vigente"]),
                            CantidadCuotas = Convert.ToInt32(reader["cantidadCuotas"]),
                            CuotasPagas = Convert.ToInt32(reader["cuotasPagas"]),
                            MesInicio = Convert.ToInt32(reader["mesInicio"]),
                            UsuariAlta = Convert.ToInt32(reader["usuarioAlta"]),
                            Existe = Convert.ToBoolean(reader["existe"]),
                            mailUsuarioBaja = Convert.ToString(reader["usuarioBaja"])
                        };
                    }
                    connection.Close();
                }
            }
            return contrato;
        }
        public Contratos obtenerDireccionPrecioInmueblePorIdContrato(int id)
        {
            Contratos? contrato = null;
            //continuar para abajo hacer yoin,para usar en el controller de pagos
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT con.idContrato, inm.direccion, inm.precio
                                FROM contratos con
                                JOIN inmuebles inm ON inm.idInmuebles = con.idInmuebles
                                WHERE con.idContrato = @id && con.existe = 1;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        contrato = new Contratos
                        {
                            IdContrato = Convert.ToInt32(reader["idContrato"]),
                            Precio = Convert.ToDecimal(reader["precio"]),
                            DireccionInmueble = Convert.ToString(reader["direccion"]) ?? "",
                        };
                    }
                    connection.Close();
                }
            }
            return contrato;
        }
        public IList<Contratos> buscarPorFraccionDireccion(string fraccion)
        {
            IList<Contratos> res = new List<Contratos>();

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                            SELECT  inm.Direccion, inm.Precio,cont.idContrato
                            FROM Contratos cont
                            join Inmuebles inm on cont.idInmuebles = inm.idInmuebles

                            WHERE direccion LIKE @fraccion && cont.existe = 1";

                using (var command = new MySqlCommand(sql, connection))
                {
                    // Agregamos los comodines para LIKE
                    command.Parameters.AddWithValue("@fraccion", "%" + fraccion + "%");

                    connection.Open();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {


                        Contratos c = new Contratos
                        {
                            IdContrato = reader.GetInt32("IdContrato"),
                            DireccionInmueble = reader.GetString("Direccion"),
                            Precio = reader.GetDecimal("Precio"),

                        };
                        res.Add(c);




                    }

                    connection.Close();
                }
            }

            return res;
        }


        public int RestarCuotaPaga(Contratos contrato)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Contratos 
                            SET CuotasPagas = CuotasPagas - 1 
                            WHERE IdContrato = @idContrato 
                            AND CuotasPagas > 0";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idContrato", contrato.IdContrato);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Contratos> ObtenerTodosTodos()
        {
            var res = new List<Contratos>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT c.IdContrato,
                                c.idInquilino,
                                c.idInmuebles,
                                c.monto,
                                c.fechaDesde,
                                c.fechaHasta,
                                c.vigente,
                                c.cantidadCuotas,
                                c.cuotasPagas,
                                c.mesInicio,
                                ua.Email AS EmailUsuarioAlta,
                                ub.Email AS EmailUsuarioBaja
                            FROM Contratos c
                            LEFT JOIN Usuario ua ON ua.IdUsuario = c.UsuarioAlta
                            LEFT JOIN Usuario ub ON ub.IdUsuario = c.UsuarioBaja
                            ORDER BY c.fechaDesde;
                            ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var contrato = new Contratos
                        {
                            IdContrato = Convert.ToInt32(reader["IdContrato"]),
                            IdInquilino = Convert.ToInt32(reader["idInquilino"]),
                            IdInmuebles = Convert.ToInt32(reader["idInmuebles"]),
                            Monto = Convert.ToDecimal(reader["monto"]),
                            FechaDesde = Convert.ToDateTime(reader["fechaDesde"]),
                            FechaHasta = Convert.ToDateTime(reader["fechaHasta"]),
                            Vigente = Convert.ToBoolean(reader["vigente"]),
                            CantidadCuotas = Convert.ToInt32(reader["cantidadCuotas"]),
                            CuotasPagas = Convert.ToInt32(reader["cuotasPagas"]),
                            MesInicio = Convert.ToInt32(reader["mesInicio"]),
                            mailUsuarioAlta = Convert.ToString(reader["EmailUsuarioAlta"]),
                            mailUsuarioBaja = Convert.ToString(reader["EmailUsuarioBaja"])



                        };
                        res.Add(contrato);
                    }
                    connection.Close();
                }
            }
            return res;
        }
        public int verificarContratoSolapado(Contratos contrato)
        {
            int resultado = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                var cmd = new MySqlCommand("sp_VerificarContratosSolapados", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_idContrato", contrato.IdContrato);
                cmd.Parameters.AddWithValue("@p_idInmuebles", contrato.IdInmuebles);
                cmd.Parameters.AddWithValue("@p_fechaDesde", contrato.FechaDesde);
                cmd.Parameters.AddWithValue("@p_fechaHasta", contrato.FechaHasta);

                connection.Open();
                resultado = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return resultado;

        }  

        public IList<ContratoDetalleDto> ObtenerContratosVigentesApi(int idPropietario)
        {
            var lista = new List<ContratoDetalleDto>();

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT 
                                c.idContrato, c.monto, c.fechaDesde, c.fechaHasta, c.vigente, 
                                c.cantidadCuotas,
                                CONCAT(t.apellido, ', ', t.nombre) AS NombreInquilino,
                                i.Direccion AS DireccionInmueble
                           FROM contratos c
                           JOIN inquilino t ON c.idInquilino = t.idInquilino
                           JOIN inmuebles i ON c.idInmuebles = i.IdInmuebles
                           WHERE i.IdUsuario = @idPropietario AND c.existe = 1
                           ORDER BY c.fechaDesde DESC";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idPropietario", idPropietario);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        var res = new List<Contratos>();
                        while (reader.Read())
                        {
                            lista.Add(new ContratoDetalleDto
                            {
                                IdContrato = Convert.ToInt32(reader["idContrato"]),
                                Monto = reader.GetDecimal("monto"),
                                FechaDesde = reader.GetDateTime("fechaDesde"),
                                FechaHasta = reader.GetDateTime("fechaHasta"),
                                Vigente = reader.GetInt32("vigente"),
                                CantidadCuotas = reader.GetInt32("cantidadCuotas"),
                                NombreInquilino = reader.GetString("NombreInquilino"),
                                DireccionInmueble = reader.GetString("DireccionInmueble")
                            });
                        }
                        connection.Close();
                    }
                }
            }
            return lista;
        }  
    
    }
    
}
        
    