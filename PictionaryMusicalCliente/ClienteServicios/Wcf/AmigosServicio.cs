using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios; 
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    public class AmigosServicio : IAmigosServicio, PictionaryServidorServicioAmigos.IAmigosManejadorCallback
    {
        private const string Endpoint = "NetTcpBinding_IAmigosManejador";

        private readonly SemaphoreSlim _semaforo = new(1, 1);
        private readonly object _solicitudesBloqueo = new();
        private readonly List<DTOs.SolicitudAmistadDTO> _solicitudes = new();

        private PictionaryServidorServicioAmigos.AmigosManejadorClient _cliente;
        private string _usuarioSuscrito;

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
                catch (FaultException ex)
                {
                    _usuarioSuscrito = null;
                    cliente.Abort();
                    ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
                }
                catch (EndpointNotFoundException ex)
                {
                    _usuarioSuscrito = null;
                    cliente.Abort();
                    ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
                }
                catch (TimeoutException ex)
                {
                    _usuarioSuscrito = null;
                    cliente.Abort();
                    ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
                }
                catch (CommunicationException ex)
                {
                    _usuarioSuscrito = null;
                    cliente.Abort();
                    ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
                }
                catch (InvalidOperationException ex)
                {
                    _usuarioSuscrito = null;
                    cliente.Abort();
                    ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
                }
                catch (OperationCanceledException ex)
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

        public Task EnviarSolicitudAsync(string emisor, string receptor) =>
            EjecutarOperacionAsync(c => c.EnviarSolicitudAmistadAsync(emisor, receptor));

        public Task ResponderSolicitudAsync(string emisor, string receptor) =>
            EjecutarOperacionAsync(c => c.ResponderSolicitudAmistadAsync(emisor, receptor));

        public Task EliminarAmigoAsync(string usuarioA, string usuarioB) =>
            EjecutarOperacionAsync(c => c.EliminarAmigoAsync(usuarioA, usuarioB));

        public void NotificarSolicitudActualizada(DTOs.SolicitudAmistadDTO solicitud)
        {
            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.UsuarioEmisor) || string.IsNullOrWhiteSpace(solicitud.UsuarioReceptor))
                return;

            string usuarioActual = _usuarioSuscrito;

            if (string.IsNullOrWhiteSpace(usuarioActual))
                return;

            bool modificada = false;

            lock (_solicitudesBloqueo)
            {
                int indice = _solicitudes.FindIndex(s =>
                    s.UsuarioEmisor == solicitud.UsuarioEmisor && s.UsuarioReceptor == usuarioActual);

                if (solicitud.SolicitudAceptada)
                {
                    if (indice >= 0)
                    {
                        _solicitudes.RemoveAt(indice);
                        modificada = true;
                    }
                }
                else if (string.Equals(solicitud.UsuarioReceptor, usuarioActual, StringComparison.OrdinalIgnoreCase))
                {
                    if (indice >= 0)
                        _solicitudes[indice] = solicitud;
                    else
                        _solicitudes.Add(solicitud);

                    modificada = true;
                }
            }

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

        public void Dispose()
        {
            _semaforo.Wait();
            try
            {
                CerrarCliente(_cliente);
                _cliente = null;
                _usuarioSuscrito = null;
                LimpiarSolicitudes();
            }
            finally
            {
                _semaforo.Release();
            }
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
                catch (FaultException ex)
                {
                    if (esTemporal) cliente.Abort(); else ReiniciarClienteConSuscripcion();
                    ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
                }
                catch (EndpointNotFoundException ex)
                {
                    if (esTemporal) cliente.Abort(); else ReiniciarClienteConSuscripcion();
                    ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
                }
                catch (TimeoutException ex)
                {
                    if (esTemporal) cliente.Abort(); else ReiniciarClienteConSuscripcion();
                    ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
                }
                catch (CommunicationException ex)
                {
                    if (esTemporal) cliente.Abort(); else ReiniciarClienteConSuscripcion();
                    ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
                }
                catch (InvalidOperationException ex)
                {
                    if (esTemporal) cliente.Abort(); else ReiniciarClienteConSuscripcion();
                    ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
                }
                catch (OperationCanceledException ex)
                {
                    if (esTemporal) cliente.Abort(); else ReiniciarClienteConSuscripcion();
                    ManejarExcepcionServicio(ex, Lang.errorTextoErrorProcesarSolicitud);
                }
            }
            finally
            {
                _semaforo.Release();
            }
        }

        private async void ReiniciarClienteConSuscripcion()
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


        private PictionaryServidorServicioAmigos.AmigosManejadorClient CrearCliente()
        {
            var contexto = new InstanceContext(this);
            return new PictionaryServidorServicioAmigos.AmigosManejadorClient(contexto, Endpoint);
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
            catch (FaultException) { cliente.Abort(); } 
            catch (EndpointNotFoundException) { cliente.Abort(); }
            catch (TimeoutException) { cliente.Abort(); }
            catch (CommunicationException) { cliente.Abort(); }
            catch (InvalidOperationException) { cliente.Abort(); }
            catch (OperationCanceledException) { cliente.Abort(); }
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

            bool abortado = false;
            try
            {
                if (cliente.State != CommunicationState.Faulted)
                {
                    cliente.Close();
                }
                else
                {
                    cliente.Abort();
                    abortado = true;
                }
            }
            catch (CommunicationException)
            {
                cliente.Abort();
                abortado = true;
            }
            catch (TimeoutException)
            {
                cliente.Abort();
                abortado = true;
            }
            catch (Exception) 
            {
                if (!abortado) cliente.Abort();
            }
        }

        private static void ManejarExcepcionServicio(Exception ex, string mensajePredeterminado)
        {
            switch (ex)
            {
                case FaultException faultEx:
                    throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, ErrorServicioAyudante.ObtenerMensaje(faultEx, mensajePredeterminado), ex);
                case EndpointNotFoundException _: 
                    throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                case TimeoutException _:
                    throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
                case CommunicationException _:
                    throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                case InvalidOperationException _:
                    throw new ExcepcionServicio(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
                default:
                    throw new ExcepcionServicio(TipoErrorServicio.Desconocido, mensajePredeterminado, ex);
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