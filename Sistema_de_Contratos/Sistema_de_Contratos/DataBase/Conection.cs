using Microsoft.Data.SqlClient;
using System;
using System.Configuration;
using System.Windows;


namespace Sistema_de_Contratos.DataBase
{
    public class Conection
    {
        private readonly string connectionString;

        public Conection()
        {
            connectionString = ConfigurationManager.ConnectionStrings["ConexionSQL"].ConnectionString;
        }

        public SqlConnection AbrirConexion()
        {
            SqlConnection conexion = new SqlConnection(connectionString);
            conexion.Open();
            return conexion;
        }

        public void CerrarConexion(SqlConnection conexion)
        {
            if (conexion != null && conexion.State == System.Data.ConnectionState.Open)
            {
                conexion.Close();
            }
        }

        public bool ProbarConexion()
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(connectionString))
                {
                    conexion.Open();
                    MessageBox.Show("Conexión exitosa con la base de datos.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con la base de datos:\n" + ex.Message);
                return false;
            }
        }

    }
}
