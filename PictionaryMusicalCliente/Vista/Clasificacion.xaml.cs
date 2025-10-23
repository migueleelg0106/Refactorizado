using System.Windows;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class Clasificacion : Window
    {
        public Clasificacion()
        {
            InitializeComponent();

            IClasificacionServicio clasificacionService = new ClasificacionServicio();

            var vistaModelo = new ClasificacionVistaModelo(clasificacionService)
            {
                CerrarAccion = Close
            };

            DataContext = vistaModelo;
        }

        private async void Clasificacion_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ClasificacionVistaModelo vistaModelo)
            {
                await vistaModelo.CargarClasificacionAsync().ConfigureAwait(true);
            }
        }
    }
}
