using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ServiceModel;
using log4net;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    /// <summary>
    /// Controla la logica de busqueda y envio de solicitudes de amistad.
    /// </summary>
    public class BusquedaAmigoVistaModelo : BaseVistaModelo
    {
        private static readonly ILog Log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAmigosServicio _amigosServicio;
        private readonly string _usuarioActual;
        private string _nombreUsuarioBusqueda;
        private bool _estaProcesando;

        /// <summary>
        /// Inicializa el ViewModel con el servicio de amigos.
        /// </summary>
        /// <param name="amigosServicio">Servicio para operaciones de red.</param>
        public BusquedaAmigoVistaModelo(IAmigosServicio amigosServicio)
        {
            _amigosServicio = amigosServicio ??
                throw new ArgumentNullException(nameof(amigosServicio));
            _usuarioActual = SesionUsuarioActual.Usuario?.NombreUsuario ?? string.Empty;

            EnviarSolicitudComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await EnviarSolicitudAsync();
            }, _ => PuedeEnviarSolicitud());

            CancelarComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
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

            if (string.IsNullOrWhiteSpace(nombreAmigo))
            {
                AvisoAyudante.Mostrar(Lang.buscarAmigoTextoIngreseUsuario);
                return;
            }

            if (string.IsNullOrWhiteSpace(_usuarioActual))
            {
                Log.Warn("Intento de enviar solicitud sin usuario actual en sesión.");
                AvisoAyudante.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            EstaProcesando = true;

            try
            {
                Log.Info($"Enviando solicitud de amistad de {_usuarioActual} a {nombreAmigo}");
                await _amigosServicio.EnviarSolicitudAsync(
                    _usuarioActual,
                    nombreAmigo).ConfigureAwait(true);

                ManejadorSonido.ReproducirExito();
                AvisoAyudante.Mostrar(Lang.amigosTextoSolicitudEnviada);
                SolicitudEnviada?.Invoke();
            }
            catch (FaultException ex)
            {
                Log.Error($"Error WCF (Fault) al enviar solicitud a {nombreAmigo}.", ex);
                ManejadorSonido.ReproducirError();
                string mensajeServidor = ex.Message;

                string mensajeError = MensajeServidorAyudante.Localizar(
                    mensajeServidor,
                    Lang.errorTextoErrorProcesarSolicitud);
                AvisoAyudante.Mostrar(mensajeError);
            }
            catch (ServicioExcepcion ex)
            {
                Log.Error($"Error de servicio al enviar solicitud a {nombreAmigo}.", ex);
                ManejadorSonido.ReproducirError();
                string mensajeServidor = ex.Message;

                string mensajeError = MensajeServidorAyudante.Localizar(
                    mensajeServidor,
                    Lang.errorTextoErrorProcesarSolicitud);
                AvisoAyudante.Mostrar(mensajeError);
            }
            finally
            {
                EstaProcesando = false;
            }
        }
    }
}