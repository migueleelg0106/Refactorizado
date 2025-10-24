using System.Windows;

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
