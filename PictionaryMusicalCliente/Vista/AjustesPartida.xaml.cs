using PictionaryMusicalCliente.Utilidades;
using System.Windows;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para AjustesPartida.xaml
    /// </summary>
    public partial class AjustesPartida : Window
    {
        private readonly CancionManejador _servicioCancion;
        public AjustesPartida(CancionManejador servicioCancion)
        {
            InitializeComponent();
            _servicioCancion = servicioCancion;

            deslizadorVolumen.Value = _servicioCancion.Volumen;
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

        private void DeslizadorVolumen_CambioValor(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_servicioCancion != null)
            {
                _servicioCancion.Volumen = e.NewValue;
            }
        }
    }
}
