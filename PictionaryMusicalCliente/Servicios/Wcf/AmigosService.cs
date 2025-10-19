using System;
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
    public class AmigosService : IAmigosService
    {
        private const string AmigosEndpoint = "NetTcpBinding_IAmigosManejador";

        public async Task<ResultadoOperacion> EnviarSolicitudAmistadAsync(SolicitudAmistad solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            if (solicitud.RemitenteId <= 0)
            {
                throw new ArgumentException("Identificador de remitente inválido.", nameof(solicitud));
            }

            if (solicitud.DestinatarioId <= 0)
            {
                throw new ArgumentException("Identificador de destinatario inválido.", nameof(solicitud));
            }

            var cliente = CrearCliente();

            try
            {
                var dto = new AmigosSrv.SolicitudAmistadDTO
                {
                    RemitenteId = solicitud.RemitenteId,
                    DestinatarioId = solicitud.DestinatarioId
                };

                AmigosSrv.ResultadoOperacionDTO resultado = await WcfClientHelper
                    .UsarAsync(cliente, c => c.EnviarSolicitudAmistadAsync(dto))
                    .ConfigureAwait(false);

                return ConvertirResultado(resultado, Lang.avisoTextoSolicitudAmistadEnviada);
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

        public async Task<ResultadoOperacion> EliminarAmigoAsync(OperacionAmistad solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            if (solicitud.JugadorId <= 0)
            {
                throw new ArgumentException("Identificador de jugador inválido.", nameof(solicitud));
            }

            if (solicitud.AmigoId <= 0)
            {
                throw new ArgumentException("Identificador de amigo inválido.", nameof(solicitud));
            }

            var cliente = CrearCliente();

            try
            {
                var dto = new AmigosSrv.OperacionAmistadDTO
                {
                    JugadorId = solicitud.JugadorId,
                    AmigoId = solicitud.AmigoId
                };

                AmigosSrv.ResultadoOperacionDTO resultado = await WcfClientHelper
                    .UsarAsync(cliente, c => c.EliminarAmigoAsync(dto))
                    .ConfigureAwait(false);

                return ConvertirResultado(resultado, Lang.avisoTextoAmistadEliminada);
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

        private static AmigosSrv.AmigosManejadorClient CrearCliente()
        {
            var callback = new AmigosCallback();
            var contexto = new InstanceContext(callback);
            return new AmigosSrv.AmigosManejadorClient(contexto, AmigosEndpoint);
        }

        private static ResultadoOperacion ConvertirResultado(AmigosSrv.ResultadoOperacionDTO resultado, string mensajePredeterminado)
        {
            if (resultado == null)
            {
                return null;
            }

            string mensaje = MensajeServidorHelper.Localizar(resultado.Mensaje, mensajePredeterminado);
            return resultado.OperacionExitosa
                ? ResultadoOperacion.Exitoso(mensaje)
                : ResultadoOperacion.Fallo(mensaje);
        }

        private sealed class AmigosCallback : AmigosSrv.IAmigosManejadorCallback
        {
            public void SolicitudRecibida(AmigosSrv.SolicitudAmistadNotificacionDTO solicitud)
            {
            }

            public void SolicitudActualizada(AmigosSrv.SolicitudAmistadEstadoDTO solicitud)
            {
            }

            public void AmigoAgregado(AmigosSrv.AmigoDTO amigo)
            {
            }

            public void AmistadEliminada(AmigosSrv.AmistadEliminadaDTO amistadEliminada)
            {
            }
        }
    }
}
