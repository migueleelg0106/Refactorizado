using System.Windows;
using PictionaryMusicalCliente.VistaModelo.VentanaPrincipal;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana que muestra la tabla de puntuaciones globales y estadisticas de jugadores.
    /// </summary>
    public partial class Clasificacion : Window
    {
        /// <summary>
        /// Constructor por defecto. VentanaServicio asigna el DataContext.
        /// </summary>
        public Clasificacion()
        {
            InitializeComponent();
            Loaded += Clasificacion_LoadedAsync;
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