using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
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
        private const string NombreEndpoint = "NetTcpBinding_IAmigosManejador";

        private readonly SemaphoreSlim _semaforo = new(1, 1);
        private readonly object _solicitudesBloqueo = new();
        private readonly List<DTOs.SolicitudAmistadDTO> _solicitudes = new();

        private PictionaryServidorServicioAmigos.AmigosManejadorClient _cliente;
        private string _usuarioSuscrito;
        private bool _recursosLiberados;

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
                lock (_solicitudesBloqueo)
                {
                    return _solicitudes.Count == 0
                        ? Array.Empty<DTOs.SolicitudAmistadDTO>()
                        : _solicitudes.ToArray();
                }
            }
        }

        /// <summary>
        /// Conecta al usuario al servicio de notificaciones de amistad.
        /// </summary>
        public async Task SuscribirAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException(
                    "El nombre de usuario es obligatorio.",
                    nameof(nombreUsuario));
            }

            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                if (string.Equals(
                    _usuarioSuscrito,
                    nombreUsuario,
                    StringComparison.OrdinalIgnoreCase) && _cliente != null)
                {
                    return;
                }

                await CancelarSuscripcionInternaAsync().ConfigureAwait(false);

                LimpiarSolicitudes();
                var cliente = CrearCliente();
                _usuarioSuscrito = nombreUsuario;

                try
                {
                    await cliente.SuscribirAsync(nombreUsuario).ConfigureAwait(false);
                    _cliente = cliente;
                    NotificarSolicitudesActualizadas();
                    _logger.InfoFormat("Suscripción a servicio de amigos exitosa para: {0}", 
                        nombreUsuario);
                }
                catch (Exception ex) when (EsExcepcionDeServicio(ex))
                {
                    _logger.ErrorFormat("Fallo al suscribir a servicio de amigos para: {0}",
                        nombreUsuario, ex);
                    _usuarioSuscrito = null;
                    cliente.Abort();
                    ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
                }
            }
            finally
            {
                _semaforo.Release();
            }
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

            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_cliente == null || !string.Equals(
                    _usuarioSuscrito,
                    nombreUsuario,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                await CancelarSuscripcionInternaAsync().ConfigureAwait(false);
                _logger.InfoFormat("Suscripción a servicio de amigos cancelada para: {0}", 
                    nombreUsuario);
            }
            finally
            {
                _semaforo.Release();
            }
        }

        /// <summary>
        /// Envia una nueva peticion de amistad al servidor.
        /// </summary>
        public Task EnviarSolicitudAsync(
            string nombreUsuarioEmisor,
            string nombreUsuarioReceptor) => EjecutarOperacionAsync(
                c => c.EnviarSolicitudAmistadAsync(nombreUsuarioEmisor, nombreUsuarioReceptor));

        /// <summary>
        /// Responde a una peticion existente (aceptar/rechazar).
        /// </summary>
        public Task ResponderSolicitudAsync(
            string nombreUsuarioEmisor,
            string nombreUsuarioReceptor) => EjecutarOperacionAsync(
                c => c.ResponderSolicitudAmistadAsync(nombreUsuarioEmisor, nombreUsuarioReceptor));

        /// <summary>
        /// Elimina a un amigo de la lista de contactos.
        /// </summary>
        public Task EliminarAmigoAsync(
            string nombreUsuarioA,
            string nombreUsuarioB) => EjecutarOperacionAsync(
                c => c.EliminarAmigoAsync(nombreUsuarioA, nombreUsuarioB));

        /// <summary>
        /// Callback del servidor: Notifica que una solicitud cambio de estado.
        /// </summary>
        public void NotificarSolicitudActualizada(DTOs.SolicitudAmistadDTO solicitud)
        {
            if (solicitud == null ||
                string.IsNullOrWhiteSpace(solicitud.UsuarioEmisor) ||
                string.IsNullOrWhiteSpace(solicitud.UsuarioReceptor))
            {
                return;
            }

            _logger.InfoFormat("Callback recibido: Solicitud actualizada entre {0} y {1}.",
                solicitud.UsuarioEmisor,
                solicitud.UsuarioReceptor);

            string usuarioActual = _usuarioSuscrito;

            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return;
            }

            bool modificada = ActualizarSolicitudInterna(solicitud, usuarioActual);

            if (modificada)
            {
                NotificarSolicitudesActualizadas();
            }
        }

        /// <summary>
        /// Callback del servidor: Notifica que una amistad ha sido eliminada.
        /// </summary>
        public void NotificarAmistadEliminada(DTOs.SolicitudAmistadDTO solicitud)
        {
            if (solicitud == null)
            {
                return;
            }

            _logger.InfoFormat("Callback recibido: Amistad eliminada entre {0} y {1}.", 
                solicitud.UsuarioEmisor, solicitud.UsuarioReceptor);

            string usuarioActual = _usuarioSuscrito;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return;
            }

            bool modificada = false;

            lock (_solicitudesBloqueo)
            {
                int indice = _solicitudes.FindIndex(s =>
                    s.UsuarioEmisor == solicitud.UsuarioEmisor &&
                    s.UsuarioReceptor == usuarioActual);

                if (indice >= 0)
                {
                    _solicitudes.RemoveAt(indice);
                    modificada = true;
                }
            }

            if (modificada)
            {
                NotificarSolicitudesActualizadas();
            }
        }

        /// <summary>
        /// Libera los recursos del cliente WCF y semaforos.
        /// </summary>
        protected virtual void Dispose(bool liberando)
        {
            if (!_recursosLiberados)
            {
                if (liberando)
                {
                    try
                    {
                        CerrarCliente(_cliente);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("Error al cerrar cliente de amigos durante Dispose.", ex);
                    }

                    _cliente = null;
                    _usuarioSuscrito = null;
                    LimpiarSolicitudes();

                    _semaforo?.Dispose();
                }

                _recursosLiberados = true;
            }
        }

        /// <summary>
        /// Implementacion de IDisposable.
        /// </summary>
        public void Dispose()
        {
            Dispose(liberando: true);
            GC.SuppressFinalize(this);
        }

        private async Task EjecutarOperacionAsync(
            Func<PictionaryServidorServicioAmigos.AmigosManejadorClient, Task> operacion)
        {
            if (operacion == null)
            {
                throw new ArgumentNullException(nameof(operacion));
            }

            PictionaryServidorServicioAmigos.AmigosManejadorClient cliente = null;
            bool esTemporal = false;

            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                cliente = _cliente ?? CrearCliente();
                esTemporal = (_cliente == null);

                try
                {
                    await operacion(cliente).ConfigureAwait(false);
                    if (esTemporal)
                    {
                        CerrarCliente(cliente);
                    }
                }
                catch (Exception ex) when (EsExcepcionDeServicio(ex))
                {
                    _logger.Error("Error al ejecutar operación en servicio de amigos.", ex);
                    await ManejarExcepcionOperacionAsync(
                        ex,
                        cliente,
                        esTemporal).ConfigureAwait(false);
                }
            }
            finally
            {
                _semaforo.Release();
            }
        }

        private async Task ReiniciarClienteConSuscripcionAsync()
        {
            string usuario = _usuarioSuscrito;
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return;
            }

            await CancelarSuscripcionInternaAsync().ConfigureAwait(false);

            try
            {
                await SuscribirAsync(usuario).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error("Fallo al intentar reconectar suscripción de amigos.", ex);
            }
        }

        private bool ActualizarSolicitudInterna(
            DTOs.SolicitudAmistadDTO solicitud,
            string usuarioActual)
        {
            lock (_solicitudesBloqueo)
            {
                int indice = _solicitudes.FindIndex(s =>
                    s.UsuarioEmisor == solicitud.UsuarioEmisor &&
                    s.UsuarioReceptor == usuarioActual);

                if (solicitud.SolicitudAceptada)
                {
                    if (indice >= 0)
                    {
                        _solicitudes.RemoveAt(indice);
                        return true;
                    }
                }
                else if (string.Equals(
                    solicitud.UsuarioReceptor,
                    usuarioActual,
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (indice >= 0)
                    {
                        _solicitudes[indice] = solicitud;
                    }
                    else
                    {
                        _solicitudes.Add(solicitud);
                    }

                    return true;
                }
            }
            return false;
        }

        private async Task ManejarExcepcionOperacionAsync(
            Exception ex,
            ICommunicationObject cliente,
            bool esTemporal)
        {
            bool esErrorComunicacion = !(ex is FaultException);

            if (esTemporal)
            {
                cliente.Abort();
            }
            else if (esErrorComunicacion)
            {
                _logger.Warn("Detectado error de comunicación en canal permanente. " +
                    "Intentando reconexión.");
                await ReiniciarClienteConSuscripcionAsync().ConfigureAwait(false);
            }
            ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
        }

        private static bool EsExcepcionDeServicio(Exception ex)
        {
            return ex is FaultException ||
                   ex is EndpointNotFoundException ||
                   ex is TimeoutException ||
                   ex is CommunicationException ||
                   ex is InvalidOperationException ||
                   ex is OperationCanceledException;
        }

        private PictionaryServidorServicioAmigos.AmigosManejadorClient CrearCliente()
        {
            var contexto = new InstanceContext(this);
            return new PictionaryServidorServicioAmigos.AmigosManejadorClient(
                contexto,
                NombreEndpoint);
        }

        private async Task CancelarSuscripcionInternaAsync()
        {
            var cliente = _cliente;
            var usuario = _usuarioSuscrito;
            _cliente = null;
            _usuarioSuscrito = null;

            if (cliente == null)
            {
                LimpiarSolicitudes();
                NotificarSolicitudesActualizadas();
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(usuario) &&
                    cliente.State == CommunicationState.Opened)
                {
                    await cliente.CancelarSuscripcionAsync(usuario).ConfigureAwait(false);
                }
                CerrarCliente(cliente);
            }
            catch (Exception ex) when (EsExcepcionDeServicio(ex))
            {
                _logger.Warn("Excepción al cancelar suscripción interna de amigos.", ex);
                cliente.Abort();
            }
            finally
            {
                LimpiarSolicitudes();
                NotificarSolicitudesActualizadas();
            }
        }

        private static void CerrarCliente(
            PictionaryServidorServicioAmigos.AmigosManejadorClient cliente)
        {
            if (cliente == null)
            {
                return;
            }

            try
            {
                if (cliente.State != CommunicationState.Faulted)
                {
                    cliente.Close();
                }
                else
                {
                    cliente.Abort();
                }
            }
            catch (CommunicationException ex)
            {
                _logger.Warn("Excepción de comunicación al cerrar cliente de amigos.", ex);
                cliente.Abort();
            }
            catch (TimeoutException ex)
            {
                _logger.Warn("Timeout al cerrar cliente de amigos.", ex);
                cliente.Abort();
            }
        }

        private static void ManejarExcepcionServicio(Exception ex, string mensajePredeterminado)
        {
            switch (ex)
            {
                case FaultException faultEx:
                    throw new ServicioExcepcion(
                        TipoErrorServicio.FallaServicio,
                        ErrorServicioAyudante.ObtenerMensaje(faultEx, mensajePredeterminado),
                        ex);
                case EndpointNotFoundException _:
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Comunicacion,
                        Lang.errorTextoServidorNoDisponible,
                        ex);
                case TimeoutException _:
                    throw new ServicioExcepcion(
                        TipoErrorServicio.TiempoAgotado,
                        Lang.errorTextoServidorTiempoAgotado,
                        ex);
                case CommunicationException _:
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Comunicacion,
                        Lang.errorTextoServidorNoDisponible,
                        ex);
                case InvalidOperationException _:
                    throw new ServicioExcepcion(
                        TipoErrorServicio.OperacionInvalida,
                        Lang.errorTextoErrorProcesarSolicitud,
                        ex);
                default:
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Desconocido,
                        mensajePredeterminado,
                        ex);
            }
        }

        private void LimpiarSolicitudes()
        {
            lock (_solicitudesBloqueo)
            {
                _solicitudes.Clear();
            }
        }

        private void NotificarSolicitudesActualizadas()
        {
            IReadOnlyCollection<DTOs.SolicitudAmistadDTO> instantaneo;
            lock (_solicitudesBloqueo)
            {
                instantaneo = _solicitudes.ToArray();
            }
            SolicitudesActualizadas?.Invoke(this, instantaneo);
        }
    }
}