using Microsoft.Data.SqlClient;
using Sistema_de_Contratos.DataBase;
using Sistema_de_Contratos.DataBase.UseCases;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media.Animation;


namespace Sistema_de_Contrato.DataBase.UseCases
{
    internal class AsignarContrato
    {
        public class AsignarContratoDao
        {
            private string connectionString = ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString;
            public void CrearContrato(dynamic contrato, int idTrabajador)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                    try
                    {
                        connection.Open();
                        //Obtencion del puesto
                        string queryPuesto = "SELECT IdPuesto FROM Puestos WHERE Nombre = @Nombre";
                        SqlCommand cmdPuesto = new SqlCommand(queryPuesto, connection);
                        cmdPuesto.Parameters.AddWithValue("@Nombre", contrato.Puesto);

                        object resultado = cmdPuesto.ExecuteScalar();
                        if (resultado == null)
                        {
                            MessageBox.Show("El puesto seleccionado no existe en la base de datos.");
                            return;
                        }

                        int idPuesto = Convert.ToInt32(resultado);

                        string queryInsertarContrato = @"INSERT INTO Contrato (IdTrabajador, IdPuesto, FechaInicio, FechaFin, Sueldo) 
                                             VALUES (@IdTrabajador, @IdPuesto, @FechaInicio, @FechaFin, @Sueldo)";

                        SqlCommand cmdContrato = new SqlCommand(queryInsertarContrato, connection);
                        cmdContrato.Parameters.AddWithValue("IdTrabajador", idTrabajador);
                        cmdContrato.Parameters.AddWithValue("@IdPuesto", idPuesto);
                        cmdContrato.Parameters.AddWithValue("FechaInicio", contrato.Fecha_de_Inicio);
                        cmdContrato.Parameters.AddWithValue("FechaFin", contrato.Fecha_de_Fin);
                        cmdContrato.Parameters.AddWithValue("Sueldo", contrato.Sueldo);

                        int filasAfectadas = cmdContrato.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                            MessageBox.Show("Contrato asignado correctamente al trabajador : " + idTrabajador);
                        else
                            MessageBox.Show("No se pudo generar el contrato");

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al Asignar contrato: " + ex.Message);
                    }
            }
        }
    }
}