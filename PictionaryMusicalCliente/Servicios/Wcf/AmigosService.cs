using System;
using System.ServiceModel;
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

            var callback = new AmigosCallback();
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

            var callback = new AmigosCallback();
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

        private class AmigosCallback : AmigosSrv.IAmigosManejadorCallback
        {
            public void SolicitudAmistadRecibida(AmigosSrv.SolicitudAmistadNotificacionDTO notificacion)
            {
            }

            public void RespuestaSolicitudAmistadRecibida(AmigosSrv.RespuestaSolicitudAmistadNotificacionDTO notificacion)
            {
            }

            public void AmigoEliminado(AmigosSrv.SolicitudAmistadNotificacionDTO notificacion)
            {
            }
        }
    }
}
