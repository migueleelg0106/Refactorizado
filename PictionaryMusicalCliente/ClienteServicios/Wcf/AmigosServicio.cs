using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    public class AmigosServicio : IAmigosServicio, PictionaryServidorServicioAmigos.IAmigosManejadorCallback
    {
        private const string NombreEndpoint = "NetTcpBinding_IAmigosManejador";

        private readonly SemaphoreSlim _semaforo = new(1, 1);
        private readonly object _solicitudesBloqueo = new();
        private readonly List<DTOs.SolicitudAmistadDTO> _solicitudes = new();

        private PictionaryServidorServicioAmigos.AmigosManejadorClient _cliente;
        private string _usuarioSuscrito;

        // Variable para el patrón Dispose (Control de recursos liberados)
        private bool _recursosLiberados;

        public event EventHandler<IReadOnlyCollection<DTOs.SolicitudAmistadDTO>> SolicitudesActualizadas;

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

        public async Task SuscribirAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));

            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                if (string.Equals(_usuarioSuscrito, nombreUsuario, StringComparison.OrdinalIgnoreCase)
                    && _cliente != null)
                    return;

                await CancelarSuscripcionInternaAsync().ConfigureAwait(false);

                LimpiarSolicitudes();
                var cliente = CrearCliente();
                _usuarioSuscrito = nombreUsuario;

                try
                {
                    await cliente.SuscribirAsync(nombreUsuario).ConfigureAwait(false);
                    _cliente = cliente;
                    NotificarSolicitudesActualizadas();
                }
                catch (Exception ex) when (EsExcepcionDeServicio(ex))
                {
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

        public async Task CancelarSuscripcionAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return;

            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_cliente == null || !string.Equals(_usuarioSuscrito, nombreUsuario, StringComparison.OrdinalIgnoreCase))
                    return;

                await CancelarSuscripcionInternaAsync().ConfigureAwait(false);
            }
            finally
            {
                _semaforo.Release();
            }
        }

        public Task EnviarSolicitudAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor) =>
            EjecutarOperacionAsync(c => c.EnviarSolicitudAmistadAsync(nombreUsuarioEmisor, nombreUsuarioReceptor));

        public Task ResponderSolicitudAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor) =>
            EjecutarOperacionAsync(c => c.ResponderSolicitudAmistadAsync(nombreUsuarioEmisor, nombreUsuarioReceptor));

        public Task EliminarAmigoAsync(string nombreUsuarioA, string nombreUsuarioB) =>
            EjecutarOperacionAsync(c => c.EliminarAmigoAsync(nombreUsuarioA, nombreUsuarioB));

        public void NotificarSolicitudActualizada(DTOs.SolicitudAmistadDTO solicitud)
        {
            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.UsuarioEmisor) || string.IsNullOrWhiteSpace(solicitud.UsuarioReceptor))
                return;

            string usuarioActual = _usuarioSuscrito;

            if (string.IsNullOrWhiteSpace(usuarioActual))
                return;

            bool modificada = ActualizarSolicitudInterna(solicitud, usuarioActual);

            if (modificada)
                NotificarSolicitudesActualizadas();
        }

        public void NotificarAmistadEliminada(DTOs.SolicitudAmistadDTO solicitud)
        {
            if (solicitud == null)
                return;

            string usuarioActual = _usuarioSuscrito;
            if (string.IsNullOrWhiteSpace(usuarioActual))
                return;

            bool modificada = false;

            lock (_solicitudesBloqueo)
            {
                int indice = _solicitudes.FindIndex(s =>
                    s.UsuarioEmisor == solicitud.UsuarioEmisor && s.UsuarioReceptor == usuarioActual);

                if (indice >= 0)
                {
                    _solicitudes.RemoveAt(indice);
                    modificada = true;
                }
            }

            if (modificada)
                NotificarSolicitudesActualizadas();
        }

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
                    catch (Exception)
                    {
                        // Ignorar errores al cerrar durante el Dispose
                    }

                    _cliente = null;
                    _usuarioSuscrito = null;
                    LimpiarSolicitudes();

                    _semaforo?.Dispose();
                }

                _recursosLiberados = true;
            }
        }

        public void Dispose()
        {
            Dispose(liberando: true);
            GC.SuppressFinalize(this);
        }

        private async Task EjecutarOperacionAsync(Func<PictionaryServidorServicioAmigos.AmigosManejadorClient, Task> operacion)
        {
            if (operacion == null)
                throw new ArgumentNullException(nameof(operacion));

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
                    if (esTemporal) CerrarCliente(cliente);
                }
                catch (Exception ex) when (EsExcepcionDeServicio(ex))
                {
                    await ManejarExcepcionOperacionAsync(ex, cliente, esTemporal).ConfigureAwait(false);
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
            if (string.IsNullOrWhiteSpace(usuario)) return;

            await CancelarSuscripcionInternaAsync().ConfigureAwait(false);

            try
            {
                await SuscribirAsync(usuario).ConfigureAwait(false);
            }
            catch
            {
                //Registrar en bitácora
            }
        }

        private bool ActualizarSolicitudInterna(DTOs.SolicitudAmistadDTO solicitud, string usuarioActual)
        {
            lock (_solicitudesBloqueo)
            {
                int indice = _solicitudes.FindIndex(s =>
                    s.UsuarioEmisor == solicitud.UsuarioEmisor && s.UsuarioReceptor == usuarioActual);

                if (solicitud.SolicitudAceptada)
                {
                    if (indice >= 0)
                    {
                        _solicitudes.RemoveAt(indice);
                        return true;
                    }
                }
                else if (string.Equals(solicitud.UsuarioReceptor, usuarioActual, StringComparison.OrdinalIgnoreCase))
                {
                    if (indice >= 0)
                        _solicitudes[indice] = solicitud;
                    else
                        _solicitudes.Add(solicitud);

                    return true;
                }
            }
            return false;
        }

        private async Task ManejarExcepcionOperacionAsync(Exception ex, ICommunicationObject cliente, bool esTemporal)
        {
            if (esTemporal)
            {
                cliente.Abort();
            }
            else
            {
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
            return new PictionaryServidorServicioAmigos.AmigosManejadorClient(contexto, NombreEndpoint);
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
                if (!string.IsNullOrWhiteSpace(usuario) && cliente.State == CommunicationState.Opened)
                {
                    await cliente.CancelarSuscripcionAsync(usuario).ConfigureAwait(false);
                }
                CerrarCliente(cliente);
            }
            catch (Exception ex) when (EsExcepcionDeServicio(ex))
            {
                cliente.Abort();
            }
            finally
            {
                LimpiarSolicitudes();
                NotificarSolicitudesActualizadas();
            }
        }

        private static void CerrarCliente(PictionaryServidorServicioAmigos.AmigosManejadorClient cliente)
        {
            if (cliente == null)
                return;

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
            catch (CommunicationException)
            {
                cliente.Abort();
            }
            catch (TimeoutException)
            {
                cliente.Abort();
            }
        }

        private static void ManejarExcepcionServicio(Exception ex, string mensajePredeterminado)
        {
            switch (ex)
            {
                case FaultException faultEx:
                    throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, ErrorServicioAyudante.ObtenerMensaje(faultEx, mensajePredeterminado), ex);
                case EndpointNotFoundException _:
                    throw new ServicioExcepcion(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                case TimeoutException _:
                    throw new ServicioExcepcion(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
                case CommunicationException _:
                    throw new ServicioExcepcion(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                case InvalidOperationException _:
                    throw new ServicioExcepcion(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
                default:
                    throw new ServicioExcepcion(TipoErrorServicio.Desconocido, mensajePredeterminado, ex);
            }
        }

        private void LimpiarSolicitudes()
        {
            lock (_solicitudesBloqueo)
                _solicitudes.Clear();
        }

        private void NotificarSolicitudesActualizadas()
        {
            IReadOnlyCollection<DTOs.SolicitudAmistadDTO> snapshot;
            lock (_solicitudesBloqueo)
            {
                snapshot = _solicitudes.ToArray();
            }
            SolicitudesActualizadas?.Invoke(this, snapshot);
        }
    }
}