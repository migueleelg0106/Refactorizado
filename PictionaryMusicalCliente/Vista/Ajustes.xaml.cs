using System.Windows;
using PictionaryMusicalCliente.ClienteServicios;
using System.Windows.Controls;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para Ajustes.xaml
    /// </summary>
    public partial class Ajustes : Window
    {
        private readonly MusicaManejador _servicioMusica;

        public Ajustes(MusicaManejador servicioMusica)
        {
            InitializeComponent();
            _servicioMusica = servicioMusica;

            deslizadorVolumen.Value = _servicioMusica.Volume;
        }

        private void BotonConfirmar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BotonCerrarSesion(object sender, RoutedEventArgs e)
        {
            TerminacionSesion cerrarSesion = new TerminacionSesion();
            cerrarSesion.Owner = this;
            cerrarSesion.ShowDialog();
        }

        private void DeslizadorVolumen_CambioValor(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_servicioMusica != null)
            {
                _servicioMusica.Volume = e.NewValue;
            }
        }
    }
}
