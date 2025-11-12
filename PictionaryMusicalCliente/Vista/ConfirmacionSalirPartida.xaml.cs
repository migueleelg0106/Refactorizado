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
            bool debeAbrirVentanaPrincipal = true;
            Window ventanaDestino = null;

            if (Owner?.Owner is VentanaJuego ventanaJuego && ventanaJuego.EsInvitado)
            {
                debeAbrirVentanaPrincipal = false;
                ventanaJuego.PrepararCierrePorAplicacion();
            }

            if (debeAbrirVentanaPrincipal)
            {
                ventanaDestino = new VentanaPrincipal();
            }

            ventanaDestino?.Show();

            (Owner as Window)?.Close();
            (Owner?.Owner as Window)?.Close();
            Close();
        }

        private void BotonCancelarSalirPartida(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
