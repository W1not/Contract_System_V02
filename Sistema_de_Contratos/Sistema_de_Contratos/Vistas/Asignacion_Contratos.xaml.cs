using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Sistema_de_Contrato.DataBase.UseCases.AsignarContrato;
using static Sistema_de_Contratos.DataBase.UseCases.AddTrabajador;


namespace Sistema_de_Contratos
{
    /// <summary>
    /// Lógica de interacción para Asignacion_Contratos.xaml
    /// </summary>
    public partial class Asignacion_Contratos : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString;

        private int _idTrabajador;

        public Asignacion_Contratos(int idTrabajador)
        {
            _idTrabajador = idTrabajador;
            InitializeComponent();
            CargarPuestos();

        }

        private void CargarPuestos()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Nombre FROM Puestos ORDER BY Nombre";

                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        comboCargos.Items.Add(reader["Nombre"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar puestos: " + ex.Message);
            }
        }


        private void BtnAsignar_Contrato_Click(object sender, RoutedEventArgs e)
        {
            bool bSuccess;
            DateTime fechaInicio;
            DateTime fechaFin;

            // Validar fechas
            if (!DateTime.TryParse(dpFechaInicioContrato.Text, out fechaInicio) ||
                !DateTime.TryParse(dpFechaFin.Text, out fechaFin))
            {
                MessageBox.Show("Por favor ingresa fechas válidas.");
                return;
            }

            // Calcular meses y días
            int meses = 0, dias = 0;
            try
            {
                var duracion = CalcularDuracion(fechaInicio, fechaFin);
                meses = duracion.Meses;
                dias = duracion.Dias;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            string puesto = comboCargos.Text.Trim();
            string sueldo = txtSueldo.Text.Trim();

            var contrato = new
            {
                Fecha_de_Inicio = fechaInicio,
                Fecha_de_Fin = fechaFin,
                Puesto = puesto,
                Sueldo = sueldo
            };

            try
            {
                AsignarContratoDao dao = new AsignarContratoDao();
                dao.CrearContrato(contrato, _idTrabajador);
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error SQL:");
                Console.WriteLine($"Número de error: {ex.Number}"); // Código específico de SQL Server
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"Línea afectada: {ex.LineNumber}"); // Útil si usas procedimientos almacenados
                Console.WriteLine("Detalles:");
                foreach (SqlError error in ex.Errors) // SQL Server puede devolver múltiples errores
                {
                    Console.WriteLine($"- {error.Message}");
                }
            }

            this.Close();
        }

        private (int Meses, int Dias) CalcularDuracion(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaFin < fechaInicio)
                throw new ArgumentException("La fecha de fin no puede ser menor que la fecha de inicio.");

            int meses = ((fechaFin.Year - fechaInicio.Year) * 12) + fechaFin.Month - fechaInicio.Month;

            if (fechaFin.Day < fechaInicio.Day)
            {
                meses--;
                DateTime fechaIntermedia = fechaInicio.AddMonths(meses);
                int dias = (fechaFin - fechaIntermedia).Days;
                return (meses, dias);
            }
            else
            {
                DateTime fechaIntermedia = fechaInicio.AddMonths(meses);
                int dias = (fechaFin - fechaIntermedia).Days;
                return (meses, dias);
            }
        }


        private void txtSueldo_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permitir solo números y UN punto decimal
            TextBox? textBox = sender as TextBox;

            // Simular el resultado si se añadiera este nuevo carácter
            string futuroTexto = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            // Validar con regex: solo dígitos y un punto decimal opcional
            e.Handled = !Regex.IsMatch(futuroTexto, @"^\d*\.?\d*$");
        }

        private void txtSueldo_LostFocus(object sender, RoutedEventArgs e)
        {

            if (!decimal.TryParse(txtSueldo.Text, out _))
            {
                MessageBox.Show("Ingrese un número válido.");
                txtSueldo.Text = string.Empty;
            }
        }

        private void Omitir_Registro_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
