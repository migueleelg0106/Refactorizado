using System;
using System.Windows;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.VentanaPrincipal;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana que muestra la tabla de puntuaciones globales y estadisticas de jugadores.
    /// </summary>
    public partial class Clasificacion : Window
    {
        private readonly IClasificacionServicio _clasificacionServicio;
        private readonly IAvisoServicio _avisoServicio;
        private readonly ISonidoManejador _sonidoManejador;

        /// <summary>
        /// Constructor por defecto, solo para uso del diseñador/XAML. 
        /// La aplicación debe usar el constructor que recibe dependencias.
        /// </summary>
        public Clasificacion()
        {
        }

        /// <summary>
        /// Inicializa la ventana de clasificacion inyectando el servicio requerido.
        /// </summary>
        /// <param name="clasificacionServicio">Servicio para obtener los datos del ranking.
        /// </param>
        public Clasificacion(IClasificacionServicio clasificacionServicio,
            IAvisoServicio avisoServicio, ISonidoManejador sonidoManejador)
        {
            _clasificacionServicio = clasificacionServicio ??
                throw new ArgumentNullException(nameof(clasificacionServicio));
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));

            InitializeComponent();

            var vistaModelo = new ClasificacionVistaModelo(_clasificacionServicio,
                _avisoServicio, _sonidoManejador)
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