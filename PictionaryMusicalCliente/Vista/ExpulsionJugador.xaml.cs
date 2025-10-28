using System.Windows;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para ExpulsarJugador.xaml
    /// </summary>
    public partial class ExpulsionJugador : Window
    {
        public ExpulsionJugador()
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
