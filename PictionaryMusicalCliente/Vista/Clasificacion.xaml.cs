using System.Windows;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.VistaModelo.VentanaPrincipal;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Ventana que muestra la tabla de puntuaciones globales y estadisticas de jugadores.
    /// </summary>
    public partial class Clasificacion : Window
    {
        /// <summary>
        /// Inicializa la ventana de clasificacion y su contexto de datos.
        /// </summary>
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