using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using AmigosSrv = PictionaryMusicalCliente.PictionaryServidorServicioAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public sealed class AmigosService : IAmigosService, AmigosSrv.IAmigosManejadorCallback
    {
        private const string AmigosEndpoint = "NetTcpBinding_IAmigosManejador";
        private static readonly Lazy<AmigosService> _instancia = new(() => new AmigosService());

        private readonly object _clienteLock = new();
        private readonly object _sincronizacion = new();
        private readonly HashSet<string> _amigos = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<SolicitudAmistad> _solicitudesPendientes = new();

        private InstanceContext _contexto;
        private AmigosSrv.AmigosManejadorClient _cliente;
        private string _usuarioActual;

        private AmigosService()
        {
        }

        public static AmigosService Instancia => _instancia.Value;

        public event EventHandler<SolicitudAmistadEventArgs> SolicitudRecibida;

        public event EventHandler<RespuestaSolicitudAmistadEventArgs> SolicitudRespondida;

        public event EventHandler<AmistadEliminadaEventArgs> AmistadEliminada;

        public async Task SuscribirseAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException(nameof(nombreUsuario));
            }

            _usuarioActual = nombreUsuario.Trim();

            AmigosSrv.AmigosManejadorClient cliente = ObtenerCliente();

            try
            {
                await cliente.SuscribirseAsync(_usuarioActual).ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoAmigosSuscripcion);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public async Task DesuscribirseAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            AmigosSrv.AmigosManejadorClient cliente = ObtenerCliente();

            try
            {
                await cliente.DesuscribirseAsync(nombreUsuario.Trim()).ConfigureAwait(false);
            }
            catch (FaultException)
            {
                // El servicio no arroja FaultException explícitas en esta operación, por lo que se ignora.
            }
            catch (CommunicationException)
            {
                // Ignorado: en un escenario de cierre no es necesario propagar la excepción.
            }
            catch (TimeoutException)
            {
                // Ignorado por simetría con CommunicationException.
            }
            finally
            {
                CerrarCliente();
            }
        }

        public async Task<ResultadoOperacion> EnviarSolicitudAmistadAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor)
        {
            AmigosSrv.AmigosManejadorClient cliente = ObtenerCliente();

            try
            {
                AmigosSrv.ResultadoOperacionDTO resultado = await cliente
                    .EnviarSolicitudAmistadAsync(nombreUsuarioEmisor, nombreUsuarioReceptor)
                    .ConfigureAwait(false);

                return ConvertirResultado(resultado);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoAmigosOperacion);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public async Task<ResultadoOperacion> ResponderSolicitudAmistadAsync(
            string nombreUsuarioEmisor,
            string nombreUsuarioReceptor,
            bool aceptarSolicitud)
        {
            AmigosSrv.AmigosManejadorClient cliente = ObtenerCliente();

            try
            {
                AmigosSrv.ResultadoOperacionDTO resultado = await cliente
                    .ResponderSolicitudAmistadAsync(nombreUsuarioEmisor, nombreUsuarioReceptor, aceptarSolicitud)
                    .ConfigureAwait(false);

                return ConvertirResultado(resultado);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoAmigosOperacion);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public async Task<ResultadoOperacion> EliminarAmigoAsync(string nombreUsuarioA, string nombreUsuarioB)
        {
            AmigosSrv.AmigosManejadorClient cliente = ObtenerCliente();

            try
            {
                AmigosSrv.ResultadoOperacionDTO resultado = await cliente
                    .EliminarAmigoAsync(nombreUsuarioA, nombreUsuarioB)
                    .ConfigureAwait(false);

                return ConvertirResultado(resultado);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoAmigosOperacion);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                ResetCliente();
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public IReadOnlyCollection<string> ObtenerAmigos()
        {
            lock (_sincronizacion)
            {
                return _amigos
                    .OrderBy(a => a, StringComparer.CurrentCultureIgnoreCase)
                    .ToArray();
            }
        }

        public IReadOnlyCollection<SolicitudAmistad> ObtenerSolicitudesPendientes()
        {
            lock (_sincronizacion)
            {
                return _solicitudesPendientes
                    .Select(s => new SolicitudAmistad(s.UsuarioEmisor, s.UsuarioReceptor, s.EstaAceptada))
                    .ToArray();
            }
        }

        public void NotificarSolicitudAmistad(AmigosSrv.SolicitudAmistadDTO solicitudDto)
        {
            if (solicitudDto == null)
            {
                return;
            }

            var solicitud = new SolicitudAmistad(
                solicitudDto.UsuarioEmisor,
                solicitudDto.UsuarioReceptor,
                solicitudDto.EstaAceptada);

            bool esReceptorActual = string.Equals(
                solicitud.UsuarioReceptor,
                _usuarioActual,
                StringComparison.OrdinalIgnoreCase);

            if (esReceptorActual && !solicitud.EstaAceptada)
            {
                lock (_sincronizacion)
                {
                    if (!_solicitudesPendientes.Any(s =>
                            string.Equals(s.UsuarioEmisor, solicitud.UsuarioEmisor, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(s.UsuarioReceptor, solicitud.UsuarioReceptor, StringComparison.OrdinalIgnoreCase)))
                    {
                        _solicitudesPendientes.Add(solicitud);
                    }
                }
            }

            SolicitudRecibida?.Invoke(this, new SolicitudAmistadEventArgs(solicitud));
        }

        public void NotificarRespuestaSolicitud(AmigosSrv.RespuestaSolicitudAmistadDTO respuestaDto)
        {
            if (respuestaDto == null)
            {
                return;
            }

            var respuesta = new RespuestaSolicitudAmistad(
                respuestaDto.UsuarioEmisor,
                respuestaDto.UsuarioReceptor,
                respuestaDto.SolicitudAceptada);

            bool esEmisorActual = string.Equals(respuesta.UsuarioEmisor, _usuarioActual, StringComparison.OrdinalIgnoreCase);
            bool esReceptorActual = string.Equals(respuesta.UsuarioReceptor, _usuarioActual, StringComparison.OrdinalIgnoreCase);

            lock (_sincronizacion)
            {
                if (esReceptorActual)
                {
                    _solicitudesPendientes.RemoveAll(s =>
                        string.Equals(s.UsuarioEmisor, respuesta.UsuarioEmisor, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(s.UsuarioReceptor, respuesta.UsuarioReceptor, StringComparison.OrdinalIgnoreCase));
                }

                if (respuesta.SolicitudAceptada && (esEmisorActual || esReceptorActual))
                {
                    string otroUsuario = respuesta.ObtenerOtroUsuario(_usuarioActual);
                    if (!string.IsNullOrWhiteSpace(otroUsuario))
                    {
                        _amigos.Add(otroUsuario);
                    }
                }
            }

            SolicitudRespondida?.Invoke(this, new RespuestaSolicitudAmistadEventArgs(respuesta));
        }

        public void NotificarAmistadEliminada(AmigosSrv.AmistadEliminadaDTO amistadDto)
        {
            if (amistadDto == null)
            {
                return;
            }

            var amistad = new AmistadEliminada(amistadDto.UsuarioA, amistadDto.UsuarioB);

            if (amistad.InvolucraUsuario(_usuarioActual))
            {
                string otroUsuario = amistad.ObtenerOtroUsuario(_usuarioActual);
                if (!string.IsNullOrWhiteSpace(otroUsuario))
                {
                    lock (_sincronizacion)
                    {
                        _amigos.RemoveWhere(a => string.Equals(a, otroUsuario, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            AmistadEliminada?.Invoke(this, new AmistadEliminadaEventArgs(amistad));
        }

        private AmigosSrv.AmigosManejadorClient ObtenerCliente()
        {
            lock (_clienteLock)
            {
                if (_cliente != null)
                {
                    if (_cliente.State != CommunicationState.Faulted)
                    {
                        return _cliente;
                    }

                    try
                    {
                        _cliente.Abort();
                    }
                    catch
                    {
                        // Ignorado de manera intencional.
                    }

                    _cliente = null;
                    _contexto = null;
                }

                _contexto = new InstanceContext(this);
                _cliente = new AmigosSrv.AmigosManejadorClient(_contexto, AmigosEndpoint);
                return _cliente;
            }
        }

        private static ResultadoOperacion ConvertirResultado(AmigosSrv.ResultadoOperacionDTO resultado)
        {
            if (resultado == null)
            {
                return null;
            }

            string mensaje = MensajeServidorHelper.Localizar(resultado.Mensaje, resultado.Mensaje);
            return resultado.OperacionExitosa
                ? ResultadoOperacion.Exitoso(mensaje)
                : ResultadoOperacion.Fallo(mensaje);
        }

        private void ResetCliente()
        {
            lock (_clienteLock)
            {
                if (_cliente != null)
                {
                    try
                    {
                        _cliente.Abort();
                    }
                    catch
                    {
                        // Ignorado.
                    }
                    finally
                    {
                        _cliente = null;
                        _contexto = null;
                    }
                }
            }
        }

        private void CerrarCliente()
        {
            lock (_clienteLock)
            {
                if (_cliente == null)
                {
                    return;
                }

                try
                {
                    if (_cliente.State != CommunicationState.Faulted)
                    {
                        _cliente.Close();
                    }
                    else
                    {
                        _cliente.Abort();
                    }
                }
                catch
                {
                    _cliente.Abort();
                }
                finally
                {
                    _cliente = null;
                    _contexto = null;
                }
            }
        }
    }
}
