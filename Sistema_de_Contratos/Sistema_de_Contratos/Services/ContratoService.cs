using DocumentFormat.OpenXml.Office.Word;
using Humanizer;
using Microsoft.Data.SqlClient;
using Sistema_de_Contratos.Models;
using Sistema_de_Contratos.Vistas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sistema_de_Contratos.Services
{
    public class ContratoService
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString;
        
        public ContratoWordModel ObtenerContratoWordModel(int idTrabajador)
        {
            var ciudadPorEstado = new Dictionary<string, string>
                {
                    {"Tabasco", "Villahermosa"},
                    {"Puebla" , "Puebla" }
                };

            var ventana = new SeleccionarEstadoWindow();
            bool? resultado = ventana.ShowDialog();
            string ciudadElegida = "Ciudad no definida";
            string estadoElegido = "Estado no definida";

            if (resultado == true)
            {
                estadoElegido = ventana.EstadoSeleccionado.Trim();
                if (!ciudadPorEstado.TryGetValue(estadoElegido, out ciudadElegida))
                {
                    ciudadElegida = "Ciudad no definida";
                }

                MessageBox.Show("Estado elegido: " + estadoElegido + "\n" + "Ciudad Elegida: " + ciudadElegida);
            }

            var modelo = new ContratoWordModel();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                SELECT 
                    t.Nombre_Completo,  
                    t.CURP, 
                    t.Sexo, 
                    t.EstadoCivil, 
                    t.DomicilioParticular, 
                    t.RFC, 
                    t.DomicilioFiscal,
                    c.FechaInicio, 
                    c.FechaFin, 
                    c.Sueldo,
                    p.Nombre AS Puesto
                FROM 
                    Trabajador t
                INNER JOIN 
                    Contrato c ON t.IdTrabajador = c.IdTrabajador
                INNER JOIN 
                    Puestos p ON c.IdPuesto = p.IdPuesto
                WHERE 
                    t.IdTrabajador = @IdTrabajador
                    AND c.DatoDeBaja = 0
                    AND c.FechaFin >= CAST(GETDATE() AS DATE)
                ORDER BY 
                    c.FechaInicio DESC;
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IdTrabajador", idTrabajador);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string nombreCompleto = $"{reader["Nombre_Completo"]}";
                        DateTime fechaInicio = Convert.ToDateTime(reader["FechaInicio"]);
                        DateTime fechaFin = Convert.ToDateTime(reader["FechaFin"]);
                        decimal sueldo = Convert.ToDecimal(reader["Sueldo"]);
                        modelo.NombreCompleto = nombreCompleto;
                        modelo.EstadoCivil = reader["EstadoCivil"].ToString();
                        modelo.CURP = reader["CURP"].ToString();
                        string curp = reader["CURP"].ToString();
                        DateTime fechaNacimiento = ObtenerFechaNacimientoDesdeCURP(curp);
                        modelo.Edad = CalcularEdad(fechaNacimiento);
                        modelo.RFC = reader["RFC"].ToString();
                        modelo.Sexo = reader["Sexo"].ToString();
                        modelo.Pronombre = modelo.Sexo == "Femenino" ? "La trabajadora" : "El trabajador";
                        modelo.PronombreCorto = modelo.Sexo == "Femenino" ? "la" : "el";
                        modelo.FechaInicio = $"el día {fechaInicio:dd} del mes de {fechaInicio.ToString("MMMM", new CultureInfo("es-MX"))} del año {fechaInicio:yyyy}";
                        modelo.FechaFin = $"el día {fechaFin:dd} del mes de {fechaFin.ToString("MMMM", new CultureInfo("es-MX"))} del año {fechaFin:yyyy}";
                        modelo.DuracionContrato = CalcularDuracion(fechaInicio, fechaFin);
                        modelo.Puesto = reader["Puesto"].ToString();
                        modelo.Sueldo = sueldo;
                        modelo.SueldoLetra = ConvertirSueldoALetra(sueldo);
                        modelo.DomicilioParticular = reader["DomicilioParticular"].ToString();
                        modelo.DomicilioFiscal = reader["DomicilioFiscal"].ToString();
                        modelo.Estado = estadoElegido;
                        modelo.Ciudad = ciudadElegida;
                    }
                }
            }

            return modelo;
        }

        private int CalcularEdad(DateTime nacimiento)
        {
            var hoy = DateTime.Today;
            int edad = hoy.Year - nacimiento.Year;
            if (nacimiento > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        private string CalcularDuracion(DateTime inicio, DateTime fin)
        {
            int meses = ((fin.Year - inicio.Year) * 12) + fin.Month - inicio.Month;
            int dias = (fin - inicio.AddMonths(meses)).Days;
            return $"{meses} meses y {dias} días";
        }

        private string ConvertirSueldoALetra(decimal sueldo)
            {
                int parteEntera = (int)Math.Floor(sueldo);
                int centavos = (int)((sueldo - parteEntera) * 100);

                // Convierte números a palabras en español
                string palabrasEntero = parteEntera.ToWords(new CultureInfo("es"));
                string palabrasCentavos = centavos.ToString();

                return $"{palabrasEntero} pesos {centavos}/100 M/N";
            }

        private DateTime ObtenerFechaNacimientoDesdeCURP(string curp)
        {
            string fecha = curp.Substring(4, 6); // AAMMDD
            int anio = int.Parse(fecha.Substring(0, 2));
            int mes = int.Parse(fecha.Substring(2, 2));
            int dia = int.Parse(fecha.Substring(4, 2));

            anio += (anio < 30) ? 2000 : 1900;

            return new DateTime(anio, mes, dia);
        }

    }



}
