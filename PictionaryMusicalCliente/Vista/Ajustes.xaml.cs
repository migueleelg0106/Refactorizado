using System.Windows;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.VistaModelo.Ajustes;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para la ventana de configuración general de la aplicación.
    /// </summary>
    public partial class Ajustes : Window
    {
        private readonly AjustesVistaModelo _vistaModelo;

        /// <summary>
        /// Inicializa una nueva instancia de la ventana de ajustes con las dependencias 
        /// necesarias.
        /// </summary>
        /// <param name="servicioMusica">El servicio encargado del control de audio global.</param>
        public Ajustes(MusicaManejador servicioMusica)
        {
            InitializeComponent();

            _vistaModelo = new AjustesVistaModelo(servicioMusica);

            _vistaModelo.OcultarVentana = () => Close();

            _vistaModelo.MostrarDialogoCerrarSesion = () =>
            {
                var cerrarSesion = new TerminacionSesion
                {
                    Owner = this
                };
                cerrarSesion.ShowDialog();
            };

            DataContext = _vistaModelo;
        }
    }
}