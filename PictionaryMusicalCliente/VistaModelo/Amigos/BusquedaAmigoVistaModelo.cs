using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    /// <summary>
    /// Controla la logica de busqueda y envio de solicitudes de amistad.
    /// </summary>
    public class BusquedaAmigoVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAmigosServicio _amigosServicio;
        private readonly SonidoManejador _sonidoManejador;
        private readonly IAvisoServicio _avisoServicio;
        private readonly IUsuarioAutenticado _usuarioSesion;
        private string _nombreUsuarioBusqueda;
        private bool _estaProcesando;

        /// <summary>
        /// Inicializa el ViewModel con el servicio de amigos.
        /// </summary>
        /// <param name="ventana">Servicio para gestionar ventanas.</param>
        /// <param name="localizador">Servicio de localizacion.</param>
        /// <param name="amigosServicio">Servicio para operaciones de red.</param>
        /// <param name="sonidoManejador">Servicio de sonido.</param>
        /// <param name="avisoServicio">Servicio de avisos.</param>
        /// <param name="usuarioSesion">Usuario en sesion.</param>
        public BusquedaAmigoVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            IAmigosServicio amigosServicio,
            SonidoManejador sonidoManejador,
            IAvisoServicio avisoServicio,
            IUsuarioAutenticado usuarioSesion)
            : base(ventana, localizador)
        {
            _amigosServicio = amigosServicio ??
                throw new ArgumentNullException(nameof(amigosServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _usuarioSesion = usuarioSesion ??
                throw new ArgumentNullException(nameof(usuarioSesion));

            EnviarSolicitudComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await EnviarSolicitudAsync();
            }, _ => PuedeEnviarSolicitud());

            CancelarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                _ventana.CerrarVentana(this);
            });
        }

        /// <summary>
        /// Nombre de usuario ingresado para buscar.
        /// </summary>
        public string NombreUsuarioBusqueda
        {
            get => _nombreUsuarioBusqueda;
            set
            {
                if (EstablecerPropiedad(ref _nombreUsuarioBusqueda, value))
                {
                    EnviarSolicitudComando?.NotificarPuedeEjecutar();
                }
            }
        }

        /// <summary>
        /// Indica si hay una operacion de red en curso.
        /// </summary>
        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    EnviarSolicitudComando?.NotificarPuedeEjecutar();
                }
            }
        }

        /// <summary>
        /// Comando para enviar la solicitud de amistad.
        /// </summary>
        public IComandoAsincrono EnviarSolicitudComando { get; }

        /// <summary>
        /// Comando para cancelar y cerrar la ventana.
        /// </summary>
        public ICommand CancelarComando { get; }

        private bool PuedeEnviarSolicitud()
        {
            return !EstaProcesando
                && !string.IsNullOrWhiteSpace(NombreUsuarioBusqueda);
        }

        private async Task EnviarSolicitudAsync()
        {
            string nombreAmigo = NombreUsuarioBusqueda?.Trim();
            string usuarioActual = _usuarioSesion.NombreUsuario;

            if (string.IsNullOrWhiteSpace(nombreAmigo))
            {
                _avisoServicio.Mostrar(Lang.buscarAmigoTextoIngreseUsuario);
                return;
            }

            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                _logger.Warn("Intento de enviar solicitud sin usuario actual en sesion.");
                _avisoServicio.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            EstaProcesando = true;

            await EjecutarOperacionAsync(async () =>
            {
                _logger.InfoFormat("Enviando solicitud de amistad de {0} a {1}",
                    usuarioActual, nombreAmigo);
                await _amigosServicio.EnviarSolicitudAsync(
                    usuarioActual,
                    nombreAmigo).ConfigureAwait(true);

                _sonidoManejador.ReproducirNotificacion();
                _avisoServicio.Mostrar(Lang.amigosTextoSolicitudEnviada);
                _ventana.CerrarVentana(this);
            },
            ex =>
            {
                _logger.ErrorFormat("Error al enviar solicitud a {0}.", nombreAmigo, ex);
                _sonidoManejador.ReproducirError();

                string mensajeError = ex.Message ?? Lang.errorTextoErrorProcesarSolicitud;
                _avisoServicio.Mostrar(mensajeError);
                
                EstaProcesando = false;
            });

            EstaProcesando = false;
        }
    }
}