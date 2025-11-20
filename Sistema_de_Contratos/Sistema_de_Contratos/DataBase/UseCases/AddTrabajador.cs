using Microsoft.Data.SqlClient;
using System.Configuration;
namespace Sistema_de_Contratos.DataBase.UseCases
{
    internal class AddTrabajador
    {
        public class TrabajadorDAO
        {
            private string connectionString = ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString;

            public int InsertarTrabajador(dynamic trabajador)
            {

                int idGenerado = 0;

                string query = @"
                        INSERT INTO Trabajador (
                        Nombre_Completo, 
                        Sexo, 
                        RFC, 
                        CURP, 
                        EstadoCivil, 
                        DomicilioFiscal, 
                        DomicilioParticular, 
                        Cedula
                    ) VALUES (
                        @Nombre_Completo, 
                        @Sexo, 
                        @RFC, 
                        @CURP, 
                        @EstadoCivil, 
                        @DomicilioFiscal, 
                        @DomicilioParticular, 
                        @Cedula
                    );
                        SELECT SCOPE_IDENTITY();";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@Nombre_Completo", trabajador.Nombre_Completo);
                    command.Parameters.AddWithValue("@Sexo", trabajador.Sexo);
                    command.Parameters.AddWithValue("@RFC", trabajador.RFC);
                    command.Parameters.AddWithValue("@CURP", trabajador.CURP);
                    command.Parameters.AddWithValue("@EstadoCivil", trabajador.Estado_Civil);
                    command.Parameters.AddWithValue("@DomicilioFiscal", trabajador.Domicilio_Fiscal);
                    command.Parameters.AddWithValue("@DomicilioParticular", trabajador.Domicilio_Particular);
                    command.Parameters.AddWithValue("@Cedula", trabajador.Cedula);

                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            idGenerado = Convert.ToInt32(result);
                            Console.WriteLine("Trabajador registrado correctamente con ID: " + idGenerado);
                        }
                        else
                        {
                            Console.WriteLine("No se devolvio el ID del trabajador");
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Error en la base de datos: " + ex.Message);
                    }
                }

                return idGenerado;

            }
        }
    }
}
