using log4net;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using System;
using System.ServiceModel;
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
        private readonly ISonidoManejador _sonidoManejador;
        private readonly IAvisoServicio _avisoServicio;
        private readonly ILocalizadorServicio _localizadorServicio;
        private readonly IUsuarioAutenticado _usuarioSesion;
        private string _nombreUsuarioBusqueda;
        private bool _estaProcesando;

        /// <summary>
        /// Inicializa el ViewModel con el servicio de amigos.
        /// </summary>
        /// <param name="amigosServicio">Servicio para operaciones de red.</param>
        public BusquedaAmigoVistaModelo(IAmigosServicio amigosServicio,
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
            _localizadorServicio = localizadorServicio ??
                throw new ArgumentNullException(nameof(localizadorServicio));
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
                Cancelado?.Invoke();
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

        /// <summary>
        /// Evento disparado cuando la solicitud se envia con exito.
        /// </summary>
        public Action SolicitudEnviada { get; set; }

        /// <summary>
        /// Evento disparado al cancelar.
        /// </summary>
        public Action Cancelado { get; set; }

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
                _logger.Warn("Intento de enviar solicitud sin usuario actual en sesión.");
                _avisoServicio.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            EstaProcesando = true;

            try
            {
                _logger.InfoFormat("Enviando solicitud de amistad de {0} a {1}",
                    usuarioActual, nombreAmigo);
                await _amigosServicio.EnviarSolicitudAsync(
                    usuarioActual,
                    nombreAmigo).ConfigureAwait(true);

                _sonidoManejador.ReproducirExito();
                _avisoServicio.Mostrar(Lang.amigosTextoSolicitudEnviada);
                SolicitudEnviada?.Invoke();
            }
            catch (FaultException ex)
            {
                _logger.ErrorFormat("Error WCF (Fault) al enviar solicitud a {0}.",
                    nombreAmigo, ex);
                _sonidoManejador.ReproducirError();

                string mensajeError = _localizadorServicio.Localizar(
                    ex.Message,
                    Lang.errorTextoErrorProcesarSolicitud);
                _avisoServicio.Mostrar(mensajeError);
            }
            catch (ServicioExcepcion ex)
            {
                _logger.ErrorFormat("Error de servicio al enviar solicitud a {0}.",
                    nombreAmigo, ex);
                _sonidoManejador.ReproducirError(); 
                _avisoServicio.Mostrar(ex.Message);
            }
            finally
            {
                EstaProcesando = false;
            }
        }
    }
}