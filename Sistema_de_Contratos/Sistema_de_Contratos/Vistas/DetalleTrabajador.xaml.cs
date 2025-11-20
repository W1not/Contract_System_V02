using Sistema_de_Contratos.DataBase.UseCases;
using Sistema_de_Contratos.Models;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Sistema_de_Contratos.Vistas;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Configuration;
using System.Data;
using System.Windows.Markup;


namespace Sistema_de_Contratos.Vistas
{
    /// <summary>
    /// Lógica de interacción para DetalleTrabajador.xaml
    /// </summary>
    public partial class DetalleTrabajador : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString;

        private int _idTrabajador;
   
        public DetalleTrabajador(int idTrabajador)
        {
            _idTrabajador = idTrabajador;

            try
            {
                InitializeComponent();
                CargarDetalles();

            }catch(XamlParseException ex)
            {
                MessageBox.Show($"Error al cargar la interfaz: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}",
                          "Error crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void CargarDetalles()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Información del trabajador y su contrato actual
                string queryTrabajador = @"
                    SELECT 
                        t.Nombre_Completo, 
                        t.CURP, 
                        p.Nombre AS Puesto, 
                        c.FechaInicio, 
                        c.MotivoBaja,
                        c.DatoDeBaja,
                        c.FechaBaja,
                        CASE 
                            WHEN c.DatoDeBaja = 1 THEN 'Baja' 
                            WHEN c.FechaFin < GETDATE() THEN 'Caducado'
                            ELSE 'Vigente' 
                        END AS Estado,
                        (SELECT COUNT(*) FROM Contrato WHERE IdTrabajador = t.IdTrabajador) - 1 AS Renovaciones
                    FROM Trabajador t
                    INNER JOIN Contrato c ON t.IdTrabajador = c.IdTrabajador
                    INNER JOIN Puestos p ON c.IdPuesto = p.IdPuesto
                    WHERE t.IdTrabajador = @IdTrabajador
                    AND c.EsContratoActual = 1";

                SqlCommand cmd = new SqlCommand(queryTrabajador, conn);
                cmd.Parameters.AddWithValue("@IdTrabajador", _idTrabajador);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtNombre.Text = reader["Nombre_Completo"].ToString();
                        txtCURP.Text = reader["CURP"].ToString();
                        txtPuesto.Text = reader["Puesto"].ToString();
                        txtFechaInicio.Text = Convert.ToDateTime(reader["FechaInicio"]).ToString("dd/MM/yyyy");
                        txtEstado.Text = reader["Estado"].ToString();
                        txtRenovaciones.Text = reader["Renovaciones"].ToString();
                    }

                    if (Convert.ToInt32(reader["DatoDeBaja"]) == 1)
                    {
                        txtMotivoBaja.Text = reader["MotivoBaja"].ToString();

                        if (reader["FechaBaja"] != DBNull.Value)
                        {
                            txtMotivoBaja.Visibility = Visibility.Visible;

                        }
                        else
                        {
                            txtMotivoBaja.Visibility = Visibility.Collapsed;
                        }
                    }
                }

                // Historial de contratos
                string queryHistorial = @"
                    SELECT 
                        'Contrato' AS TipoContrato,
                        FechaInicio,
                        FechaFin,
                        DATEDIFF(MONTH, FechaInicio, FechaFin) AS Duracion,
                        Sueldo AS Salario,
                        CASE 
                            WHEN DatoDeBaja = 1 THEN 'Baja' 
                            WHEN FechaFin < GETDATE() THEN 'Caducado'
                            ELSE 'Vigente' 
                        END AS Estado
                    FROM Contrato
                    WHERE IdTrabajador = @IdTrabajador
                    ORDER BY FechaInicio DESC";

                SqlCommand cmdHistorial = new SqlCommand(queryHistorial, conn);
                cmdHistorial.Parameters.AddWithValue("@IdTrabajador", _idTrabajador);

                SqlDataAdapter adapter = new SqlDataAdapter(cmdHistorial);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dgContratos.ItemsSource = dt.DefaultView;
            }
        }

        private void Button_ClickEditar(object sender, RoutedEventArgs e)
        {
            int idTrabajador = _idTrabajador;

            var detalleWindow = new EditarTrabajador(idTrabajador);
            detalleWindow.ShowDialog();
        }

        private void Button_ClickContrato(object sender, RoutedEventArgs e)
        {
            var contratoWindow = new GestionContratoWindow(_idTrabajador);
            if (contratoWindow.ShowDialog() == true)
            {
            }
        }

        private void Button_ClickCerrar(object sender, RoutedEventArgs e) => this.Close();

    }
}
