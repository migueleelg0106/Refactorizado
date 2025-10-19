using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.Sesiones;
using AmigosSrv = PictionaryMusicalCliente.PictionaryServidorServicioAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public sealed class AmigosService : IAmigosService, AmigosSrv.IAmigosManejadorCallback
    {
        private static readonly Lazy<AmigosService> InstanciaInterna = new Lazy<AmigosService>(() => new AmigosService());

        private readonly object _sincronizacion = new object();
        private AmigosSrv.AmigosManejadorClient _cliente;

        private AmigosService()
        {
        }

        public static AmigosService Instancia => InstanciaInterna.Value;

        public event EventHandler<SolicitudAmistadNotificacion> SolicitudAmistadRecibida;

        public event EventHandler<RespuestaSolicitudAmistadNotificacion> SolicitudAmistadRespondida;

        public event EventHandler<AmistadEliminadaNotificacion> AmistadEliminada;

        public async Task<ResultadoOperacion> EnviarSolicitudAsync(string nombreUsuarioReceptor)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuarioReceptor))
            {
                throw new ArgumentException("El nombre de usuario del receptor es obligatorio.", nameof(nombreUsuarioReceptor));
            }

            string remitente = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(remitente))
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud);
            }

            AmigosSrv.AmigosManejadorClient cliente = ObtenerCliente();

            try
            {
                AmigosSrv.ResultadoOperacionDTO resultado = await Task.Run(() =>
                    cliente.EnviarSolicitudAmistad(remitente, nombreUsuarioReceptor.Trim()))
                    .ConfigureAwait(false);

                return ConvertirResultado(resultado, Lang.errorTextoErrorProcesarSolicitud);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public async Task<ResultadoOperacion> ResponderSolicitudAsync(string nombreUsuarioRemitente, bool aceptada)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuarioRemitente))
            {
                throw new ArgumentException("El nombre de usuario del remitente es obligatorio.", nameof(nombreUsuarioRemitente));
            }

            string receptor = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(receptor))
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud);
            }

            AmigosSrv.AmigosManejadorClient cliente = ObtenerCliente();

            try
            {
                AmigosSrv.ResultadoOperacionDTO resultado = await Task.Run(() =>
                    cliente.ResponderSolicitudAmistad(nombreUsuarioRemitente.Trim(), receptor, aceptada))
                    .ConfigureAwait(false);

                return ConvertirResultado(resultado, Lang.errorTextoErrorProcesarSolicitud);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public async Task<ResultadoOperacion> EliminarAmigoAsync(string nombreUsuarioAmigo)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuarioAmigo))
            {
                throw new ArgumentException("El nombre de usuario a eliminar es obligatorio.", nameof(nombreUsuarioAmigo));
            }

            string remitente = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(remitente))
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud);
            }

            AmigosSrv.AmigosManejadorClient cliente = ObtenerCliente();

            try
            {
                AmigosSrv.ResultadoOperacionDTO resultado = await Task.Run(() =>
                    cliente.EliminarAmigo(remitente, nombreUsuarioAmigo.Trim()))
                    .ConfigureAwait(false);

                return ConvertirResultado(resultado, Lang.errorTextoErrorProcesarSolicitud);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        void AmigosSrv.IAmigosManejadorCallback.SolicitudAmistadRecibida(AmigosSrv.SolicitudAmistadNotificacionDTO notificacion)
        {
            var evento = SolicitudAmistadRecibida;
            if (evento != null && notificacion != null)
            {
                evento.Invoke(this, new SolicitudAmistadNotificacion
                {
                    Remitente = notificacion.Remitente,
                    Receptor = notificacion.Receptor
                });
            }
        }

        void AmigosSrv.IAmigosManejadorCallback.SolicitudAmistadRespondida(AmigosSrv.RespuestaSolicitudAmistadNotificacionDTO notificacion)
        {
            var evento = SolicitudAmistadRespondida;
            if (evento != null && notificacion != null)
            {
                evento.Invoke(this, new RespuestaSolicitudAmistadNotificacion
                {
                    Remitente = notificacion.Remitente,
                    Receptor = notificacion.Receptor,
                    Aceptada = notificacion.Aceptada
                });
            }
        }

        void AmigosSrv.IAmigosManejadorCallback.AmistadEliminada(AmigosSrv.AmistadEliminadaNotificacionDTO notificacion)
        {
            var evento = AmistadEliminada;
            if (evento != null && notificacion != null)
            {
                evento.Invoke(this, new AmistadEliminadaNotificacion
                {
                    Jugador = notificacion.Jugador,
                    Amigo = notificacion.Amigo
                });
            }
        }

        private AmigosSrv.AmigosManejadorClient ObtenerCliente()
        {
            lock (_sincronizacion)
            {
                if (_cliente != null && _cliente.State != CommunicationState.Closed &&
                    _cliente.State != CommunicationState.Closing && _cliente.State != CommunicationState.Faulted)
                {
                    return _cliente;
                }

                LiberarCliente();

                var contexto = new InstanceContext(this);
                var cliente = new AmigosSrv.AmigosManejadorClient(contexto, "NetTcpBinding_IAmigosManejador");
                cliente.InnerChannel.Closed += CanalCerrado;
                cliente.InnerChannel.Faulted += CanalCerrado;

                _cliente = cliente;
                return _cliente;
            }
        }

        private void CanalCerrado(object sender, EventArgs e)
        {
            lock (_sincronizacion)
            {
                LiberarCliente();
            }
        }

        private void LiberarCliente()
        {
            if (_cliente == null)
            {
                return;
            }

            try
            {
                _cliente.InnerChannel.Closed -= CanalCerrado;
                _cliente.InnerChannel.Faulted -= CanalCerrado;
                if (_cliente.State == CommunicationState.Opened)
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
            }
        }

        private static ResultadoOperacion ConvertirResultado(AmigosSrv.ResultadoOperacionDTO resultado, string mensajePredeterminado)
        {
            if (resultado == null)
            {
                return ResultadoOperacion.Fallo(mensajePredeterminado);
            }

            string mensaje = MensajeServidorHelper.Localizar(resultado.Mensaje, mensajePredeterminado);
            return resultado.OperacionExitosa
                ? ResultadoOperacion.Exitoso(mensaje)
                : ResultadoOperacion.Fallo(mensaje);
        }
    }
}
