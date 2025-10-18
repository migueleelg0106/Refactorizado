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

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para ConfirmacionSalirPartida.xaml
    /// </summary>
    public partial class ConfirmacionSalirPartida : Window
    {
        public ConfirmacionSalirPartida()
        {
            InitializeComponent();
        }

        private void BotonAceptarSalirPartida(object sender, RoutedEventArgs e)
        {
            VentanaPrincipal ventana = new VentanaPrincipal();
            ventana.Show();
            (this.Owner as Window)?.Close();            
            (this.Owner?.Owner as Window)?.Close();    
            this.Close();
        }

        private void BotonCancelarSalirPartida(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
