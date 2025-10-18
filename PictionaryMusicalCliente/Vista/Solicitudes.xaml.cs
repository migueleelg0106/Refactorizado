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
    /// Lógica de interacción para Solicitudes.xaml
    /// </summary>
    public partial class Solicitudes : Window
    {
        public Solicitudes()
        {
            InitializeComponent();
        }

        private void BotonAceptar(object sender, RoutedEventArgs e)
        {

        }

        private void BotonCancelar(object sender, RoutedEventArgs e)
        {

        }

        private void BotonRegresar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
