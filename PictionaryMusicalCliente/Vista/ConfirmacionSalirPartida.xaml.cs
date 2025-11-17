using System.Windows;
using PictionaryMusicalCliente.VistaModelo.VentanaJuego;

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

            if (Owner?.Owner is VentanaJuego ventanaJuego
                && ventanaJuego.DataContext is VentanaJuegoVistaModelo vistaModelo
                && vistaModelo.EsInvitado)
            {
                debeAbrirVentanaPrincipal = false;
                vistaModelo.NotificarCierreAplicacionCompleta();
                ventanaDestino = new InicioSesion();
            }

            if (debeAbrirVentanaPrincipal)
            {
                ventanaDestino = new VentanaPrincipal();
            }

            ventanaDestino?.Show();

            Owner?.Close();
            Owner?.Owner?.Close();
            Close();
        }

        private void BotonCancelarSalirPartida(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
