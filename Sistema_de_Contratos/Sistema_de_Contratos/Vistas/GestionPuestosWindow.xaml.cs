using Sistema_de_Contratos.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Configuration;

namespace Sistema_de_Contratos.Vistas
{
    public partial class GestionPuestosWindow : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString;
        private List<Puesto> puestos = new List<Puesto>();
        private Puesto puestoSeleccionado = null;
        private bool modoEdicion = false;

        public GestionPuestosWindow()
        {
            InitializeComponent();
            CargarPuestos();
        }

        private void CargarPuestos()
        {
            puestos.Clear();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT IdPuesto, Nombre, Activo FROM Puestos ORDER BY Nombre";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        puestos.Add(new Puesto
                        {
                            IdPuesto = (int)reader["IdPuesto"],
                            Nombre = reader["Nombre"].ToString(),
                            Activo = (bool)reader["Activo"]
                        });
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 4060)
                    {
                        MessageBox.Show("Sin Conexion a la base de datos");
                    }

                }



            }

            dgPuestos.ItemsSource = null;
            dgPuestos.ItemsSource = puestos;
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            modoEdicion = false;
            puestoSeleccionado = new Puesto { Activo = true };
            MostrarFormulario();
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgPuestos.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un puesto para editar", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            modoEdicion = true;
            puestoSeleccionado = (Puesto)dgPuestos.SelectedItem;
            MostrarFormulario();
        }

        private void MostrarFormulario()
        {
            txtNombre.Text = puestoSeleccionado.Nombre;
            chkActivo.IsChecked = puestoSeleccionado.Activo;
            pnlFormulario.Visibility = Visibility.Visible;

            // Deshabilitar botones mientras se edita
            btnAgregar.IsEnabled = false;
            btnEditar.IsEnabled = false;
            btnEliminar.IsEnabled = false;
            btnActualizar.IsEnabled = false;
            dgPuestos.IsEnabled = false;
        }

        private void OcultarFormulario()
        {
            pnlFormulario.Visibility = Visibility.Collapsed;

            // Habilitar botones nuevamente
            btnAgregar.IsEnabled = true;
            btnEditar.IsEnabled = true;
            btnEliminar.IsEnabled = true;
            btnActualizar.IsEnabled = true;
            dgPuestos.IsEnabled = true;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del puesto es requerido", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            puestoSeleccionado.Nombre = txtNombre.Text;
            puestoSeleccionado.Activo = chkActivo.IsChecked ?? false;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query;

                    if (modoEdicion)
                    {
                        query = "UPDATE Puestos SET Nombre = @Nombre, Activo = @Activo WHERE IdPuesto = @IdPuesto";
                    }
                    else
                    {
                        query = "INSERT INTO Puestos (Nombre, Activo) VALUES (@Nombre, @Activo)";
                    }

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Nombre", puestoSeleccionado.Nombre);
                    cmd.Parameters.AddWithValue("@Activo", puestoSeleccionado.Activo);

                    if (modoEdicion)
                    {
                        cmd.Parameters.AddWithValue("@IdPuesto", puestoSeleccionado.IdPuesto);
                    }

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Puesto guardado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarPuestos();
                OcultarFormulario();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error al guardar el puesto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            OcultarFormulario();
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgPuestos.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un puesto para eliminar", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var puesto = (Puesto)dgPuestos.SelectedItem;
            var confirmacion = MessageBox.Show($"¿Está seguro de eliminar el puesto '{puesto.Nombre}'?",
                                             "Confirmar eliminación",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM Puestos WHERE IdPuesto = @IdPuesto";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@IdPuesto", puesto.IdPuesto);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Puesto eliminado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarPuestos();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error al eliminar el puesto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarPuestos();
        }
    }
}