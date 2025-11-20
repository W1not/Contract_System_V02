using Sistema_de_Contratos.DataBase;
using Sistema_de_Contratos.Vistas.Legal;
using System.Windows;
using System.Windows.Controls;


namespace Sistema_de_Contratos
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Conection conexion = new Conection();
            conexion.ProbarConexion();

        }

        private bool TryAuthenticate(string user, string pass, out string error)
        {
            error = string.Empty;

            if (string.IsNullOrEmpty(user))
            {
                error = "EL usuario es requerido";
                return false;
            }
            if (string.IsNullOrEmpty(pass))
            {
                error = "La contraseña es requerida";
                return false;
            }

            if (user == "admin" && pass == "admin")
            {
                return true;
            }

            error = "Usuario o contraseña incorrectos. ";
            return false;

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;
            LoginButton.IsEnabled = false;

            var user = UserTextBox.Text?.Trim();
            var pass = PasswordBox.Password;

            if (TryAuthenticate(user,pass, out var error))
            {
                var dashboard = new Home_Window ();
                dashboard.Show();
                this.Close();

            }
            else
            {
                ErrorText.Text = error;
                ErrorText.Visibility = Visibility.Visible;
                LoginButton.IsEnabled = true;
            }
        }

    }



    
}