using Microsoft.Data.SqlClient;
using Sistema_de_Contratos.Models;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Configuration;
using System.Windows;

namespace Sistema_de_Contratos.Vistas
{
    public partial class GestionContratoWindow : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString;
        private int idTrabajador;
        private Contrato contratoActual;
        private bool esContratoNuevo = false;

        public GestionContratoWindow(int idTrabajador)
        {
            InitializeComponent();
            this.idTrabajador = idTrabajador;
            CargarDatosTrabajador();
            CargarPuestos();
            CargarContratoActual();
            chkDarDeBaja.Checked += ChkDarDeBaja_CheckedChanged;
            chkDarDeBaja.Unchecked += ChkDarDeBaja_CheckedChanged;
        }

        private void CargarDatosTrabajador()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                SELECT 
                    T.Nombre_Completo AS NombreCompleto,
                    T.CURP,
                    P.Nombre AS PuestoActual
                FROM Trabajador T
                LEFT JOIN Contrato C ON T.IdTrabajador = C.IdTrabajador
                LEFT JOIN Puestos P ON C.IdPuesto = P.IdPuesto
                WHERE T.IdTrabajador = @IdTrabajador
                AND (C.FechaBaja IS NULL OR C.FechaBaja = (
                    SELECT MAX(FechaBaja) 
                    FROM Contrato 
                    WHERE IdTrabajador = @IdTrabajador
                ))";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdTrabajador", idTrabajador);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    this.DataContext = new
                    {
                        NombreTrabajador = reader["NombreCompleto"].ToString(),
                        CURP = reader["CURP"].ToString(),
                        PuestoActual = reader["PuestoActual"].ToString()
                    };
                }
            }
        }

        private void CargarPuestos()
        {
            List<Puesto> puestos = new List<Puesto>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT IdPuesto, Nombre FROM Puestos WHERE Activo = 1";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    puestos.Add(new Puesto
                    {
                        IdPuesto = (int)reader["IdPuesto"],
                        Nombre = reader["Nombre"].ToString(),
                        Activo = true
                    });
                }
            }

            cmbPuestos.ItemsSource = puestos;
        }

        private void CargarContratoActual()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                SELECT TOP 1 
                    IdContrato,
                    IdPuesto,
                    FechaInicio,
                    FechaFin,
                    Sueldo,
                    DatoDeBaja,
                    MotivoBaja,
                    FechaBaja
                FROM Contrato
                WHERE IdTrabajador = @IdTrabajador
                ORDER BY 
                    CASE WHEN FechaBaja IS NULL THEN 0 ELSE 1 END,
                    FechaInicio DESC";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdTrabajador", idTrabajador);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    contratoActual = new Contrato
                    {
                        IdContrato = (int)reader["IdContrato"],
                        IdPuesto = (int)reader["IdPuesto"],
                        FechaInicio = (DateTime)reader["FechaInicio"],
                        FechaFin = (DateTime)reader["FechaFin"],
                        Sueldo = Convert.ToDecimal(reader["Sueldo"]),
                        DatoDeBaja = (bool)reader["DatoDeBaja"],
                        MotivoBaja = reader["MotivoBaja"]?.ToString(),
                        FechaBaja = reader["FechaBaja"] as DateTime?
                    };

                    cmbPuestos.SelectedValue = contratoActual.IdPuesto;
                    dpFechaInicio.SelectedDate = contratoActual.FechaInicio;
                    dpFechaFin.SelectedDate = contratoActual.FechaFin;
                    txtSueldo.Text = contratoActual.Sueldo.ToString("N2"); 
                    chkDarDeBaja.IsChecked = contratoActual.DatoDeBaja;
                    txtMotivoBaja.Text = contratoActual.MotivoBaja;
                    dpFechaBaja.SelectedDate = contratoActual.FechaBaja;
                }
                else
                {
                    esContratoNuevo = true;
                    btnRenovar.Visibility = Visibility.Collapsed;
                    dpFechaInicio.SelectedDate = DateTime.Today;
                    dpFechaFin.SelectedDate = DateTime.Today.AddYears(1);
                } 
            }
        }

        private void ChkDarDeBaja_CheckedChanged(object sender, RoutedEventArgs e)
        {
            pnlBaja.Visibility = chkDarDeBaja.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            btnRenovar.Visibility = chkDarDeBaja.IsChecked == true ? Visibility.Collapsed : Visibility.Visible;

            if (chkDarDeBaja.IsChecked == true)
            {
                
                dpFechaBaja.SelectedDate = DateTime.Today;
            }
        }

        private void btnRenovar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarDatos()) return;

            var confirmacion = MessageBox.Show(
                "¿Está seguro que desea renovar el contrato?\n\n" +
                $"Nuevo puesto: {cmbPuestos.Text}\n" +
                $"Fecha inicio: {dpFechaInicio.SelectedDate.Value.ToShortDateString()}\n" +
                $"Fecha fin: {dpFechaFin.SelectedDate.Value.ToShortDateString()}\n" +
                $"Sueldo: {txtSueldo.Text}",
                "Confirmar renovación de contrato",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Marcar el contrato actual como histórico
                    string marcarHistoricoQuery = @"
            UPDATE Contrato 
            SET EsContratoActual = 0
            WHERE IdTrabajador = @IdTrabajador AND EsContratoActual = 1";

                    SqlCommand historicoCmd = new SqlCommand(marcarHistoricoQuery, connection);
                    historicoCmd.Parameters.AddWithValue("@IdTrabajador", idTrabajador);
                    historicoCmd.ExecuteNonQuery();

                    // 2. Crear nuevo contrato como actual
                    Contrato nuevoContrato = new Contrato
                    {
                        IdTrabajador = idTrabajador,
                        IdPuesto = (int)cmbPuestos.SelectedValue,
                        FechaInicio = dpFechaInicio.SelectedDate.Value,
                        FechaFin = dpFechaFin.SelectedDate.Value,
                        Sueldo = decimal.Parse(txtSueldo.Text),
                        EsContratoActual = true
                    };

                    string nuevoContratoQuery = @"
            INSERT INTO Contrato (
                IdTrabajador,
                IdPuesto,
                FechaInicio,
                FechaFin,
                Sueldo,
                EsContratoActual
            ) VALUES (
                @IdTrabajador,
                @IdPuesto,
                @FechaInicio,
                @FechaFin,
                @Sueldo,
                1
            )";

                    SqlCommand nuevoCmd = new SqlCommand(nuevoContratoQuery, connection);
                    nuevoCmd.Parameters.AddWithValue("@IdTrabajador", nuevoContrato.IdTrabajador);
                    nuevoCmd.Parameters.AddWithValue("@IdPuesto", nuevoContrato.IdPuesto);
                    nuevoCmd.Parameters.AddWithValue("@FechaInicio", nuevoContrato.FechaInicio);
                    nuevoCmd.Parameters.AddWithValue("@FechaFin", nuevoContrato.FechaFin);
                    nuevoCmd.Parameters.AddWithValue("@Sueldo", nuevoContrato.Sueldo);

                    nuevoCmd.ExecuteNonQuery();

                    MessageBox.Show("Contrato renovado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al renovar el contrato: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDarDeBaja_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMotivoBaja.Text))
            {
                MessageBox.Show("Debe especificar un motivo de baja.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show("¿Está seguro de dar de baja este contrato?", "Confirmar baja",
                                             MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string bajaQuery = @"
                UPDATE Contrato 
                SET DatoDeBaja = 1,
                    FechaBaja = @FechaBaja,
                    MotivoBaja = @MotivoBaja,
                    EsContratoActual = 0
                WHERE IdTrabajador = @IdTrabajador AND EsContratoActual = 1";

                        SqlCommand cmd = new SqlCommand(bajaQuery, connection);
                        cmd.Parameters.AddWithValue("@IdTrabajador", idTrabajador);
                        cmd.Parameters.AddWithValue("@FechaBaja", DateTime.Today);
                        cmd.Parameters.AddWithValue("@MotivoBaja", txtMotivoBaja.Text);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Trabajador dado de baja exitosamente.", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al dar de baja: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarDatos()) return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (esContratoNuevo)
                    {
                        // Código para nuevo contrato (existente)
                    }
                    else
                    {
                        // Actualizar el contrato existente incluyendo el puesto
                        contratoActual.IdPuesto = (int)cmbPuestos.SelectedValue;
                        contratoActual.FechaInicio = dpFechaInicio.SelectedDate.Value;
                        contratoActual.FechaFin = dpFechaFin.SelectedDate.Value;
                        contratoActual.Sueldo = decimal.Parse(txtSueldo.Text);
                        contratoActual.DatoDeBaja = chkDarDeBaja.IsChecked ?? false;
                        contratoActual.MotivoBaja = chkDarDeBaja.IsChecked == true ? txtMotivoBaja.Text : null;
                        contratoActual.FechaBaja = chkDarDeBaja.IsChecked == true ? dpFechaBaja.SelectedDate : null;

                        string query = @"
                UPDATE Contrato SET
                    IdPuesto = @IdPuesto,
                    FechaInicio = @FechaInicio,
                    FechaFin = @FechaFin,
                    Sueldo = @Sueldo,
                    DatoDeBaja = @DatoDeBaja,
                    MotivoBaja = @MotivoBaja,
                    FechaBaja = @FechaBaja
                WHERE IdContrato = @IdContrato";

                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@IdContrato", contratoActual.IdContrato);
                        cmd.Parameters.AddWithValue("@IdPuesto", contratoActual.IdPuesto);
                        cmd.Parameters.AddWithValue("@FechaInicio", contratoActual.FechaInicio);
                        cmd.Parameters.AddWithValue("@FechaFin", contratoActual.FechaFin);
                        cmd.Parameters.AddWithValue("@Sueldo", contratoActual.Sueldo);
                        cmd.Parameters.AddWithValue("@DatoDeBaja", contratoActual.DatoDeBaja);
                        cmd.Parameters.AddWithValue("@MotivoBaja", contratoActual.MotivoBaja ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@FechaBaja", contratoActual.FechaBaja ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Cambios guardados exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar los cambios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarDatos()
        {
            if (cmbPuestos.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un puesto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbPuestos.Focus();
                return false;
            }

            // Resto de validaciones...
            return true;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}