using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using System;
using System.Windows;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana para gestionar las solicitudes de amistad pendientes.
    /// </summary>
    public partial class Solicitudes : Window
    {
        private readonly SolicitudesVistaModelo _vistaModelo;
        private readonly IUsuarioAutenticado _usuarioSesion;
        private readonly ISonidoManejador _sonidoManejador;
        private readonly IAvisoServicio _avisoServicio;

        /// <summary>
        /// Constructor por defecto, solo para uso del diseñador/XAML. 
        /// La aplicación debe usar el constructor que recibe dependencias.
        /// </summary>
        public Solicitudes()
        {
        }

        /// <summary>
        /// Inicializa la ventana inyectando el servicio de amigos.
        /// </summary>
        /// <param name="amigosServicio">Servicio de gestion de amigos ya instanciado.</param>
        public Solicitudes(IAmigosServicio amigosServicio,
            ISonidoManejador sonidos,
            IAvisoServicio aviso,
            IUsuarioAutenticado usuario)
        {
            _sonidoManejador = sonidos ??
                throw new ArgumentNullException(nameof(sonidos));
            _avisoServicio = aviso ??
                throw new ArgumentNullException(nameof(aviso));
            _usuarioSesion = usuario ??
                throw new ArgumentNullException(nameof(usuario));

            if (amigosServicio == null)
            {
                throw new ArgumentNullException(nameof(amigosServicio));
            }

            InitializeComponent();

            _vistaModelo = new SolicitudesVistaModelo(amigosServicio, _sonidoManejador,
                _avisoServicio, _usuarioSesion);
            DataContext = _vistaModelo;

            ConfigurarEventos();
        }

        private void ConfigurarEventos()
        {
            _vistaModelo.Cerrar += VistaModelo_Cerrar;
            Closed += Solicitudes_Closed;
        }

        private void VistaModelo_Cerrar()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(VistaModelo_Cerrar);
                return;
            }
            Close();
        }

        private void Solicitudes_Closed(object sender, EventArgs e)
        {
            Closed -= Solicitudes_Closed;
            if (_vistaModelo != null)
            {
                _vistaModelo.Cerrar -= VistaModelo_Cerrar;
                _vistaModelo.Dispose();
            }
        }
    }
}