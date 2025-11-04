using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using System;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using log4net;

namespace Servicios.Servicios
{
    public class CambioContrasenaManejador : ICambioContrasenaManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CambioContrasenaManejador));

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

        public ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenvioCodigoDTO solicitud)
        {
            try
            {
                return ServicioRecuperacionCuenta.ReenviarCodigoRecuperacion(solicitud);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Solicitud inválida al reenviar código de recuperación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "Los datos proporcionados no son válidos para reenviar el código."
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al reenviar código de recuperación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "No fue posible reenviar el código de recuperación."
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al reenviar código de recuperación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "No fue posible reenviar el código de recuperación."
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
                _logger.Warn("Solicitud inválida al confirmar código de recuperación", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "Los datos proporcionados no son válidos para confirmar el código."
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al confirmar código de recuperación", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "No fue posible confirmar el código de recuperación."
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al confirmar código de recuperación", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "No fue posible confirmar el código de recuperación."
                };
            }
        }


        public ResultadoOperacionDTO ActualizarContrasena(ActualizacionContrasenaDTO solicitud)
        {
            try
            {
                return ServicioRecuperacionCuenta.ActualizarContrasena(solicitud);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Solicitud inválida al actualizar la contraseña", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "Los datos proporcionados no son válidos para actualizar la contraseña."
                };
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error("Validación de entidad fallida al actualizar la contraseña", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "No fue posible actualizar la contraseña."
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error de actualización de base de datos al actualizar la contraseña", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "No fue posible actualizar la contraseña."
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al actualizar la contraseña", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "No fue posible actualizar la contraseña."
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al actualizar la contraseña", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "No fue posible actualizar la contraseña."
                };
            }
        }
    }
}