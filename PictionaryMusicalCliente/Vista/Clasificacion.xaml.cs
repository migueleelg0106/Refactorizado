using System.Windows;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.VistaModelo.VentanaPrincipal;

namespace PictionaryMusicalCliente
{
    public partial class Clasificacion : Window
    {
        public Clasificacion()
        {
            InitializeComponent();

            IClasificacionServicio clasificacionServicio = new ClasificacionServicio();

            var vistaModelo = new ClasificacionVistaModelo(clasificacionServicio)
            {
                CerrarAccion = Close
            };

            DataContext = vistaModelo;
        }

        private async void Clasificacion_LoadedAsync(object sender, RoutedEventArgs e)
        {
            if (DataContext is ClasificacionVistaModelo vistaModelo)
            {
                await vistaModelo.CargarClasificacionAsync().ConfigureAwait(true);
            }
        }
    }
}
