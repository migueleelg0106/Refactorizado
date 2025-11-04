using Servicios.Contratos;
using System;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using log4net;
using Servicios.Contratos.DTOs;

namespace Servicios.Servicios
{
    public class CodigoVerificacionManejador : ICodigoVerificacionManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CodigoVerificacionManejador));

        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            try
            {
                return ServicioVerificacionRegistro.SolicitarCodigo(nuevaCuenta);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Solicitud inválida al solicitar código de verificación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "Los datos proporcionados no son válidos para solicitar el código."
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al solicitar código de verificación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "No fue posible procesar la solicitud de verificación."
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al solicitar código de verificación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "No fue posible procesar la solicitud de verificación."
                };
            }
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenvioCodigoVerificacionDTO solicitud)
        {
            try
            {
                return ServicioVerificacionRegistro.ReenviarCodigo(solicitud);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Solicitud inválida al reenviar código de verificación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "Los datos proporcionados no son válidos para reenviar el código."
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al reenviar código de verificación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "No fue posible reenviar el código de verificación."
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al reenviar código de verificación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "No fue posible reenviar el código de verificación."
                };
            }
        }

        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion)
        {
            try
            {
                return ServicioVerificacionRegistro.ConfirmarCodigo(confirmacion);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Solicitud inválida al confirmar el código de verificación", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = "Los datos proporcionados no son válidos para confirmar el código."
                };
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error("Validación de entidad fallida al confirmar el código de verificación", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = "No se pudo confirmar el código de verificación."
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error de actualización de base de datos al confirmar el código de verificación", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = "No se pudo confirmar el código de verificación."
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al confirmar el código de verificación", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = "No se pudo confirmar el código de verificación."
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al confirmar el código de verificación", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = "No se pudo confirmar el código de verificación."
                };
            }
        }

        public ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud)
        {
            try
            {
                return ServicioRecuperacionCuenta.SolicitarCodigoRecuperacion(solicitud);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Solicitud inválida al solicitar código de recuperación", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "Los datos proporcionados no son válidos para recuperar la cuenta."
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al solicitar código de recuperación", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "No fue posible procesar la recuperación de la cuenta."
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al solicitar código de recuperación", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "No fue posible procesar la recuperación de la cuenta."
                };
            }
        }

        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion)
        {
            try
            {
                return ServicioRecuperacionCuenta.ConfirmarCodigoRecuperacion(confirmacion);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Solicitud inválida al confirmar el código de recuperación", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "Los datos proporcionados no son válidos para confirmar el código."
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al confirmar el código de recuperación", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "No fue posible confirmar el código de recuperación."
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al confirmar el código de recuperación", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "No fue posible confirmar el código de recuperación."
                };
            }
        }
    }
}