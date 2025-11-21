using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Implementa la logica de envio y confirmacion de codigos de verificacion.
    /// </summary>
    public class CodigoVerificacionServicio : ICodigoVerificacionServicio
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(CodigoVerificacionServicio));
        private const string CodigoVerificacionEndpoint =
            "BasicHttpBinding_ICodigoVerificacionManejador";
        private const string CuentaEndpoint = "BasicHttpBinding_ICuentaManejador";

        /// <summary>
        /// Solicita al servidor el envio de un codigo al correo del usuario para registro.
        /// </summary>
        public async Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(
            DTOs.NuevaCuentaDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            var cliente = new PictionaryServidorServicioCodigoVerificacion
                .CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);

            try
            {
                var resultado = await WcfClienteAyudante
                    .UsarAsincronoAsync(cliente, c => c.SolicitarCodigoVerificacionAsync
                        (solicitud))
                    .ConfigureAwait(false);

                if (resultado != null && resultado.CodigoEnviado)
                {
                    _logger.InfoFormat("Código de verificación solicitado para: {0}", 
                        solicitud.Correo);
                }

                return resultado;
            }
            catch (FaultException ex)
            {
                _logger.Warn("Error al solicitar código (Servidor).", ex);
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorCodigoVerificacion);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.Error("Endpoint de verificación no encontrado.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al solicitar código.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicación al solicitar código.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida al solicitar código.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }

        /// <summary>
        /// Solicita el reenvio del codigo de verificacion usando un token previo.
        /// </summary>
        public async Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(
            string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException("Token requerido", nameof(tokenCodigo));
            }

            var cliente = new PictionaryServidorServicioCuenta
                .CuentaManejadorClient(CuentaEndpoint);

            try
            {
                var reenvioCodigoVerificacionDto = new DTOs.ReenvioCodigoVerificacionDTO
                {
                    TokenCodigo = tokenCodigo.Trim()
                };

                var resultado = await WcfClienteAyudante
                    .UsarAsincronoAsync(
                        cliente,
                        c => c.ReenviarCodigoVerificacionAsync(reenvioCodigoVerificacionDto))
                    .ConfigureAwait(false);

                _logger.Info("Reenvío de código solicitado.");
                return resultado;
            }
            catch (FaultException ex)
            {
                _logger.Warn("Error al reenviar código (Servidor).", ex);
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorCodigoVerificacion);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.Error("Endpoint de reenvío no encontrado.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al reenviar código.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicación al reenviar código.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida al reenviar código.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }

        /// <summary>
        /// Confirma que el codigo ingresado por el usuario corresponde al enviado.
        /// </summary>
        public async Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(
            string tokenCodigo,
            string codigoIngresado)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException("Token requerido", nameof(tokenCodigo));
            }

            if (string.IsNullOrWhiteSpace(codigoIngresado))
            {
                throw new ArgumentException("Código requerido", nameof(codigoIngresado));
            }

            var cliente = new PictionaryServidorServicioCodigoVerificacion
                .CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);

            try
            {
                var confirmacionCodigoDto = new DTOs.ConfirmacionCodigoDTO
                {
                    TokenCodigo = tokenCodigo,
                    CodigoIngresado = codigoIngresado
                };

                var resultado = await WcfClienteAyudante
                    .UsarAsincronoAsync(
                        cliente,
                        c => c.ConfirmarCodigoVerificacionAsync(confirmacionCodigoDto))
                    .ConfigureAwait(false);

                if (resultado != null && resultado.RegistroExitoso)
                {
                    _logger.Info("Código de registro confirmado exitosamente.");
                }

                return resultado;
            }
            catch (FaultException ex)
            {
                _logger.Warn("Error al confirmar código (Servidor).", ex);
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorCodigoVerificacion);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.Error("Endpoint de confirmación no encontrado.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al confirmar código.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicación al confirmar código.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida al confirmar código.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }
    }
}