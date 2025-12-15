using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Administrador;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Gestiona la comunicacion Duplex para solicitudes de amistad en tiempo real.
    /// </summary>
    public class AmigosServicio : IAmigosServicio,
        PictionaryServidorServicioAmigos.IAmigosManejadorCallback
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AmigosServicio));
        private readonly SemaphoreSlim _semaforo = new(1, 1);
        private readonly IWcfClienteFabrica _fabricaClientes; 
        private readonly ISolicitudesAmistadAdministrador _administradorSolicitudes;
        private readonly IManejadorErrorServicio _manejadorError;

        private PictionaryServidorServicioAmigos.AmigosManejadorClient _cliente;
        private string _usuarioSuscrito;
        private bool _recursosLiberados;

        public AmigosServicio(
            ISolicitudesAmistadAdministrador administradorSolicitudes,
            IManejadorErrorServicio manejadorError, 
            IWcfClienteFabrica fabricaClientes)
        {
            _administradorSolicitudes = administradorSolicitudes ??
                throw new ArgumentNullException(nameof(administradorSolicitudes));
            _manejadorError = manejadorError ??
                throw new ArgumentNullException(nameof(manejadorError));
            _fabricaClientes = fabricaClientes ??
                throw new ArgumentNullException(nameof(fabricaClientes));
        }

        /// <summary>
        /// Evento disparado al recibir cambios desde el servidor.
        /// </summary>
        public event EventHandler<IReadOnlyCollection<DTOs.SolicitudAmistadDTO>>
            SolicitudesActualizadas;
        
        /// <summary>
        /// Obtiene una copia segura de las solicitudes actuales.
        /// </summary>
        public IReadOnlyCollection<DTOs.SolicitudAmistadDTO> SolicitudesPendientes
        {
            get
            {
                return _administradorSolicitudes.ObtenerSolicitudes();
            }
        }

        /// <summary>
        /// Conecta al usuario al servicio de notificaciones de amistad.
        /// </summary>
        public async Task SuscribirAsync(string nombreUsuario)
        {
            ValidarNombreUsuario(nombreUsuario);

            await EjecutarEnSeccionCriticaAsync(async () =>
            {
                if (EsSuscripcionActual(nombreUsuario))
                {
                    return;
                }
                await SuscribirNuevoClienteAsync(nombreUsuario);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Desconecta al usuario y libera el canal de comunicacion.
        /// </summary>
        public async Task CancelarSuscripcionAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            await EjecutarEnSeccionCriticaAsync(async () =>
            {
                if (CoincideUsuarioSuscrito(nombreUsuario))
                {
                    await CancelarSuscripcionInternaAsync();
                }
            }).ConfigureAwait(false);

        }

        /// <summary>
        /// Envia una nueva peticion de amistad al servidor.
        /// </summary>
        public Task EnviarSolicitudAsync(string emisor, string receptor) =>
            EjecutarOperacionAsync(c => c.EnviarSolicitudAmistadAsync(emisor, receptor));

        /// <summary>
        /// Responde a una peticion existente (aceptar/rechazar).
        /// </summary>
        public Task ResponderSolicitudAsync(string emisor, string receptor) =>
            EjecutarOperacionAsync(c => c.ResponderSolicitudAmistadAsync(emisor, receptor));

        /// <summary>
        /// Elimina a un amigo de la lista de contactos.
        /// </summary>
        public Task EliminarAmigoAsync(string usuarioA, string usuarioB) =>
            EjecutarOperacionAsync(c => c.EliminarAmigoAsync(usuarioA, usuarioB));

        /// <summary>
        /// Callback del servidor: Notifica que una solicitud cambio de estado.
        /// </summary>
        public void NotificarSolicitudActualizada(DTOs.SolicitudAmistadDTO solicitud)
        {
            ProcesarNotificacion(solicitud, (solicita, usuario) => 
                _administradorSolicitudes.ActualizarSolicitud(solicita, usuario));
        }

        /// <summary>
        /// Callback del servidor: Notifica que una amistad ha sido eliminada.
        /// </summary>
        public void NotificarAmistadEliminada(DTOs.SolicitudAmistadDTO solicitud)
        {
            ProcesarNotificacion(solicitud, (solicita, usuario) =>
                _administradorSolicitudes.EliminaAmistadParaUsuario(solicita, usuario));
        }

        /// <summary>
        /// Libera los recursos del cliente WCF y semaforos.
        /// </summary>
        protected virtual void Dispose(bool liberando)
        {
            if (_recursosLiberados) return;

            if (liberando)
            {
                CerrarClienteSeguro(_cliente);
                LimpiarEstadoLocal();
                _semaforo?.Dispose();
            }
            _recursosLiberados = true;
        }

        /// <summary>
        /// Implementacion de IDisposable.
        /// </summary>
        public void Dispose()
        {
            Dispose(liberando: true);
            GC.SuppressFinalize(this);
        }

        private void ProcesarNotificacion(
            DTOs.SolicitudAmistadDTO solicitud,
            Func<DTOs.SolicitudAmistadDTO, string, bool> accionActualizacion)
        {
            if (!EsSolicitudValida(solicitud)) return;

            string usuarioActual = _usuarioSuscrito;
            if (string.IsNullOrWhiteSpace(usuarioActual)) return;

            bool modificada = accionActualizacion(solicitud, usuarioActual);
            if (modificada)
            {
                NotificarSolicitudesActualizadas();
            }
        }

        private async Task EjecutarOperacionAsync(
            Func<PictionaryServidorServicioAmigos.AmigosManejadorClient, Task> operacion)
        {
            await _semaforo.WaitAsync().ConfigureAwait(false);
            try
            {
                var cliente = _cliente ?? CrearCliente();
                bool esTemporal = (_cliente == null);

                await EjecutarLogicaClienteAsync(cliente, operacion, esTemporal);
            }
            finally
            {
                _semaforo.Release();
            }
        }

        private async Task EjecutarLogicaClienteAsync(
            PictionaryServidorServicioAmigos.AmigosManejadorClient cliente,
            Func<PictionaryServidorServicioAmigos.AmigosManejadorClient, Task> operacion,
            bool esTemporal)
        {
            try
            {
                await operacion(cliente).ConfigureAwait(false);
                if (esTemporal) CerrarClienteSeguro(cliente);
            }
            catch (Exception excepcion)
            {
                await ManejarErrorOperacionAsync(excepcion, cliente, esTemporal);
            }
        }

        private async Task ManejarErrorOperacionAsync(
            Exception excepcion,
            ICommunicationObject cliente,
            bool esTemporal)
        {
            if (esTemporal)
            {
                cliente.Abort();
            }
            else if (EsErrorComunicacion(excepcion))
            {
                _logger.Warn("Error comunicacion permanente. Intentando reconexion.");
                await IntentarReconexionAsync();
            }

            LanzarExcepcionServicio(excepcion, Lang.errorTextoErrorProcesarSolicitud);
        }

        private async Task IntentarReconexionAsync()
        {
            string usuario = _usuarioSuscrito;
            if (string.IsNullOrWhiteSpace(usuario)) return;

            await CancelarSuscripcionInternaAsync();

            try
            {
                await SuscribirNuevoClienteAsync(usuario);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Fallo critico al reconectar suscripcion.", excepcion);
            }
        }

        private async Task SuscribirNuevoClienteAsync(string nombreUsuario)
        {
            await CancelarSuscripcionInternaAsync();
            LimpiarEstadoLocal();

            var cliente = CrearCliente();
            _cliente = cliente;
            _usuarioSuscrito = nombreUsuario;

            try
            {
                await cliente.SuscribirAsync(nombreUsuario).ConfigureAwait(false);
                NotificarSolicitudesActualizadas();
            }
            catch (Exception excepcion)
            {
                cliente.Abort();
                LimpiarEstadoLocal();
                LanzarExcepcionServicio(excepcion, Lang.errorTextoErrorProcesarSolicitud);
            }
        }

        private async Task CancelarSuscripcionInternaAsync()
        {
            var cliente = _cliente;
            var usuario = _usuarioSuscrito;

            LimpiarEstadoLocal();

            if (cliente != null)
            {
                await IntentarCancelarEnServidorAsync(cliente, usuario);
                CerrarClienteSeguro(cliente);
            }
        }

        private static async Task IntentarCancelarEnServidorAsync(
            PictionaryServidorServicioAmigos.AmigosManejadorClient cliente,
            string usuario)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(usuario) &&
                    cliente.State == CommunicationState.Opened)
                {
                    await cliente.CancelarSuscripcionAsync(usuario).ConfigureAwait(false);
                }
            }
            catch (Exception excepcion)
            {
                _logger.Warn("No se pudo cancelar suscripcion en servidor.", excepcion);
            }
        }

        private void LanzarExcepcionServicio(Exception excepcion, string mensajeDefault)
        {
            if (excepcion is FaultException fault)
            {
                string mensaje = _manejadorError.ObtenerMensaje(fault, mensajeDefault);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, excepcion);
            }

            if (excepcion is TimeoutException)
            {
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    excepcion);
            }

            if (EsErrorComunicacion(excepcion))
            {
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    excepcion);
            }

            throw new ServicioExcepcion(TipoErrorServicio.Desconocido, mensajeDefault, excepcion);
        }

        private static bool EsErrorComunicacion(Exception excepcion)
        {
            return excepcion is CommunicationException || excepcion is EndpointNotFoundException;
        }

        private void LimpiarEstadoLocal()
        {
            _cliente = null;
            _usuarioSuscrito = null;
            _administradorSolicitudes.LimpiarSolicitudes();
            NotificarSolicitudesActualizadas();
        }

        private static void CerrarClienteSeguro(ICommunicationObject cliente)
        {
            if (cliente == null) return;
            try
            {
                if (cliente.State == CommunicationState.Opened) cliente.Close();
                else cliente.Abort();
            }
            catch (Exception)
            {
                cliente.Abort();
            }
        }

        private PictionaryServidorServicioAmigos.AmigosManejadorClient CrearCliente()
        {
            var contexto = new InstanceContext(this);
            return (PictionaryServidorServicioAmigos.AmigosManejadorClient)
                   _fabricaClientes.CrearClienteAmigos(contexto);
        }

        private async Task EjecutarEnSeccionCriticaAsync(Func<Task> accion)
        {
            await _semaforo.WaitAsync().ConfigureAwait(false);
            try
            {
                await accion().ConfigureAwait(false);
            }
            finally
            {
                _semaforo.Release();
            }
        }

        private void NotificarSolicitudesActualizadas()
        {
            SolicitudesActualizadas?.Invoke(this, _administradorSolicitudes.ObtenerSolicitudes());
        }

        private bool EsSuscripcionActual(string usuario)
        {
            return string.Equals(_usuarioSuscrito, usuario, StringComparison.OrdinalIgnoreCase)
                   && _cliente != null;
        }

        private bool CoincideUsuarioSuscrito(string usuario)
        {
            return string.Equals(_usuarioSuscrito, usuario, StringComparison.OrdinalIgnoreCase);
        }

        private static void ValidarNombreUsuario(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("Usuario obligatorio.", nameof(nombre));
        }

        private static bool EsSolicitudValida(DTOs.SolicitudAmistadDTO s)
        {
            return s != null &&
                   !string.IsNullOrWhiteSpace(s.UsuarioEmisor) &&
                   !string.IsNullOrWhiteSpace(s.UsuarioReceptor);
        }
    }
}