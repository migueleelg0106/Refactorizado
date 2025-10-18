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
    /// Lógica de interacción para Ajustes.xaml
    /// </summary>
    public partial class Ajustes : Window
    {
        public Ajustes()
        {
            InitializeComponent();
        }

        private void BotonConfirmar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BotonCerrarSesion(object sender, RoutedEventArgs e)
        {
            CerrarSesion cerrarSesion = new CerrarSesion();
            cerrarSesion.Owner = this;
            cerrarSesion.ShowDialog();
        }
    }
}
