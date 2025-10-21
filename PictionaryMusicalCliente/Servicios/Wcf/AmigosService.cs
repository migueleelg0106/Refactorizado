using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using AmigosSrv = PictionaryMusicalCliente.PictionaryServidorServicioAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class AmigosService : IAmigosService, AmigosSrv.IAmigosManejadorCallback
    {
        private const string Endpoint = "NetTcpBinding_IAmigosManejador";

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly object _solicitudesLock = new();
        private readonly List<SolicitudAmistad> _solicitudes = new();

        private AmigosSrv.AmigosManejadorClient _cliente;
        private string _usuarioSuscrito;

        public event EventHandler<IReadOnlyCollection<SolicitudAmistad>> SolicitudesActualizadas;

        public IReadOnlyCollection<SolicitudAmistad> SolicitudesPendientes
        {
            get
            {
                lock (_solicitudesLock)
                {
                    return _solicitudes.Count == 0
                        ? Array.Empty<SolicitudAmistad>()
                        : _solicitudes.ToArray();
                }
            }
        }

        public async Task SuscribirAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));
            }

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (string.Equals(_usuarioSuscrito, nombreUsuario, StringComparison.OrdinalIgnoreCase)
                    && _cliente != null)
                {
                    return;
                }

                await CancelarSuscripcionInternaAsync().ConfigureAwait(false);

                var cliente = CrearCliente();

                try
                {
                    await cliente.SuscribirAsync(nombreUsuario).ConfigureAwait(false);
                    _cliente = cliente;
                    _usuarioSuscrito = nombreUsuario;
                    LimpiarSolicitudes();
                    NotificarSolicitudesActualizadas();
                }
                catch (FaultException ex)
                {
                    cliente.Abort();
                    string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                    throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    cliente.Abort();
                    throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (TimeoutException ex)
                {
                    cliente.Abort();
                    throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
                }
                catch (CommunicationException ex)
                {
                    cliente.Abort();
                    throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (InvalidOperationException ex)
                {
                    cliente.Abort();
                    throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task CancelarSuscripcionAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_cliente == null)
                {
                    return;
                }

                if (!string.Equals(_usuarioSuscrito, nombreUsuario, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                await CancelarSuscripcionInternaAsync().ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Task EnviarSolicitudAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
        {
            return EjecutarOperacionAsync(cliente =>
                cliente.EnviarSolicitudAmistadAsync(nombreUsuarioEmisor, nombreUsuarioReceptor));
        }

        public Task ResponderSolicitudAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
        {
            return EjecutarOperacionAsync(cliente =>
                cliente.ResponderSolicitudAmistadAsync(nombreUsuarioEmisor, nombreUsuarioReceptor));
        }

        public Task EliminarAmigoAsync(string nombreUsuarioA, string nombreUsuarioB)
        {
            return EjecutarOperacionAsync(cliente =>
                cliente.EliminarAmigoAsync(nombreUsuarioA, nombreUsuarioB));
        }

        public void SolicitudActualizada(AmigosSrv.SolicitudAmistadDTO solicitud)
        {
            if (solicitud == null
                || string.IsNullOrWhiteSpace(solicitud.UsuarioEmisor)
                || string.IsNullOrWhiteSpace(solicitud.UsuarioReceptor))
            {
                return;
            }

            var nuevaSolicitud = new SolicitudAmistad(
                solicitud.UsuarioEmisor,
                solicitud.UsuarioReceptor,
                solicitud.SolicitudAceptada);

            bool modificada = false;

            lock (_solicitudesLock)
            {
                int indice = _solicitudes.FindIndex(s =>
                    s.CoincideCon(nuevaSolicitud.UsuarioEmisor, nuevaSolicitud.UsuarioReceptor));

                if (nuevaSolicitud.SolicitudAceptada)
                {
                    if (indice >= 0)
                    {
                        _solicitudes.RemoveAt(indice);
                        modificada = true;
                    }
                }
                else
                {
                    if (indice >= 0)
                    {
                        _solicitudes[indice] = nuevaSolicitud;
                    }
                    else
                    {
                        _solicitudes.Add(nuevaSolicitud);
                    }

                    modificada = true;
                }
            }

            if (modificada)
            {
                NotificarSolicitudesActualizadas();
            }
        }

        public void AmistadEliminada(AmigosSrv.SolicitudAmistadDTO solicitud)
        {
            if (solicitud == null
                || string.IsNullOrWhiteSpace(solicitud.UsuarioEmisor)
                || string.IsNullOrWhiteSpace(solicitud.UsuarioReceptor))
            {
                return;
            }

            bool modificada = false;

            lock (_solicitudesLock)
            {
                int indice = _solicitudes.FindIndex(s =>
                    s.CoincideCon(solicitud.UsuarioEmisor, solicitud.UsuarioReceptor));

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

        public void Dispose()
        {
            _semaphore.Wait();

            try
            {
                CerrarCliente(_cliente);
                _cliente = null;
                _usuarioSuscrito = null;
                LimpiarSolicitudes();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task EjecutarOperacionAsync(Func<AmigosSrv.AmigosManejadorClient, Task> operacion)
        {
            if (operacion == null)
            {
                throw new ArgumentNullException(nameof(operacion));
            }

            AmigosSrv.AmigosManejadorClient cliente = null;
            bool esTemporal = false;

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                cliente = _cliente;

                if (cliente == null)
                {
                    cliente = CrearCliente();
                    esTemporal = true;
                }

                try
                {
                    await operacion(cliente).ConfigureAwait(false);

                    if (esTemporal)
                    {
                        CerrarCliente(cliente);
                    }
                }
                catch (FaultException ex)
                {
                    if (esTemporal)
                    {
                        cliente.Abort();
                    }

                    string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                    throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    if (esTemporal)
                    {
                        cliente.Abort();
                    }

                    throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (TimeoutException ex)
                {
                    if (esTemporal)
                    {
                        cliente.Abort();
                    }

                    throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
                }
                catch (CommunicationException ex)
                {
                    if (esTemporal)
                    {
                        cliente.Abort();
                    }

                    throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (InvalidOperationException ex)
                {
                    if (esTemporal)
                    {
                        cliente.Abort();
                    }

                    throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private AmigosSrv.AmigosManejadorClient CrearCliente()
        {
            var contexto = new InstanceContext(this);
            return new AmigosSrv.AmigosManejadorClient(contexto, Endpoint);
        }

        private async Task CancelarSuscripcionInternaAsync()
        {
            AmigosSrv.AmigosManejadorClient cliente = _cliente;
            string usuario = _usuarioSuscrito;
            _cliente = null;
            _usuarioSuscrito = null;

            if (cliente == null)
            {
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(usuario))
                {
                    await cliente.CancelarSuscripcionAsync(usuario).ConfigureAwait(false);
                }

                CerrarCliente(cliente);
            }
            catch (FaultException ex)
            {
                cliente.Abort();
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                cliente.Abort();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                cliente.Abort();
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                cliente.Abort();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                cliente.Abort();
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }

            LimpiarSolicitudes();
            NotificarSolicitudesActualizadas();
        }

        private static void CerrarCliente(AmigosSrv.AmigosManejadorClient cliente)
        {
            if (cliente == null)
            {
                return;
            }

            try
            {
                if (cliente.State == CommunicationState.Faulted)
                {
                    cliente.Abort();
                }
                else
                {
                    cliente.Close();
                }
            }
            catch
            {
                cliente.Abort();
            }
        }

        private void LimpiarSolicitudes()
        {
            lock (_solicitudesLock)
            {
                _solicitudes.Clear();
            }
        }

        private void NotificarSolicitudesActualizadas()
        {
            IReadOnlyCollection<SolicitudAmistad> snapshot = SolicitudesPendientes;
            SolicitudesActualizadas?.Invoke(this, snapshot);
        }
    }
}
