using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using Sistema_de_Contratos.Models;
using Sistema_de_Contratos.Services;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sistema_de_Contratos.Vistas
{
    public partial class GestionTrabajadores : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString;
        private List<TrabajadorView> todosTrabajadores = new List<TrabajadorView>();
        private List<Puesto> puestos = new List<Puesto>();

        public GestionTrabajadores()
        {
            InitializeComponent();
            CargarPuestos();
            CargarTrabajadores();
        }

        private void CargarPuestos()
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT IdPuesto, Nombre FROM Puestos WHERE Activo = 1 ORDER BY Nombre";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        puestos.Add(new Puesto
                        {
                            IdPuesto = (int)reader["IdPuesto"],
                            Nombre = reader["Nombre"].ToString()
                        });
                    }
                }
            }catch(SqlException ex)
            {
                if (ex.Number == 4060)
                {
                    MessageBox.Show("Sin Conexion a la base de datos");
                }
                else
                {
                    MessageBox.Show("Error general. \n Contacte al area de TI " + ex.Message + "\n" + ex.Number);
                }
                
            }
            

            // Agregar opción "Todos los puestos"
            puestos.Insert(0, new Puesto { IdPuesto = 0, Nombre = "Todos los puestos" });
            cmbBuscarPuesto.ItemsSource = puestos;
            cmbBuscarPuesto.SelectedIndex = 0;
        }

        private void CargarTrabajadores()
        {
            todosTrabajadores.Clear();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                try
                {
                    connection.Open();
                    string query = @"
                                    SELECT 
                                        T.IdTrabajador,
                                        T.Nombre_Completo AS NombreCompleto,
                                        T.CURP,
                                        P.Nombre AS Puesto,
                                        C.FechaInicio,
                                        C.FechaFin,
                                        C.Sueldo,
                                        CASE 
                                            WHEN C.DatoDeBaja = 1 THEN 'Dado de Baja'
                                            WHEN C.FechaFin < GETDATE() THEN 'Vencido'
                                            ELSE 'Vigente'
                                        END AS EstadoContrato,
                                        CASE
                                            WHEN C.DatoDeBaja = 1 THEN NULL
                                            WHEN C.FechaFin >= GETDATE() THEN DATEDIFF(DAY, GETDATE(), C.FechaFin)
                                            ELSE NULL
                                        END AS DiasRestantes
                                    FROM Trabajador T
                                    INNER JOIN Contrato C ON T.IdTrabajador = C.IdTrabajador AND C.EsContratoActual = 1
                                    INNER JOIN Puestos P ON C.IdPuesto = P.IdPuesto";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        todosTrabajadores.Add(new TrabajadorView
                        {
                            IdTrabajador = (int)reader["IdTrabajador"],
                            NombreCompleto = reader["NombreCompleto"].ToString(),
                            CURP = reader["CURP"].ToString(),
                            Puesto = reader["Puesto"].ToString(),
                            EstadoContrato = reader["EstadoContrato"].ToString(),
                            DiasRestantes = reader["DiasRestantes"] is DBNull ? (int?)null : Convert.ToInt32(reader["DiasRestantes"]),
                            FechaInicio = (DateTime)reader["FechaInicio"],
                            FechaFin = (DateTime)reader["FechaFin"],
                            Sueldo = (decimal)reader["Sueldo"]
                        });
                    }
                }catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
            }

            ActualizarVista();
            ActualizarEstadisticas();
        }

        private void ActualizarVista()
        {
            string filtroNombre = txtBuscarNombre.Text.ToLower();
            int idPuestoFiltro = cmbBuscarPuesto.SelectedValue != null ? (int)cmbBuscarPuesto.SelectedValue : 0;

            var trabajadoresFiltrados = todosTrabajadores.Where(t =>
            (string.IsNullOrEmpty(filtroNombre) || t.NombreCompleto.ToLower().Contains(filtroNombre.ToLower())) &&
            (idPuestoFiltro == 0 || t.Puesto == (cmbBuscarPuesto.SelectedItem as Puesto)?.Nombre)
            ).ToList();


            dgTrabajadores.ItemsSource = trabajadoresFiltrados;
        }

        private void ActualizarEstadisticas()
        {
            txtTotalTrabajadores.Text = todosTrabajadores.Count.ToString();
            txtVigentes.Text = todosTrabajadores.Count(t => t.EstadoContrato == "Vigente").ToString();
            txtVencidos.Text = todosTrabajadores.Count(t => t.EstadoContrato == "Vencido").ToString();
            txtBajas.Text = todosTrabajadores.Count(t => t.EstadoContrato == "Dado de Baja").ToString();
        }

        private void TxtBuscar_KeyUp(object sender, KeyEventArgs e)
        {
            ActualizarVista();
        }

        private void CmbBuscarPuesto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarVista();
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            txtBuscarNombre.Text = string.Empty;
            cmbBuscarPuesto.SelectedIndex = 0;
            ActualizarVista();
        }

        private void BtnRegresar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnDetalles_Click(object sender, RoutedEventArgs e)
        {
            if (dgTrabajadores.SelectedItem is TrabajadorView seleccionado)
            {
                int idTrabajador = seleccionado.IdTrabajador;

                var detalleWindow = new DetalleTrabajador(idTrabajador);
                detalleWindow.ShowDialog(); 
            }
        }

        private void btnGenerarContrato_Click(object sender, RoutedEventArgs e)
        {
            if (dgTrabajadores.SelectedItem is TrabajadorView seleccionado)
            {
                int idTrabajador = seleccionado.IdTrabajador;

                // Paso 1: Obtener los datos del contrato desde el servicio
                var servicioContrato = new ContratoService(); // Ya incluye la conexión
                var modeloContrato = servicioContrato.ObtenerContratoWordModel(idTrabajador);

                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Documentos Word (*.docx)|*.docx",
                    Title = "Selecciona la plantilla de contrato"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string rutaPlantilla = openFileDialog.FileName;
                    DateTime detalle = DateTime.Now;
                    string fecha = detalle.ToString("ddMMyyyy");
                    string nombreArchivo = $"Contrato_{modeloContrato.NombreCompleto.Replace(" ", "_")}_{fecha}.docx";
                    string carpetaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string carpetaContratos = Path.Combine(carpetaDocumentos, "Contratos");

                    // Crea la carpeta si no existe
                    if (!Directory.Exists(carpetaContratos))
                    {
                        Directory.CreateDirectory(carpetaContratos);
                    }

                    // Ruta final del archivo
                    string rutaDestino = Path.Combine(carpetaContratos, nombreArchivo);

                    // Paso 4: Generar el contrato en Word
                    try
                    {
                        var generador = new WordGeneratorService();
                        generador.GenerarContratoWord(rutaPlantilla, modeloContrato, rutaDestino);
                        MessageBox.Show("Contrato generado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al generar el contrato: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecciona un trabajador antes de generar el contrato.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void btnDescargar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Pregunta al usuario qué desea descargar
                var resultado = MessageBox.Show(
                    "¿Deseas exportar solo los trabajadores vigentes?\n\n" +
                    "Presiona 'Sí' para exportar solo vigentes, o 'No' para exportar toda la base de datos.",
                    "Seleccionar tipo de exportación",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question
                );

                // Cancelar si el usuario presiona "Cancelar"
                if (resultado == MessageBoxResult.Cancel)
                    return;

                // Mostrar cuadro para guardar archivo
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Archivo CSV (*.csv)|*.csv",
                    FileName = (resultado == MessageBoxResult.Yes)
                        ? $"Personas_Vigentes_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                        : $"Personas_Todas_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Llama a la función según la elección
                    if (resultado == MessageBoxResult.Yes)
                        ExportarACS_Vigentes(saveFileDialog.FileName);
                    else
                        ExportarACS_Todos(saveFileDialog.FileName);

                    MessageBox.Show("Datos exportados correctamente a CSV", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportarACS_Vigentes(string filePath)
        {
            string query = @"
                SELECT 
                    T.IdTrabajador,
                    T.Nombre_Completo AS NombreCompleto,
                    T.CURP,
                    T.RFC,
                    P.Nombre AS Puesto,
                    CASE 
                        WHEN C.DatoDeBaja = 1 THEN 'Dado de Baja'
                        WHEN C.FechaFin < GETDATE() THEN 'Vencido'
                        ELSE 'Vigente'
                    END AS Estado_Contrato
                FROM Trabajador T
                INNER JOIN Contrato C ON T.IdTrabajador = C.IdTrabajador
                INNER JOIN Puestos P ON C.IdPuesto = P.IdPuesto
                WHERE 
                    C.EsContratoActual = 1
                    AND C.DatoDeBaja = 0
                    AND (C.FechaFin IS NULL OR C.FechaFin >= GETDATE());";

            ExportarGenerico(filePath, query);
        }

        private void ExportarACS_Todos(string filePath)
        {
            string query = @"
                SELECT 
                    T.IdTrabajador,
                    T.Nombre_Completo AS NombreCompleto,
                    T.CURP,
                    T.RFC,
                    P.Nombre AS Puesto,
                    CASE 
                        WHEN C.DatoDeBaja = 1 THEN 'Dado de Baja'
                        WHEN C.FechaFin < GETDATE() THEN 'Vencido'
                        ELSE 'Vigente'
                    END AS Estado_Contrato
                FROM Trabajador T
                INNER JOIN Contrato C ON T.IdTrabajador = C.IdTrabajador
                INNER JOIN Puestos P ON C.IdPuesto = P.IdPuesto;";

            ExportarGenerico(filePath, query);
        }

        private void ExportarGenerico(string filePath, string query)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(query, connection))
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    writer.WriteLine("ID,Nombre Completo,CURP,RFC,Puesto,Estado_Contrato");
                    while (reader.Read())
                    {
                        writer.WriteLine(
                            $"{reader["IdTrabajador"]}," +
                            $"\"{reader["NombreCompleto"]}\"," +
                            $"\"{reader["CURP"]}\"," +
                            $"\"{reader["RFC"]}\"," +
                            $"\"{reader["Puesto"]}\"," +
                            $"\"{reader["Estado_Contrato"]}\""
                        );
                    }
                }
            }
        }

        /*private void ExportarACS(string filePath)
        {
            string query2 = @"
                    SELECT 
                        T.IdTrabajador,
                        T.Nombre_Completo AS NombreCompleto,
                        T.CURP,
                        T.RFC,
                        P.Nombre AS Puesto,
                        CASE 
                            WHEN C.DatoDeBaja = 1 THEN 'Dado de Baja'
                            WHEN C.FechaFin < GETDATE() THEN 'Vencido'
                            ELSE 'Vigente'
                        END AS Estado_Contrato
                    FROM Trabajador T
                    INNER JOIN Contrato C ON T.IdTrabajador = C.IdTrabajador
                    INNER JOIN Puestos P ON C.IdPuesto = P.IdPuesto
                    WHERE 
                        C.EsContratoActual = 1
                        AND C.DatoDeBaja = 0
                        AND (C.FechaFin IS NULL OR C.FechaFin >= GETDATE());";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(query2, connection))
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    writer.WriteLine("ID,Nombre Completo,CURP,RFC,Puesto,Estado_Contrato");

                    while (reader.Read())
                    {
                        writer.WriteLine(
                            $"{reader["IdTrabajador"]}," +
                            $"\"{reader["NombreCompleto"]}\"," +
                            $"\"{reader["CURP"]}\"," +
                            $"\"{reader["RFC"]}\"," +
                            $"\"{reader["Puesto"]}\"," +
                            $"\"{reader["Estado_Contrato"]}\""
                        );
                    }
                }
            }
        }*/


    }
}