using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion de codigos de verificacion.
    /// Maneja solicitud, reenvio y confirmacion de codigos para registro y recuperacion de 
    /// cuentas.
    /// Delega la logica de negocio a los servicios correspondientes inyectados.
    /// </summary>
    public class CodigoVerificacionManejador : ICodigoVerificacionManejador
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(CodigoVerificacionManejador));

        private readonly IVerificacionRegistroServicio _verificacionRegistroServicio;
        private readonly IRecuperacionCuentaServicio _recuperacionCuentaServicio;

        /// <summary>
        /// Constructor por defecto para WCF.
        /// Inicializa los servicios con sus dependencias por defecto.
        /// </summary>
        public CodigoVerificacionManejador() : this(
            new VerificacionRegistroServicio(
                new ContextoFactoria(),
                new NotificacionCodigosServicio(new CorreoCodigoVerificacionNotificador())),
            new RecuperacionCuentaServicio(
                new ContextoFactoria(),
                new NotificacionCodigosServicio(new CorreoCodigoVerificacionNotificador())))
        {
        }

        /// <summary>
        /// Constructor con inyeccion de dependencias.
        /// </summary>
        public CodigoVerificacionManejador(
            IVerificacionRegistroServicio verificacionRegistroServicio,
            IRecuperacionCuentaServicio recuperacionCuentaServicio)
        {
            _verificacionRegistroServicio = verificacionRegistroServicio ??
                throw new ArgumentNullException(nameof(verificacionRegistroServicio));

            _recuperacionCuentaServicio = recuperacionCuentaServicio ??
                throw new ArgumentNullException(nameof(recuperacionCuentaServicio));
        }

        /// <summary>
        /// Solicita un codigo de verificacion para registrar una nueva cuenta.
        /// Valida datos, verifica disponibilidad, genera codigo y lo envia por correo.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la nueva cuenta a verificar.</param>
        /// <returns>Resultado indicando si el codigo fue enviado y posibles conflictos.</returns>
        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            try
            {
                var resultado = _verificacionRegistroServicio.SolicitarCodigo(nuevaCuenta);

                if (!resultado.CodigoEnviado)
                {
                    _logger.WarnFormat(
                        "Solicitud de codigo fallida. Motivo: {0}",
                        resultado.Mensaje);
                }

                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Argumento nulo al solicitar codigo de verificacion.", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosSolicitudVerificacionInvalidos
                };
            }
            catch (EntityException ex)
            {
                _logger.Error(
                    "Error de base de datos al solicitar codigo de verificacion.",
                    ex);

                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorSolicitudVerificacion
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al solicitar codigo de verificacion.", ex);
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
        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(
            ReenvioCodigoVerificacionDTO solicitud)
        {
            try
            {
                var resultado = _verificacionRegistroServicio.ReenviarCodigo(solicitud);

                if (!resultado.CodigoEnviado)
                {
                    _logger.WarnFormat(
                        "Fallo al reenviar codigo de registro. Motivo: {0}",
                        resultado.Mensaje);
                }

                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Argumento nulo al reenviar codigo de verificacion.", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosReenvioCodigo
                };
            }
            catch (EntityException ex)
            {
                _logger.Error(
                    "Error de base de datos al reenviar codigo de verificacion.",
                    ex);

                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigoVerificacion
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al reenviar codigo de verificacion.", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigoVerificacion
                };
            }
        }

        /// <summary>
        /// Confirma el codigo de verificacion ingresado para registro.
        /// Valida el token de sesion y el codigo, y permite continuar con el registro de la 
        /// cuenta.
        /// </summary>
        /// <param name="confirmacion">Datos con el token y codigo ingresado.</param>
        /// <returns>Resultado indicando si el codigo fue confirmado exitosamente.</returns>
        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(
            ConfirmacionCodigoDTO confirmacion)
        {
            try
            {
                var resultado = _verificacionRegistroServicio.ConfirmarCodigo(confirmacion);

                if (!resultado.RegistroExitoso)
                {
                    _logger.WarnFormat(
                        "Intento fallido de confirmacion de codigo. Motivo: {0}",
                        resultado.Mensaje);
                }

                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Argumento nulo al confirmar codigo de verificacion.", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.DatosConfirmacionInvalidos
                };
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error("Validacion de entidad fallida al confirmar codigo.", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error de actualizacion de BD al confirmar codigo.", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al confirmar codigo.", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al confirmar codigo.", ex);
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
        /// <param name="solicitud">Datos con identificador del usuario que solicita.</param>
        /// <returns>Resultado indicando si la cuenta fue encontrada y codigo enviado.</returns>
        public ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(
            SolicitudRecuperarCuentaDTO solicitud)
        {
            try
            {
                var resultado = _recuperacionCuentaServicio.SolicitarCodigoRecuperacion(solicitud);

                if (!resultado.CodigoEnviado)
                {
                    _logger.WarnFormat(
                        "Solicitud de recuperacion fallida. Motivo: {0}",
                        resultado.Mensaje);
                }

                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Argumento nulo al solicitar codigo de recuperacion.", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosRecuperacionInvalidos
                };
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al solicitar codigo de recuperacion.", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorRecuperarCuenta
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al solicitar codigo de recuperacion.", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorRecuperarCuenta
                };
            }
        }

        /// <summary>
        /// Confirma el codigo de verificacion ingresado para recuperacion.
        /// Valida el token y el codigo, y permite continuar con el cambio de contrasena.
        /// </summary>
        /// <param name="confirmacion">Datos con el token y codigo ingresado.</param>
        /// <returns>Resultado indicando si el codigo fue confirmado exitosamente.</returns>
        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion
            (ConfirmacionCodigoDTO confirmacion)
        {
            try
            {
                var resultado = _recuperacionCuentaServicio.ConfirmarCodigoRecuperacion(
                    confirmacion);

                if (!resultado.OperacionExitosa)
                {
                    _logger.WarnFormat(
                        "Intento fallido de confirmacion de recuperacion. Motivo: {0}",
                        resultado.Mensaje);
                }

                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                _logger.Warn("Argumento nulo al confirmar codigo de recuperacion.", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.DatosConfirmacionInvalidos
                };
            }
            catch (EntityException ex)
            {
                _logger.Error(
                    "Error de base de datos al confirmar codigo de recuperacion.",
                    ex);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigoRecuperacion
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al confirmar codigo de recuperacion.", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigoRecuperacion
                };
            }
        }
    }
}