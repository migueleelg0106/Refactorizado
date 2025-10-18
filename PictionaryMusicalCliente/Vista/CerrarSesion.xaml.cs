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
using PictionaryMusicalCliente.Sesiones;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para CerrarSesion.xaml
    /// </summary>
    public partial class CerrarSesion : Window
    {
        public CerrarSesion()
        {
            InitializeComponent();
        }

        private void BotonAceptar(object sender, RoutedEventArgs e)
        {
            InicioSesion inicio = new InicioSesion();
            SesionUsuarioActual.Instancia.Limpiar();
            Application.Current.MainWindow = inicio;
            inicio.Show();

            this.Close();

            Window ventanaActual = this;
            while(ventanaActual != null)
            {
                Window owner = ventanaActual.Owner;
                ventanaActual.Close();
                ventanaActual = owner;
            }
        }

        private void BotonCancelar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
