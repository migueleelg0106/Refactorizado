using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Ajustes;
using System;
using System.Windows;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Lógica de interacción para la ventana de configuración general de la aplicación.
    /// </summary>
    public partial class Ajustes : Window
    {
        private readonly AjustesVistaModelo _vistaModelo;
        private readonly IMusicaManejador _musicaManejador;
        private readonly ISonidoManejador _sonidoManejador;
        private readonly IUsuarioAutenticado _usuarioSesion;
        private readonly Action _accionCerrarSesion;

        /// <summary>
        /// Constructor por defecto, solo para uso del diseñador/XAML. 
        /// La aplicación debe usar el constructor que recibe dependencias.
        /// </summary>
        public Ajustes()
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la ventana de ajustes con las dependencias 
        /// necesarias.
        /// </summary>
        /// <param name="servicioMusica">El servicio encargado del control de audio global.</param>
        public Ajustes(IMusicaManejador servicioMusica,
            ISonidoManejador sonidoManejador,
            IUsuarioAutenticado usuarioSesion,
            Action accionCerrarSesion)
        {
            _musicaManejador = servicioMusica ?? 
                throw new ArgumentNullException(nameof(servicioMusica));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _usuarioSesion = usuarioSesion ??
                throw new ArgumentNullException(nameof(usuarioSesion));
            _accionCerrarSesion = accionCerrarSesion;

            InitializeComponent();

            _vistaModelo = new AjustesVistaModelo(_musicaManejador,
                _sonidoManejador);

            _vistaModelo.OcultarVentana = () => Close();

            _vistaModelo.MostrarDialogoCerrarSesion = () =>
            {
                var cerrarSesion = new TerminacionSesion(_usuarioSesion, _accionCerrarSesion)
                {
                    Owner = this
                };
                cerrarSesion.ShowDialog();
            };

            DataContext = _vistaModelo;
        }
    }
}