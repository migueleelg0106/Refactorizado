using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using AmigosSrv = PictionaryMusicalCliente.PictionaryServidorServicioAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class AmigosService : IAmigosService
    {
        private const string AmigosEndpoint = "WSDualHttpBinding_IAmigosManejador";

        private readonly SynchronizationContext _contexto;

        public AmigosService()
        {
            _contexto = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public event EventHandler<SolicitudAmistadRecibidaEventArgs> SolicitudRecibida;

        public event EventHandler<RespuestaSolicitudAmistadEventArgs> SolicitudRespondida;

        public event EventHandler<AmistadEliminadaEventArgs> AmistadEliminada;

        public async Task<ResultadoOperacion> EnviarSolicitudAsync(
            string nombreUsuarioRemitente,
            string nombreUsuarioReceptor)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuarioRemitente))
            {
                throw new ArgumentException("El remitente es obligatorio", nameof(nombreUsuarioRemitente));
            }

            if (string.IsNullOrWhiteSpace(nombreUsuarioReceptor))
            {
                throw new ArgumentException("El receptor es obligatorio", nameof(nombreUsuarioReceptor));
            }

            AmigosCallback callback = CrearCallback();
            var cliente = new AmigosSrv.AmigosManejadorClient(new InstanceContext(callback), AmigosEndpoint);

            try
            {
                AmigosSrv.ResultadoOperacionDTO resultado = await WcfClientHelper
                    .UsarAsync(cliente, c => c.EnviarSolicitudAmistadAsync(nombreUsuarioRemitente, nombreUsuarioReceptor))
                    .ConfigureAwait(false);

                return ConvertirResultado(resultado);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorNoDisponible);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.avisoTextoServidorTiempoSesion, ex);
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

        public async Task<ResultadoOperacion> EliminarAmigoAsync(string nombreUsuario, string nombreAmigo)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException("El usuario es obligatorio", nameof(nombreUsuario));
            }

            if (string.IsNullOrWhiteSpace(nombreAmigo))
            {
                throw new ArgumentException("El amigo es obligatorio", nameof(nombreAmigo));
            }

            AmigosCallback callback = CrearCallback();
            var cliente = new AmigosSrv.AmigosManejadorClient(new InstanceContext(callback), AmigosEndpoint);

            try
            {
                AmigosSrv.ResultadoOperacionDTO resultado = await WcfClientHelper
                    .UsarAsync(cliente, c => c.EliminarAmigoAsync(nombreUsuario, nombreAmigo))
                    .ConfigureAwait(false);

                return ConvertirResultado(resultado);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorNoDisponible);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.avisoTextoServidorTiempoSesion, ex);
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

        private static ResultadoOperacion ConvertirResultado(AmigosSrv.ResultadoOperacionDTO resultado)
        {
            if (resultado == null)
            {
                return ResultadoOperacion.Fallo(Lang.errorTextoServidorNoDisponible);
            }

            return resultado.OperacionExitosa
                ? ResultadoOperacion.Exitoso(resultado.Mensaje)
                : ResultadoOperacion.Fallo(string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? Lang.errorTextoServidorNoDisponible
                    : resultado.Mensaje);
        }

        private AmigosCallback CrearCallback()
        {
            return new AmigosCallback(
                ProcesarSolicitudAmistadRecibida,
                ProcesarSolicitudAmistadRespondida,
                ProcesarAmistadEliminada);
        }

        private void ProcesarSolicitudAmistadRecibida(AmigosSrv.SolicitudAmistadNotificacionDTO notificacion)
        {
            if (notificacion == null)
            {
                return;
            }

            var args = new SolicitudAmistadRecibidaEventArgs(notificacion.Remitente, notificacion.Receptor);
            NotificarEnContexto(SolicitudRecibida, args);
        }

        private void ProcesarSolicitudAmistadRespondida(AmigosSrv.RespuestaSolicitudAmistadNotificacionDTO notificacion)
        {
            if (notificacion == null)
            {
                return;
            }

            var args = new RespuestaSolicitudAmistadEventArgs(
                notificacion.Remitente,
                notificacion.Receptor,
                notificacion.Aceptada);

            NotificarEnContexto(SolicitudRespondida, args);
        }

        private void ProcesarAmistadEliminada(AmigosSrv.AmistadEliminadaNotificacionDTO notificacion)
        {
            if (notificacion == null)
            {
                return;
            }

            var args = new AmistadEliminadaEventArgs(notificacion.Jugador, notificacion.Amigo);
            NotificarEnContexto(AmistadEliminada, args);
        }

        private void NotificarEnContexto<TArgs>(EventHandler<TArgs> evento, TArgs args)
            where TArgs : EventArgs
        {
            if (evento == null)
            {
                return;
            }

            _contexto.Post(state =>
            {
                var data = (Tuple<EventHandler<TArgs>, TArgs>)state;
                data.Item1?.Invoke(this, data.Item2);
            }, Tuple.Create(evento, args));
        }

        private class AmigosCallback : AmigosSrv.IAmigosManejadorCallback
        {
            private readonly Action<AmigosSrv.SolicitudAmistadNotificacionDTO> _solicitudRecibida;
            private readonly Action<AmigosSrv.RespuestaSolicitudAmistadNotificacionDTO> _solicitudRespondida;
            private readonly Action<AmigosSrv.AmistadEliminadaNotificacionDTO> _amistadEliminada;

            public AmigosCallback(
                Action<AmigosSrv.SolicitudAmistadNotificacionDTO> solicitudRecibida,
                Action<AmigosSrv.RespuestaSolicitudAmistadNotificacionDTO> solicitudRespondida,
                Action<AmigosSrv.AmistadEliminadaNotificacionDTO> amistadEliminada)
            {
                _solicitudRecibida = solicitudRecibida ?? throw new ArgumentNullException(nameof(solicitudRecibida));
                _solicitudRespondida = solicitudRespondida ?? throw new ArgumentNullException(nameof(solicitudRespondida));
                _amistadEliminada = amistadEliminada ?? throw new ArgumentNullException(nameof(amistadEliminada));
            }

            public void SolicitudAmistadRecibida(AmigosSrv.SolicitudAmistadNotificacionDTO notificacion)
            {
                _solicitudRecibida(notificacion);
            }

            public void SolicitudAmistadRespondida(AmigosSrv.RespuestaSolicitudAmistadNotificacionDTO notificacion)
            {
                _solicitudRespondida(notificacion);
            }

            public void AmistadEliminada(AmigosSrv.AmistadEliminadaNotificacionDTO notificacion)
            {
                _amistadEliminada(notificacion);
            }
        }
    }
}
