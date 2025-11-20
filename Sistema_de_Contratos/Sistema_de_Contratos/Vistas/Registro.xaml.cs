using Microsoft.Data.SqlClient;
using Sistema_de_Contratos.DataBase.UseCases;
using System.Windows;
using System.Windows.Controls;
using static Sistema_de_Contratos.DataBase.UseCases.AddTrabajador;

namespace Sistema_de_Contratos
{
    /// <summary>
    /// Lógica de interacción para Registro.xaml
    /// </summary>
    public partial class Registro : Window
    {
        public int idTrabajador;
        public Registro()
        {
            InitializeComponent();
        }

        private void BtnAsignacionContrato_Click(object sender, RoutedEventArgs e)
        {
            bool bSuccess;
        
            string Nombre_Completo = txtNombreCompleto.Text.Trim();
            string Genero = cmbGenero.Text.Trim();
            string RFC = txtRFC.Text.Trim();
            string CURP = txtCURP.Text.Trim();
            string Estado_Civil = cmbEstadoCivil.Text.Trim();
            string Domicilio_Fiscal = txtDomicilio_Fiscal.Text.Trim();
            string Domicilio_Particular = txtDomicilio_Particular.Text.Trim();
            string Cedula = txtCedula.Text.Trim();

            int edad = ObtenerEdadDesdeCurp(CURP);
            
            if (edad <= 0)
            {
                bSuccess = false;
                MessageBox.Show("CURP inválida o error en el cálculo");
            }
            else
            {
                bSuccess=true;
            }


            var trabajador = new
            {
                Nombre_Completo = Nombre_Completo,
                Sexo = Genero,
                RFC = RFC,
                CURP = CURP,
                Estado_Civil = Estado_Civil,
                Domicilio_Fiscal = Domicilio_Fiscal,
                Domicilio_Particular = Domicilio_Particular,
                Cedula = Cedula

            };

            //MessageBox.Show($"Nombre: {trabajador.Nombre_Completo}\nEd|ad: {trabajador.Edad}");

            if (bSuccess == true)
            {
                try
                {
                    TrabajadorDAO dao = new TrabajadorDAO();
                    idTrabajador = dao.InsertarTrabajador(trabajador);
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

                Asignacion_Contratos ventanaAsignacion_Contratos = new Asignacion_Contratos(idTrabajador);
                ventanaAsignacion_Contratos.Show();
                this.Close();
            }
            
            
        }

        public int ObtenerEdadDesdeCurp(string curp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(curp) || curp.Length < 18)
                    return -1;

                string fechaCurp = curp.Substring(4, 6);
                int año = int.Parse(fechaCurp.Substring(0, 2));
                int mes = int.Parse(fechaCurp.Substring(2, 2));
                int dia = int.Parse(fechaCurp.Substring(4, 2));

                if (mes < 1 || mes > 12 || dia < 1 || dia > 31)
                    return -1;

                // Paso 1: suposición inicial (siglo por carácter 11)
                char sigloIndicador = curp[10];
                int añoCompleto = char.IsDigit(sigloIndicador) ? 2000 + año : 1900 + año;

                // Paso 2: construir fecha tentativa
                if (!DateTime.TryParse($"{añoCompleto}-{mes:D2}-{dia:D2}", out DateTime fechaNacimiento))
                    return -1;

                // Paso 3: calcular edad provisional
                DateTime hoy = DateTime.Today;
                int edad = hoy.Year - fechaNacimiento.Year;
                if (fechaNacimiento > hoy.AddYears(-edad)) edad--;

                // Paso 4: si la edad es absurda, reconsiderar el año al 2000+
                if (edad > 100)
                {
                    int añoRecalculado = 2000 + año;
                    if (DateTime.TryParse($"{añoRecalculado}-{mes:D2}-{dia:D2}", out DateTime fechaRecalculada))
                    {
                        edad = hoy.Year - fechaRecalculada.Year;
                        if (fechaRecalculada > hoy.AddYears(-edad)) edad--;

                        // Solo aceptamos el cambio si la edad ahora tiene sentido
                        if (edad >= 0 && edad <= 100)
                            return edad;
                    }

                    return -1;
                }

                return edad;
            }
            catch
            {
                return -1;
            }
        }






        private void comboGenero_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? genero = (cmbGenero.SelectedItem as ComboBoxItem)?.Content.ToString();

            cmbEstadoCivil.Items.Clear();

            if (genero == "Masculino")
            {
                cmbEstadoCivil.Items.Add("Soltero");
                cmbEstadoCivil.Items.Add("Casado");
                cmbEstadoCivil.Items.Add("Divorciado");
                cmbEstadoCivil.Items.Add("Viudo");
            }
            else if (genero == "Femenino")
            {
                cmbEstadoCivil.Items.Add("Soltera");
                cmbEstadoCivil.Items.Add("Casada");
                cmbEstadoCivil.Items.Add("Divorciada");
                cmbEstadoCivil.Items.Add("Viuda");
            }

            cmbEstadoCivil.SelectedIndex = 0; // Selecciona la primera opción por defecto
        }
    }
}
