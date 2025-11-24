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
    /// Delega la logica de negocio a VerificacionRegistroServicio y RecuperacionCuentaServicio.
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
                var resultado = VerificacionRegistroServicio.SolicitarCodigo(nuevaCuenta);

                if (resultado.CodigoEnviado)
                {
                    _logger.InfoFormat("Código de verificación para registro solicitado exitosamente para '{0}'.", nuevaCuenta.Correo);
                }
                else
                {
                    _logger.WarnFormat("Solicitud de código para registro fallida. Correo: '{0}'. Motivo: {1}", nuevaCuenta.Correo, resultado.Mensaje);
                }

                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Argumento nulo al solicitar código de verificación. Los datos de la cuenta son nulos.", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosSolicitudVerificacionInvalidos
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al solicitar código de verificación. Fallo en la consulta de verificación.", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorSolicitudVerificacion
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al solicitar código de verificación. No se pudo procesar la solicitud.", ex);
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
                var resultado = VerificacionRegistroServicio.ReenviarCodigo(solicitud);

                if (resultado.CodigoEnviado)
                {
                    _logger.InfoFormat("Código de verificación para registro reenviado. Token sesión: {0}", solicitud.TokenCodigo);
                }
                else
                {
                    _logger.WarnFormat("Fallo al reenviar código de registro. Token sesión: {0}. Motivo: {1}", solicitud.TokenCodigo, resultado.Mensaje);
                }

                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Argumento nulo al reenviar código de verificación. Los datos de la solicitud son nulos.", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosReenvioCodigo
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al reenviar código de verificación. Fallo en la consulta de solicitud.", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigoVerificacion
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al reenviar código de verificación. No se pudo procesar la solicitud.", ex);
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
                var resultado = VerificacionRegistroServicio.ConfirmarCodigo(confirmacion);

                if (resultado.RegistroExitoso)
                {
                    _logger.InfoFormat("Código de verificación de registro confirmado correctamente. Token sesión: {0}", confirmacion.TokenCodigo);
                }
                else
                {
                    _logger.WarnFormat("Intento fallido de confirmación de código de registro. Token sesión: {0}. Motivo: {1}", confirmacion.TokenCodigo, resultado.Mensaje);
                }

                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Argumento nulo al confirmar código de verificación. Los datos de confirmación son nulos.", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.DatosConfirmacionInvalidos
                };
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error("Validación de entidad fallida al confirmar código de verificación. Datos inconsistentes.", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error de actualización de base de datos al confirmar código de verificación. Conflicto de concurrencia.", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al confirmar código de verificación. Fallo en la consulta de solicitud.", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al confirmar código de verificación. No se pudo procesar la confirmación.", ex);
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
                var resultado = RecuperacionCuentaServicio.SolicitarCodigoRecuperacion(solicitud);

                if (resultado.CodigoEnviado)
                {
                    _logger.InfoFormat("Solicitud de recuperación de cuenta iniciada para '{0}'.", solicitud.Identificador);
                }
                else
                {
                    _logger.WarnFormat("Solicitud de recuperación fallida para '{0}'. Motivo: {1}", solicitud.Identificador, resultado.Mensaje);
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
        /// Confirma el codigo de verificacion ingresado para recuperacion.
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
                    _logger.InfoFormat("Código de recuperación confirmado correctamente. Token sesión: {0}", confirmacion.TokenCodigo);
                }
                else
                {
                    _logger.WarnFormat("Intento fallido de confirmación de código de recuperación. Token sesión: {0}. Motivo: {1}", confirmacion.TokenCodigo, resultado.Mensaje);
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
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigoRecuperacion
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al confirmar código de recuperación. No se pudo confirmar el código.", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigoRecuperacion
                };
            }
        }
    }
}