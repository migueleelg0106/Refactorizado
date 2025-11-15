using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using System;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using log4net;
using Servicios.Servicios.Constantes;

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
                    Mensaje = MensajesError.DatosRecuperacionInvalidos
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al solicitar código de recuperación", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.ErrorRecuperarCuenta
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al solicitar código de recuperación", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.ErrorRecuperarCuenta
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
                    Mensaje = MensajesError.DatosReenvioCodigo
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al reenviar código de recuperación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.ErrorReenviarCodigo
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al reenviar código de recuperación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.ErrorReenviarCodigo
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
                    Mensaje = MensajesError.DatosConfirmacionInvalidos
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al confirmar código de recuperación", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.ErrorConfirmarCodigo
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al confirmar código de recuperación", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.ErrorConfirmarCodigo
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
                    Mensaje = MensajesError.DatosActualizacionContrasena
                };
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error("Validación de entidad fallida al actualizar la contraseña", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.ErrorActualizarContrasena
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error de actualización de base de datos al actualizar la contraseña", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.ErrorActualizarContrasena
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al actualizar la contraseña", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.ErrorActualizarContrasena
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al actualizar la contraseña", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.ErrorActualizarContrasena
                };
            }
        }
    }
}