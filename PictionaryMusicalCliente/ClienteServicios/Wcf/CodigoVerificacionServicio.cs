using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    public class CodigoVerificacionServicio : ICodigoVerificacionServicio
    {
        private const string CodigoVerificacionEndpoint = "BasicHttpBinding_ICodigoVerificacionManejador";
        private const string CuentaEndpoint = "BasicHttpBinding_ICuentaManejador";

        public async Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(DTOs.NuevaCuentaDTO solicitud)
        {
            if (solicitud == null)
                throw new ArgumentNullException(nameof(solicitud));

            var cliente = new PictionaryServidorServicioCodigoVerificacion.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);

            try
            {
                return await WcfClienteAyudante
                    .UsarAsincrono(cliente, c => c.SolicitarCodigoVerificacionAsync(solicitud))
                    .ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoServidorCodigoVerificacion);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public async Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
                throw new ArgumentException("Token requerido", nameof(tokenCodigo));

            var cliente = new PictionaryServidorServicioCuenta.CuentaManejadorClient(CuentaEndpoint);

            try
            {
                var reenvioCodigoVerificacionDto = new DTOs.ReenvioCodigoVerificacionDTO
                {
                    TokenCodigo = tokenCodigo.Trim()
                };

                return await WcfClienteAyudante
                    .UsarAsincrono(cliente, c => c.ReenviarCodigoVerificacionAsync(reenvioCodigoVerificacionDto))
                    .ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoServidorCodigoVerificacion);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public async Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
                throw new ArgumentException("Token requerido", nameof(tokenCodigo));

            if (string.IsNullOrWhiteSpace(codigoIngresado))
                throw new ArgumentException("Código requerido", nameof(codigoIngresado));

            var cliente = new PictionaryServidorServicioCodigoVerificacion.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);

            try
            {
                var confirmacionCodigoDto = new DTOs.ConfirmacionCodigoDTO
                {
                    TokenCodigo = tokenCodigo,
                    CodigoIngresado = codigoIngresado
                };

                return await WcfClienteAyudante
                    .UsarAsincrono(cliente, c => c.ConfirmarCodigoVerificacionAsync(confirmacionCodigoDto))
                    .ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoServidorCodigoVerificacion);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }
    }
}
