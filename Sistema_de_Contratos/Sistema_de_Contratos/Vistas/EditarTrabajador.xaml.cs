using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sistema_de_Contratos.Vistas
{
    /// <summary>
    /// Lógica de interacción para EditarTrabajador.xaml
    /// </summary>
    public partial class EditarTrabajador : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString;
        private int idTrabajador;

        public EditarTrabajador(int idTrabajador)
        {
            InitializeComponent();
            this.idTrabajador = idTrabajador;
            CargarDatosTrabajador();
        }

        private void CargarDatosTrabajador()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                SELECT 
                    Nombre_Completo,
                    Sexo,
                    CURP,
                    RFC,
                    EstadoCivil,
                    Cedula,
                    DomicilioParticular,
                    DomicilioFiscal
                FROM Trabajador
                WHERE IdTrabajador = @IdTrabajador";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdTrabajador", idTrabajador);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    txtNombre.Text = reader["Nombre_Completo"].ToString();
                    txtCURP.Text = reader["CURP"].ToString();
                    txtRFC.Text = reader["RFC"].ToString();
                    txtEstadoCivil.Text = reader["EstadoCivil"].ToString();
                    txtCedula.Text = reader["Cedula"].ToString();
                    txtDomicilioParticular.Text = reader["DomicilioParticular"].ToString();
                    txtDomicilioFiscal.Text = reader["DomicilioFiscal"].ToString();

                    // Seleccionar el sexo correcto en el ComboBox
                    string sexo = reader["Sexo"].ToString();
                    foreach (ComboBoxItem item in cmbSexo.Items)
                    {
                        if (item.Content.ToString() == sexo)
                        {
                            cmbSexo.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (ValidarDatos())
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = @"
                        UPDATE Trabajador SET
                            Nombre_Completo = @Nombre,
                            Sexo = @Sexo,
                            CURP = @CURP,
                            RFC = @RFC,
                            EstadoCivil = @EstadoCivil,
                            Cedula = @Cedula,
                            DomicilioParticular = @DomicilioParticular,
                            DomicilioFiscal = @DomicilioFiscal
                        WHERE IdTrabajador = @IdTrabajador";

                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@IdTrabajador", idTrabajador);
                        cmd.Parameters.AddWithValue("@Nombre", txtNombre.Text);
                        cmd.Parameters.AddWithValue("@Sexo", ((ComboBoxItem)cmbSexo.SelectedItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@CURP", txtCURP.Text);
                        cmd.Parameters.AddWithValue("@RFC", txtRFC.Text);
                        cmd.Parameters.AddWithValue("@EstadoCivil", txtEstadoCivil.Text);
                        cmd.Parameters.AddWithValue("@Cedula", txtCedula.Text);
                        cmd.Parameters.AddWithValue("@DomicilioParticular", txtDomicilioParticular.Text);
                        cmd.Parameters.AddWithValue("@DomicilioFiscal", txtDomicilioFiscal.Text);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Datos del trabajador actualizados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.DialogResult = true;
                            this.Close();
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error al actualizar los datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre completo es requerido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCURP.Text) || txtCURP.Text.Length != 18)
            {
                MessageBox.Show("La CURP debe tener exactamente 18 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCURP.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtRFC.Text) || txtRFC.Text.Length != 15)
            {
                MessageBox.Show("El RFC debe tener exactamente 15 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtRFC.Focus();
                return false;
            }

            if (cmbSexo.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un sexo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbSexo.Focus();
                return false;
            }

            return true;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
