using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Provee servicios para el flujo de recuperacion y cambio de contraseña.
    /// </summary>
    public class CambioContrasenaServicio : ICambioContrasenaServicio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CambioContrasenaServicio));
        private const string Endpoint = "BasicHttpBinding_ICambioContrasenaManejador";

        /// <summary>
        /// Inicia el proceso de recuperacion solicitando un codigo al identificador dado.
        /// </summary>
        public async Task<DTOs.ResultadoSolicitudRecuperacionDTO> SolicitarCodigoRecuperacionAsync(
            string identificador)
        {
            if (string.IsNullOrWhiteSpace(identificador))
            {
                throw new ArgumentException(
                    Lang.errorTextoIdentificadorRecuperacionRequerido,
                    nameof(identificador));
            }

            DTOs.ResultadoSolicitudRecuperacionDTO resultado =
                await EjecutarConManejoDeErroresAsync(
                () => CodigoVerificacionServicioAyudante.SolicitarCodigoRecuperacionAsync
                (identificador),
                Lang.errorTextoServidorSolicitudCambioContrasena
            ).ConfigureAwait(false);

            if (resultado != null && resultado.CodigoEnviado)
            {
                _logger.InfoFormat("Código de recuperación solicitado para: {0}", 
                    identificador);
            }

            return resultado == null ? null : new DTOs.ResultadoSolicitudRecuperacionDTO
            {
                CuentaEncontrada = resultado.CuentaEncontrada,
                CodigoEnviado = resultado.CodigoEnviado,
                CorreoDestino = resultado.CorreoDestino,
                Mensaje = resultado.Mensaje,
                TokenCodigo = resultado.TokenCodigo
            };
        }

        /// <summary>
        /// Solicita el reenvio del codigo de recuperacion en caso de no haberlo recibido.
        /// </summary>
        public async Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRecuperacionAsync(
            string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(
                    Lang.errorTextoTokenCodigoObligatorio,
                    nameof(tokenCodigo));
            }

            DTOs.ResultadoSolicitudCodigoDTO resultado = await EjecutarConManejoDeErroresAsync(
                () => CodigoVerificacionServicioAyudante.ReenviarCodigoRecuperacionAsync
                (tokenCodigo),
                Lang.errorTextoServidorReenviarCodigo
            ).ConfigureAwait(false);

            if (resultado != null && resultado.CodigoEnviado)
            {
                _logger.Info("Reenvío de código de recuperación solicitado.");
            }

            return resultado == null ? null : new DTOs.ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = resultado.CodigoEnviado,
                Mensaje = resultado.Mensaje,
                TokenCodigo = resultado.TokenCodigo
            };
        }

        /// <summary>
        /// Verifica que el codigo ingresado para recuperacion sea valido.
        /// </summary>
        public async Task<DTOs.ResultadoOperacionDTO> ConfirmarCodigoRecuperacionAsync(
            string tokenCodigo,
            string codigoIngresado)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(
                    Lang.errorTextoTokenCodigoObligatorio,
                    nameof(tokenCodigo));
            }

            if (string.IsNullOrWhiteSpace(codigoIngresado))
            {
                throw new ArgumentException(
                    Lang.errorTextoCodigoVerificacionRequerido,
                    nameof(codigoIngresado));
            }

            DTOs.ResultadoOperacionDTO resultado = await EjecutarConManejoDeErroresAsync(
                () => CodigoVerificacionServicioAyudante.ConfirmarCodigoRecuperacionAsync(
                    tokenCodigo,
                    codigoIngresado),
                Lang.errorTextoServidorValidarCodigo
            ).ConfigureAwait(false);

            if (resultado != null && resultado.OperacionExitosa)
            {
                _logger.Info("Código de recuperación confirmado.");
            }

            return resultado == null ? null : new DTOs.ResultadoOperacionDTO
            {
                OperacionExitosa = resultado.OperacionExitosa,
                Mensaje = resultado.Mensaje
            };
        }

        /// <summary>
        /// Establece la nueva contraseña despues de haber verificado el codigo.
        /// </summary>
        public async Task<DTOs.ResultadoOperacionDTO> ActualizarContrasenaAsync(
            string tokenCodigo,
            string nuevaContrasena)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(
                    Lang.errorTextoTokenCodigoObligatorio,
                    nameof(tokenCodigo));
            }

            if (string.IsNullOrWhiteSpace(nuevaContrasena))
            {
                throw new ArgumentNullException(nameof(nuevaContrasena));
            }

            var cliente = new PictionaryServidorServicioCambioContrasena
                .CambioContrasenaManejadorClient(Endpoint);

            var solicitud = new DTOs.ActualizacionContrasenaDTO
            {
                TokenCodigo = tokenCodigo,
                NuevaContrasena = nuevaContrasena
            };

            DTOs.ResultadoOperacionDTO resultado = await EjecutarConManejoDeErroresAsync(
                () => WcfClienteAyudante.UsarAsincronoAsync(
                    cliente,
                    c => c.ActualizarContrasenaAsync(solicitud)),
                Lang.errorTextoServidorActualizarContrasena
            ).ConfigureAwait(false);

            if (resultado != null && resultado.OperacionExitosa)
            {
                _logger.Info("Contraseña actualizada mediante recuperación.");
            }

            return resultado == null ? null : new DTOs.ResultadoOperacionDTO
            {
                OperacionExitosa = resultado.OperacionExitosa,
                Mensaje = resultado.Mensaje
            };
        }

        private static async Task<TResult> EjecutarConManejoDeErroresAsync<TResult>(
            Func<Task<TResult>> operacion,
            string mensajeFallaPredeterminado)
        {
            try
            {
                return await operacion().ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                _logger.WarnFormat("Error de servidor en flujo de cambio de contraseña: {0}",
                    mensajeFallaPredeterminado, ex);
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    mensajeFallaPredeterminado);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.Error("Endpoint no encontrado en flujo de cambio de contraseña.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout en flujo de cambio de contraseña.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicación en flujo de cambio de contraseña.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida en flujo de cambio de contraseña.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoPrepararSolicitudCambioContrasena,
                    ex);
            }
        }
    }
}