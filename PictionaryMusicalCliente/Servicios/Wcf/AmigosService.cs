using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using AmigosSrv = PictionaryMusicalCliente.PictionaryServidorServicioAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class AmigosService : IAmigosService, AmigosSrv.IAmigosManejadorCallback
    {
        private const string AmigosEndpoint = "NetTcpBinding_IAmigosManejador";

        public async Task<ResultadoOperacion> EnviarSolicitudAmistadAsync(
            string nombreUsuarioRemitente,
            string nombreUsuarioReceptor)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuarioRemitente))
            {
                throw new ArgumentException(
                    "El nombre de usuario remitente es obligatorio.",
                    nameof(nombreUsuarioRemitente));
            }

            if (string.IsNullOrWhiteSpace(nombreUsuarioReceptor))
            {
                throw new ArgumentException(
                    "El nombre de usuario receptor es obligatorio.",
                    nameof(nombreUsuarioReceptor));
            }

            var contexto = new InstanceContext(this);
            var cliente = new AmigosSrv.AmigosManejadorClient(contexto, AmigosEndpoint);

            try
            {
                AmigosSrv.ResultadoOperacionDTO resultado = await WcfClientHelper
                    .UsarAsync(
                        cliente,
                        c => c.EnviarSolicitudAmistadAsync(
                            nombreUsuarioRemitente,
                            nombreUsuarioReceptor))
                    .ConfigureAwait(false);

                if (resultado == null)
                {
                    return ResultadoOperacion.Fallo(Lang.errorTextoErrorProcesarSolicitud);
                }

                string mensaje = MensajeServidorHelper.Localizar(
                    resultado.Mensaje,
                    resultado.OperacionExitosa
                        ? Lang.buscarAmigoTextoSolicitudEnviada
                        : Lang.buscarAmigoErrorEnviarSolicitud);

                return resultado.OperacionExitosa
                    ? ResultadoOperacion.Exitoso(mensaje)
                    : ResultadoOperacion.Fallo(mensaje);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    Lang.errorTextoErrorProcesarSolicitud);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }

        public void SolicitudAmistadRecibida(AmigosSrv.SolicitudAmistadNotificacionDTO notificacion)
        {
            // No se requiere manejo de notificaciones en esta vista.
        }

        public void SolicitudAmistadRespondida(AmigosSrv.RespuestaSolicitudAmistadNotificacionDTO notificacion)
        {
            // No se requiere manejo de notificaciones en esta vista.
        }

        public void AmistadEliminada(AmigosSrv.AmistadEliminadaNotificacionDTO notificacion)
        {
            // No se requiere manejo de notificaciones en esta vista.
        }
    }
}
