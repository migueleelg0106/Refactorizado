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
    /// Delega la logica de negocio al ServicioRecuperacionCuenta.
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
                var resultado = ServicioRecuperacionCuenta.SolicitarCodigoRecuperacion(solicitud);
                if (resultado.CodigoEnviado)
                {
                    _logger.Info($"Solicitud de recuperación de cuenta iniciada para '{solicitud.Identificador}'.");
                }
                else
                {
                    _logger.Warn($"Solicitud de recuperación fallida para '{solicitud.Identificador}': {resultado.Mensaje}");
                }
                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn(MensajesError.Log.RecuperacionSolicitarArgumentoNulo, ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosRecuperacionInvalidos
                };
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionSolicitarErrorBD, ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorRecuperarCuenta
                };
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionSolicitarErrorDatos, ex);
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
                var resultado = ServicioRecuperacionCuenta.ReenviarCodigoRecuperacion(solicitud);
                if (resultado.CodigoEnviado)
                {
                    _logger.Info($"Reenviar código de recuperación de cuenta iniciada para '{solicitud.TokenCodigo}'.");
                }
                else
                {
                    _logger.Warn($"Solicitud de reenvío de código de recuperación fallida para '{solicitud.TokenCodigo}': {resultado.Mensaje}");
                }
                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn(MensajesError.Log.RecuperacionReenviarArgumentoNulo, ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosReenvioCodigo
                };
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionReenviarErrorBD, ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigo
                };
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionReenviarErrorDatos, ex);
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
                var resultado = ServicioRecuperacionCuenta.ConfirmarCodigoRecuperacion(confirmacion);
                if (resultado.OperacionExitosa)
                {
                    _logger.Info($"Código de recuperación confirmado correctamente. Token sesión: '{confirmacion.TokenCodigo}'.");
                }
                else
                {
                    _logger.Warn($"Intento fallido de confirmación de código de recuperación. Token sesión: '{confirmacion.TokenCodigo}': {resultado.Mensaje}");
                }
                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn(MensajesError.Log.RecuperacionConfirmarArgumentoNulo, ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.DatosConfirmacionInvalidos
                };
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionConfirmarErrorBD, ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionConfirmarErrorDatos, ex);
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
                var resultado = ServicioRecuperacionCuenta.ActualizarContrasena(solicitud);
                if (resultado.OperacionExitosa)
                {
                    _logger.Info("Contraseña actualizada correctamente mediante recuperación.");
                }
                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn(MensajesError.Log.RecuperacionActualizarArgumentoNulo, ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.DatosActualizacionContrasena
                };
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionActualizarValidacionEntidad, ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionActualizarActualizacionBD, ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionActualizarErrorBD, ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionActualizarErrorDatos, ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
        }
    }
}