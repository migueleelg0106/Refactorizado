using PictionaryMusicalServidor.Servicios.Contratos;
using System;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion de codigos de verificacion.
    /// Maneja solicitud, reenvio y confirmacion de codigos para registro y recuperacion de cuentas.
    /// Delega la logica de negocio a ServicioVerificacionRegistro y ServicioRecuperacionCuenta.
    /// </summary>
    public class CodigoVerificacionManejador : ICodigoVerificacionManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CodigoVerificacionManejador));

        /// <summary>
        /// Solicita un codigo de verificacion para registrar una nueva cuenta.
        /// Valida datos, verifica disponibilidad de usuario y correo, genera codigo y lo envia por correo.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la nueva cuenta a verificar.</param>
        /// <returns>Resultado indicando si el codigo fue enviado y posibles conflictos.</returns>
        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            try
            {
                var resultado = ServicioVerificacionRegistro.SolicitarCodigo(nuevaCuenta);

                if (resultado.CodigoEnviado)
                {
                    _logger.Info($"Código de verificación para registro solicitado exitosamente para '{nuevaCuenta.Correo}'.");
                }
                else
                {
                    _logger.Warn($"Solicitud de código para registro fallida. Correo: '{nuevaCuenta.Correo}'. Motivo: {resultado.Mensaje}");
                }

                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn(MensajesError.Log.VerificacionSolicitarArgumentoNulo, ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosSolicitudVerificacionInvalidos
                };
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.VerificacionSolicitarErrorBD, ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorSolicitudVerificacion
                };
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.VerificacionSolicitarErrorDatos, ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorSolicitudVerificacion
                };
            }
        }

        /// <summary>
        /// Reenvia el codigo de verificacion para registro.
        /// Valida el token de sesion y reenvia el codigo por correo.
        /// </summary>
        /// <param name="solicitud">Datos con el token de la sesion de verificacion.</param>
        /// <returns>Resultado indicando si el codigo fue reenviado exitosamente.</returns>
        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenvioCodigoVerificacionDTO solicitud)
        {
            try
            {
                var resultado = ServicioVerificacionRegistro.ReenviarCodigo(solicitud);

                if (resultado.CodigoEnviado)
                {
                    _logger.Info($"Código de verificación para registro reenviado. Token sesión: {solicitud.TokenCodigo}");
                }
                else
                {
                    _logger.Warn($"Fallo al reenviar código de registro. Token sesión: {solicitud.TokenCodigo}. Motivo: {resultado.Mensaje}");
                }

                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn(MensajesError.Log.VerificacionReenviarArgumentoNulo, ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosReenvioCodigo
                };
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.VerificacionReenviarErrorBD, ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigoVerificacion
                };
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.VerificacionReenviarErrorDatos, ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigoVerificacion
                };
            }
        }

        /// <summary>
        /// Confirma el codigo de verificacion ingresado para registro.
        /// Valida el token de sesion y el codigo, y permite continuar con el registro de la cuenta.
        /// </summary>
        /// <param name="confirmacion">Datos con el token y codigo ingresado.</param>
        /// <returns>Resultado indicando si el codigo fue confirmado exitosamente.</returns>
        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion)
        {
            try
            {
                var resultado = ServicioVerificacionRegistro.ConfirmarCodigo(confirmacion);

                if (resultado.RegistroExitoso)
                {
                    _logger.Info($"Código de verificación de registro confirmado correctamente. Token sesión: {confirmacion.TokenCodigo}");
                }
                else
                {
                    _logger.Warn($"Intento fallido de confirmación de código de registro. Token sesión: {confirmacion.TokenCodigo}. Motivo: {resultado.Mensaje}");
                }

                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn(MensajesError.Log.VerificacionConfirmarArgumentoNulo, ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.DatosConfirmacionInvalidos
                };
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error(MensajesError.Log.VerificacionConfirmarValidacionEntidad, ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.Error(MensajesError.Log.VerificacionConfirmarActualizacionBD, ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.VerificacionConfirmarErrorBD, ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.VerificacionConfirmarErrorDatos, ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
        }

        /// <summary>
        /// Solicita un codigo de verificacion para recuperar una cuenta.
        /// Busca la cuenta por identificador, genera codigo y lo envia por correo.
        /// </summary>
        /// <param name="solicitud">Datos con identificador del usuario que solicita recuperacion.</param>
        /// <returns>Resultado indicando si la cuenta fue encontrada y si el codigo fue enviado.</returns>
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
                    _logger.Warn($"Solicitud de recuperación fallida para '{solicitud.Identificador}'. Motivo: {resultado.Mensaje}");
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
        /// Confirma el codigo de verificacion ingresado para recuperacion.
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
                    _logger.Info($"Código de recuperación confirmado correctamente. Token sesión: {confirmacion.TokenCodigo}");
                }
                else
                {
                    _logger.Warn($"Intento fallido de confirmación de código de recuperación. Token sesión: {confirmacion.TokenCodigo}. Motivo: {resultado.Mensaje}");
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
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigoRecuperacion
                };
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionConfirmarErrorDatos, ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigoRecuperacion
                };
            }
        }
    }
}