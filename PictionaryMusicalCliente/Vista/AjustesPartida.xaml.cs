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
    /// Lógica de interacción para AjustesPartida.xaml
    /// </summary>
    public partial class AjustesPartida : Window
    {
        public AjustesPartida()
        {
            InitializeComponent();
        }

        private void BotonConfirmar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void BotonSalirPartida(object sender, RoutedEventArgs e)
        {
            ConfirmacionSalirPartida confirmacionSalirPartida = new ConfirmacionSalirPartida();
            confirmacionSalirPartida.Owner = this;
            confirmacionSalirPartida.ShowDialog();
        }
    }
}
