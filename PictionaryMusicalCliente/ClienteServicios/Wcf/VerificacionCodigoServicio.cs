using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Servicio para manejar la confirmacion y reenvio de codigos de verificacion.
    /// </summary>
    public class VerificacionCodigoServicio : IVerificacionCodigoServicio
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(VerificacionCodigoServicio));

        /// <summary>
        /// Valida el codigo ingresado por el usuario contra el token del servidor.
        /// </summary>
        public async Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(
            string tokenCodigo,
            string codigoIngresado)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(
                    Lang.errorTextoTokenCodigoObligatorio,
                    nameof(tokenCodigo));
            }

            DTOs.ResultadoRegistroCuentaDTO resultado = await EjecutarOperacionAsync(
                () => CodigoVerificacionServicioAyudante.ConfirmarCodigoRegistroAsync(
                    tokenCodigo,
                    codigoIngresado),
                Lang.errorTextoServidorValidarCodigo).ConfigureAwait(false);

            if (resultado == null)
            {
                _logger.Warn("El servicio de confirmación de código retornó null.");
                return null;
            }

            if (resultado.RegistroExitoso)
            {
                _logger.InfoFormat("Código de registro confirmado exitosamente. Token: {0}",
                    tokenCodigo);
            }
            else
            {
                _logger.WarnFormat("Confirmación de código fallida. Razón: {0}",
                    resultado.Mensaje);
            }

            return resultado;
        }

        /// <summary>
        /// Solicita el reenvio del codigo de verificacion.
        /// </summary>
        public async Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(
            string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(
                    Lang.errorTextoTokenCodigoObligatorio,
                    nameof(tokenCodigo));
            }

            DTOs.ResultadoSolicitudCodigoDTO resultado = await EjecutarOperacionAsync(
                () => CodigoVerificacionServicioAyudante.ReenviarCodigoRegistroAsync(tokenCodigo),
                Lang.errorTextoServidorReenviarCodigo).ConfigureAwait(false);

            if (resultado == null)
            {
                _logger.Warn("El servicio de reenvío de código retornó null.");
                return null;
            }

            if (resultado.CodigoEnviado)
            {
                _logger.InfoFormat("Código de registro reenviado exitosamente. Token: {0}",
                    tokenCodigo);
            }

            return resultado;
        }

        private static async Task<T> EjecutarOperacionAsync<T>(
            Func<Task<T>> operacion,
            string mensajeErrorPredeterminado)
        {
            try
            {
                return await operacion().ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                _logger.WarnFormat("Error de lógica del servidor en verificación de código: {0}",
                    mensajeErrorPredeterminado, ex);
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    mensajeErrorPredeterminado);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.Error("Endpoint de verificación de código no encontrado.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout en servicio de verificación de código.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicación en servicio de verificación.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida en servicio de verificación.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }
    }
}