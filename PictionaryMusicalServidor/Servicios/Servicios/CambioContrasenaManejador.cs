using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using log4net;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de recuperacion y cambio de contrasena de usuarios.
    /// Maneja el proceso completo de recuperacion incluyendo solicitud, reenvio, confirmacion de codigos y actualizacion de contrasena.
    /// Delega la logica de negocio al RecuperacionCuentaServicio.
    /// </summary>
    public class CambioContrasenaManejador : ICambioContrasenaManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CambioContrasenaManejador));

        /// <summary>
        /// Solicita un codigo de verificacion para recuperar la cuenta.
        /// Busca la cuenta por identificador, genera un codigo y lo envia por correo.
        /// </summary>
        /// <param name="solicitud">Datos de la solicitud con identificador del usuario.</param>
        /// <returns>Resultado indicando si se encontro la cuenta y si el codigo fue enviado.</returns>
        public ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud)
        {
            try
            {
                var resultado = RecuperacionCuentaServicio.SolicitarCodigoRecuperacion(solicitud);
                if (resultado.CodigoEnviado)
                {
                    _logger.InfoFormat("Solicitud de recuperación de cuenta iniciada para '{0}'.", solicitud.Identificador);
                }
                else
                {
                    _logger.WarnFormat("Solicitud de recuperación fallida para '{0}': {1}", solicitud.Identificador, resultado.Mensaje);
                }
                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Argumento nulo al solicitar código de recuperación. Los datos de solicitud son nulos.", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosRecuperacionInvalidos
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al solicitar código de recuperación. Fallo en la búsqueda de usuario.", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorRecuperarCuenta
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al solicitar código de recuperación. No se pudo procesar la solicitud.", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorRecuperarCuenta
                };
            }
        }

        /// <summary>
        /// Reenvia el codigo de recuperacion previamente solicitado.
        /// Valida el token de sesion y reenvia el codigo por correo.
        /// </summary>
        /// <param name="solicitud">Datos con el token de la sesion de recuperacion.</param>
        /// <returns>Resultado indicando si el codigo fue reenviado exitosamente.</returns>
        public ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenvioCodigoDTO solicitud)
        {
            try
            {
                var resultado = RecuperacionCuentaServicio.ReenviarCodigoRecuperacion(solicitud);
                if (resultado.CodigoEnviado)
                {
                    _logger.InfoFormat("Reenviar código de recuperación de cuenta iniciada para '{0}'.", solicitud.TokenCodigo);
                }
                else
                {
                    _logger.WarnFormat("Solicitud de reenvío de código de recuperación fallida para '{0}': {1}", solicitud.TokenCodigo, resultado.Mensaje);
                }
                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Argumento nulo al reenviar código de recuperación. Los datos de reenvío son nulos.", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosReenvioCodigo
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al reenviar código de recuperación. Fallo en la verificación de solicitud.", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigo
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al reenviar código de recuperación. No se pudo procesar el reenvío.", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigo
                };
            }
        }

        /// <summary>
        /// Confirma el codigo de recuperacion ingresado por el usuario.
        /// Valida el token de sesion y el codigo, y permite continuar con el cambio de contrasena.
        /// </summary>
        /// <param name="confirmacion">Datos con el token y codigo ingresado.</param>
        /// <returns>Resultado indicando si el codigo fue confirmado exitosamente.</returns>
        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion)
        {
            try
            {
                var resultado = RecuperacionCuentaServicio.ConfirmarCodigoRecuperacion(confirmacion);
                if (resultado.OperacionExitosa)
                {
                    _logger.InfoFormat("Código de recuperación confirmado correctamente. Token sesión: '{0}'.", confirmacion.TokenCodigo);
                }
                else
                {
                    _logger.WarnFormat("Intento fallido de confirmación de código de recuperación. Token sesión: '{0}': {1}", confirmacion.TokenCodigo, resultado.Mensaje);
                }
                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Argumento nulo al confirmar código de recuperación. Los datos de confirmación son nulos.", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.DatosConfirmacionInvalidos
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al confirmar código de recuperación. Fallo en la verificación de código.", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al confirmar código de recuperación. No se pudo confirmar el código.", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
        }

        /// <summary>
        /// Actualiza la contrasena del usuario despues de confirmar el codigo.
        /// Valida el token, encripta la nueva contrasena con BCrypt y actualiza la base de datos.
        /// </summary>
        /// <param name="solicitud">Datos con el token y nueva contrasena.</param>
        /// <returns>Resultado indicando si la contrasena fue actualizada exitosamente.</returns>
        public ResultadoOperacionDTO ActualizarContrasena(ActualizacionContrasenaDTO solicitud)
        {
            try
            {
                var resultado = RecuperacionCuentaServicio.ActualizarContrasena(solicitud);
                if (resultado.OperacionExitosa)
                {
                    _logger.Info("Contraseña actualizada correctamente mediante recuperación.");
                }
                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Argumento nulo al actualizar contraseña. Los datos de actualización son nulos.", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.DatosActualizacionContrasena
                };
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error("Validación de entidad fallida al actualizar contraseña. La nueva contraseña no cumple con las restricciones.", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error de actualización de base de datos al actualizar contraseña. Conflicto al guardar la nueva contraseña.", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al actualizar contraseña. Fallo en la ejecución de la actualización.", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al actualizar contraseña. No se pudo procesar la actualización.", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
        }
    }
}