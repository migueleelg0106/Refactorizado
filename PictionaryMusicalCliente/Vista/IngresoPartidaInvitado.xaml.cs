using System.Windows;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para UnirsePartidaInvitado.xaml
    /// </summary>
    public partial class IngresoPartidaInvitado : Window
    {
        public IngresoPartidaInvitado()
        {
            InitializeComponent();
        }

        private void BotonUnirsePartida(object sender, RoutedEventArgs e)
        {

        }

        private void BotonCancelarUnirse(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
