using System;
using System.Collections.Generic;
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
    /// Lógica de interacción para SeleccionarEstadoWindow.xaml
    /// </summary>
    public partial class SeleccionarEstadoWindow : Window
    {
        public string EstadoSeleccionado { get; private set; }

        public SeleccionarEstadoWindow()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (cbEstados.SelectedItem is ComboBoxItem item)
            {
                EstadoSeleccionado = item.Content.ToString();
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un estado.");
            }
        }
    }

}
