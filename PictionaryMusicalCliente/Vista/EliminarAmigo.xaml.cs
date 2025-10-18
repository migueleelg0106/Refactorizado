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
    /// Lógica de interacción para EliminarAmigo.xaml
    /// </summary>
    public partial class EliminarAmigo : Window
    {
        public EliminarAmigo()
        {
            InitializeComponent();
        }

        private void BotonAceptar(object sender, RoutedEventArgs e)
        {

        }

        private void BotonCancelar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
