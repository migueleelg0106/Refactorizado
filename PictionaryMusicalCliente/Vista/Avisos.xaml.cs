using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
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
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public partial class Avisos : Window
    {
        public Avisos(string mensaje)
        {
            InitializeComponent();
            textoMensaje.Text = mensaje; 

        }

        private void botonAceptar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
