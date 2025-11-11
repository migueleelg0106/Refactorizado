using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DTOs = Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    public class SalasServicio : ISalasServicio, PictionaryServidorServicioSalas.ISalasManejadorCallback
    {
        private const string Endpoint = "NetTcpBinding_ISalasManejador";

        private readonly SemaphoreSlim _semaforo = new(1, 1);
        private PictionaryServidorServicioSalas.SalasManejadorClient _cliente;

        public event EventHandler<string> JugadorSeUnio;
        public event EventHandler<string> JugadorSalio;

        public async Task<DTOs.SalaDTO> CrearSalaAsync(string nombreCreador, DTOs.ConfiguracionPartidaDTO configuracion)
        {
            if (string.IsNullOrWhiteSpace(nombreCreador))
                throw new ArgumentException("El nombre de creador es obligatorio.", nameof(nombreCreador));

            if (configuracion == null)
                throw new ArgumentNullException(nameof(configuracion));

            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                var cliente = ObtenerOCrearCliente();

                try
                {
                    return await cliente.CrearSalaAsync(nombreCreador, configuracion).ConfigureAwait(false);
                }
                catch (FaultException ex)
                {
                    string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                    throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    CerrarCliente();
                    throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (TimeoutException ex)
                {
                    CerrarCliente();
                    throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
                }
                catch (CommunicationException ex)
                {
                    CerrarCliente();
                    throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
            }
            finally
            {
                _semaforo.Release();
            }
        }

        public async Task<DTOs.SalaDTO> UnirseSalaAsync(string codigoSala, string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
                throw new ArgumentException("El código de sala es obligatorio.", nameof(codigoSala));

            if (string.IsNullOrWhiteSpace(nombreUsuario))
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));

            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                var cliente = ObtenerOCrearCliente();

                try
                {
                    return await cliente.UnirseSalaAsync(codigoSala, nombreUsuario).ConfigureAwait(false);
                }
                catch (FaultException ex)
                {
                    string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                    throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    CerrarCliente();
                    throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (TimeoutException ex)
                {
                    CerrarCliente();
                    throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
                }
                catch (CommunicationException ex)
                {
                    CerrarCliente();
                    throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
            }
            finally
            {
                _semaforo.Release();
            }
        }

        public async Task AbandonarSalaAsync(string codigoSala, string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
                return;

            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return;

            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_cliente == null)
                    return;

                try
                {
                    await _cliente.AbandonarSalaAsync(codigoSala, nombreUsuario).ConfigureAwait(false);
                }
                catch (FaultException ex)
                {
                    string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                    throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    CerrarCliente();
                    throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (TimeoutException ex)
                {
                    CerrarCliente();
                    throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
                }
                catch (CommunicationException ex)
                {
                    CerrarCliente();
                    throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
            }
            finally
            {
                _semaforo.Release();
            }
        }

        public void JugadorSeUnio(string codigoSala, string nombreJugador)
        {
            JugadorSeUnio?.Invoke(this, nombreJugador);
        }

        public void JugadorSalio(string codigoSala, string nombreJugador)
        {
            JugadorSalio?.Invoke(this, nombreJugador);
        }

        public void Dispose()
        {
            _semaforo.Wait();

            try
            {
                CerrarCliente();
            }
            finally
            {
                _semaforo.Release();
            }
        }

        private PictionaryServidorServicioSalas.SalasManejadorClient ObtenerOCrearCliente()
        {
            if (_cliente == null || _cliente.State == CommunicationState.Faulted)
            {
                CerrarCliente();
                var contexto = new InstanceContext(this);
                _cliente = new PictionaryServidorServicioSalas.SalasManejadorClient(contexto, Endpoint);
            }

            return _cliente;
        }

        private void CerrarCliente()
        {
            if (_cliente == null)
                return;

            try
            {
                if (_cliente.State == CommunicationState.Faulted)
                {
                    _cliente.Abort();
                }
                else
                {
                    _cliente.Close();
                }
            }
            catch (CommunicationException)
            {
                _cliente.Abort();
            }
            catch (TimeoutException)
            {
                _cliente.Abort();
            }
            finally
            {
                _cliente = null;
            }
        }
    }
}
