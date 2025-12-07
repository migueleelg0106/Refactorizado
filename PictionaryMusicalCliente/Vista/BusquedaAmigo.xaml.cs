using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using System;
using System.Windows;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Logica de interaccion para la ventana de busqueda y solicitud de amistad.
    /// </summary>
    public partial class BusquedaAmigo : Window
    {
        private readonly BusquedaAmigoVistaModelo _vistaModelo;
        private readonly ISonidoManejador _sonidoManejador;
        private readonly IAmigosServicio _amigosServicio;
        private readonly IAvisoServicio _avisoServicio;
        private readonly ILocalizadorServicio _localizador;
        private readonly IUsuarioAutenticado _usuarioSesion;

        /// <summary>
        /// Constructor por defecto, solo para uso del diseñador/XAML. 
        /// La aplicación debe usar el constructor que recibe dependencias.
        /// </summary>
        public BusquedaAmigo()
        {
        }

        /// <summary>
        /// Inicializa la ventana inyectando el servicio requerido.
        /// </summary>
        /// <param name="amigosServicio">Servicio de gestion de amigos ya configurado.</param>
        public BusquedaAmigo(IAmigosServicio amigosServicio,
            ISonidoManejador sonidoManejador,
            IAvisoServicio avisoServicio,
            ILocalizadorServicio localizadorServicio,
            IUsuarioAutenticado usuarioSesion)
        {
            _amigosServicio = amigosServicio ??
                throw new ArgumentNullException(nameof(amigosServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _localizador = localizadorServicio ??
                throw new ArgumentNullException(nameof(localizadorServicio));
            _usuarioSesion = usuarioSesion ??
                throw new ArgumentNullException(nameof(usuarioSesion));

            InitializeComponent();

            _vistaModelo = new BusquedaAmigoVistaModelo(_amigosServicio, _sonidoManejador,
                _avisoServicio, _localizador, _usuarioSesion);
            DataContext = _vistaModelo;

            ConfigurarEventos();
        }

        private void ConfigurarEventos()
        {
            _vistaModelo.SolicitudEnviada += VistaModelo_SolicitudEnviada;
            _vistaModelo.Cancelado += VistaModelo_Cancelado;
            Closed += BuscarAmigo_Closed;
        }

        private void VistaModelo_SolicitudEnviada()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(VistaModelo_SolicitudEnviada);
                return;
            }
            Close();
        }

        private void VistaModelo_Cancelado()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(VistaModelo_Cancelado);
                return;
            }
            Close();
        }

        private void BuscarAmigo_Closed(object sender, EventArgs e)
        {
            Closed -= BuscarAmigo_Closed;
            if (_vistaModelo != null)
            {
                _vistaModelo.SolicitudEnviada -= VistaModelo_SolicitudEnviada;
                _vistaModelo.Cancelado -= VistaModelo_Cancelado;
            }
        }
    }
}