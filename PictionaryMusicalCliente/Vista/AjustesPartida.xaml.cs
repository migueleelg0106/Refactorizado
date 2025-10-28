using System.Windows;

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
