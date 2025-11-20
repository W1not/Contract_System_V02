using Microsoft.Data.SqlClient;
using Sistema_de_Contratos.Models;
using System.Configuration;
using System.Windows;


namespace Sistema_de_Contratos.Vistas
{
    /// <summary>
    /// Lógica de interacción para TrabajadoresSinContratoWindow.xaml
    /// </summary>
    public partial class TrabajadoresSinContratoWindow : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString;
        private List<TrabajadorView> trabajadoresSinContrato = new List<TrabajadorView>();

        public TrabajadoresSinContratoWindow()
        {
            InitializeComponent();
            CargarTrabajadoresSinContrato();
        }

        private void CargarTrabajadoresSinContrato()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                    SELECT 
                        T.IdTrabajador,
                        T.Nombre_Completo AS NombreCompleto,
                        T.CURP
                    FROM Trabajador T
                    WHERE NOT EXISTS (
                        SELECT 1 FROM Contrato C 
                        WHERE C.IdTrabajador = T.IdTrabajador 
                        AND C.EsContratoActual = 1
                    )
                    ORDER BY T.Nombre_Completo";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        trabajadoresSinContrato.Add(new TrabajadorView
                        {
                            IdTrabajador = (int)reader["IdTrabajador"],
                            NombreCompleto = reader["NombreCompleto"].ToString(),
                            CURP = reader["CURP"].ToString()
                        });
                    }
                }

                dgTrabajadores.ItemsSource = trabajadoresSinContrato;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar trabajadores: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void BtnAsignarContrato_Click(object sender, RoutedEventArgs e)
        {
            var trabajadorSeleccionado = dgTrabajadores.SelectedItem as TrabajadorView;
            if (trabajadorSeleccionado != null)
            {
                
                var ventanaContrato = new Asignacion_Contratos(trabajadorSeleccionado.IdTrabajador);
                ventanaContrato.Owner = this;
                ventanaContrato.ShowDialog();

                // Recargar después de asignar contrato
                if (ventanaContrato.DialogResult == true)
                {
                    CargarTrabajadoresSinContrato();
                }
            }
        }
    }
}
