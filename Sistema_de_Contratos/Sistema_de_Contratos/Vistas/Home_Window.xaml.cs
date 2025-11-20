using Sistema_de_Contratos.DataBase;
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

namespace Sistema_de_Contratos.Vistas.Legal
{
    /// <summary>
    /// Lógica de interacción para Legal_Window.xaml
    /// </summary>
    public partial class Home_Window : Window
    {
        public Home_Window()
        {
            InitializeComponent();
            Conection conexion = new Conection();
            conexion.ProbarConexion();

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void BtnRegistro_Click(object sender, RoutedEventArgs e)
        {
            Registro ventanaRegistro = new Registro();
            ventanaRegistro.ShowDialog();
        }

        private void BtnGestioTrabajador_Click(object sender, RoutedEventArgs e)
        {

            GestionTrabajadores ventanaGestion = new GestionTrabajadores();
            ventanaGestion.ShowDialog();
        }

        private void BtnGestioPuestos_Click(object sender, RoutedEventArgs e)
        {
            var gestionPuestos = new GestionPuestosWindow();
            gestionPuestos.ShowDialog();
        }

        private void BtnGestiosinContrato_Click(object sender, RoutedEventArgs e)
        {

            var TrabajadoresContrato = new TrabajadoresSinContratoWindow();
            TrabajadoresContrato.ShowDialog();
        }
    }
}
