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
    /// Lógica de interacción para ExpulsarJugador.xaml
    /// </summary>
    public partial class ExpulsarJugador : Window
    {
        public ExpulsarJugador()
        {
            InitializeComponent();
        }

        private void BotonExpulsarJugador(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BotonCancelarExpulsion(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
