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
            Loaded += AlCargarClasificacionAsync;
        }

        private async void AlCargarClasificacionAsync(object remitente,
            RoutedEventArgs argumentosEvento)
        {
            if (DataContext is ClasificacionVistaModelo vistaModelo)
            {
                await vistaModelo.CargarClasificacionAsync().ConfigureAwait(true);
            }
        }
    }
}