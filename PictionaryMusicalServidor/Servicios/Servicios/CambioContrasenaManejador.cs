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
    /// Implementacion del servicio de recuperacion y cambio de contrasena de usuarios.
    /// Maneja el proceso completo de recuperacion incluyendo solicitud, reenvio, confirmacion
    /// de codigos y actualizacion de contrasena.
    /// Delega la logica de negocio al RecuperacionCuentaServicio.
    /// </summary>
    public class CambioContrasenaManejador : ICambioContrasenaManejador
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(CambioContrasenaManejador));
        private readonly IRecuperacionCuentaServicio _recuperacionServicio;

        /// <summary>
        /// Constructor por defecto para WCF.
        /// Inicializa el servicio concreto con sus dependencias.
        /// </summary>
        public CambioContrasenaManejador() : this(
            new RecuperacionCuentaServicio(
                new ContextoFactoria(),
                new NotificacionCodigosServicio(new CorreoCodigoVerificacionNotificador())))
        {
        }

        /// <summary>
        /// Constructor con inyeccion de dependencias.
        /// </summary>
        /// <param name="recuperacionServicio">Servicio de logica de recuperacion.</param>
        public CambioContrasenaManejador(IRecuperacionCuentaServicio recuperacionServicio)
        {
            _recuperacionServicio = recuperacionServicio ??
                throw new ArgumentNullException(nameof(recuperacionServicio));
        }
        /// <summary>
        /// Solicita un codigo de verificacion para recuperar la cuenta.
        /// Busca la cuenta por identificador, genera un codigo y lo envia por correo.
        /// </summary>
        /// <param name="solicitud">Datos de la solicitud con identificador del usuario.</param>
        /// <returns>Resultado indicando si se encontro la cuenta y si el codigo fue enviado.
        /// </returns>
        public ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion
            (SolicitudRecuperarCuentaDTO solicitud)
        {
            try
            {
                var resultado = _recuperacionServicio.SolicitarCodigoRecuperacion(solicitud);
                if (!resultado.CodigoEnviado)
                {
                    _logger.WarnFormat("Solicitud de recuperacion fallida para '{0}': {1}",
                        solicitud.Identificador, 
                        resultado.Mensaje);
                }
                return resultado;
            }
            catch (ArgumentNullException excepcion)
            {
                _logger.Warn(
                    "Argumento nulo al solicitar codigo de recuperacion. Datos nulos.", excepcion);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosRecuperacionInvalidos
                };
            }
            catch (EntityException excepcion)
            {
                _logger.Error(
                    "Error de base de datos al solicitar codigo de recuperacion.", excepcion);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorRecuperarCuenta
                };
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error de datos al solicitar codigo de recuperacion.", excepcion);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorRecuperarCuenta
                };
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error de datos al solicitar codigo de recuperacion.", excepcion);
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
                var resultado = _recuperacionServicio.ReenviarCodigoRecuperacion(solicitud);
                if (!resultado.CodigoEnviado)
                {
                    _logger.WarnFormat(
                        "Solicitud de reenvio de codigo fallida para el token. Mensaje: {0}",
                        resultado.Mensaje);
                }
                return resultado;
            }
            catch (ArgumentNullException excepcion)
            {
                _logger.Warn("Argumento nulo al reenviar codigo de recuperacion.", excepcion);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosReenvioCodigo
                };
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error de base de datos al reenviar codigo de recuperacion.", excepcion);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigo
                };
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error de datos al reenviar codigo de recuperacion.", excepcion);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigo
                };
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error de datos al reenviar codigo de recuperacion.", excepcion);
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
        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion
            (ConfirmacionCodigoDTO confirmacion)
        {
            try
            {
                var resultado = _recuperacionServicio.ConfirmarCodigoRecuperacion(confirmacion); 
                if (!resultado.OperacionExitosa)
                {
                    _logger.WarnFormat(
                        "Intento fallido de confirmacion. Mensaje: {0}",
                        resultado.Mensaje);
                }
                return resultado;
            }
            catch (ArgumentNullException excepcion)
            {
                _logger.Warn("Argumento nulo al confirmar codigo de recuperacion.", excepcion);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.DatosConfirmacionInvalidos
                };
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error de base de datos al confirmar codigo de recuperacion.", excepcion);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error de datos al confirmar codigo de recuperacion.", excepcion);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorConfirmarCodigo
                };
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error de datos al confirmar codigo de recuperacion.", excepcion);

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
                var resultado = _recuperacionServicio.ActualizarContrasena(solicitud);
                if (!resultado.OperacionExitosa)
                {
                    _logger.Warn("No se pudo actualizar la contrasena mediante recuperacion.");
                }
                return resultado;
            }
            catch (ArgumentNullException excepcion)
            {
                _logger.Warn("Argumento nulo al actualizar contrasena.", excepcion);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.DatosActualizacionContrasena
                };
            }
            catch (DbEntityValidationException excepcion)
            {
                _logger.Error("Validacion de entidad fallida al actualizar contrasena.", excepcion);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (DbUpdateException excepcion)
            {
                _logger.Error(
                    "Error de actualizacion de base de datos al actualizar contrasena.", excepcion);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error de base de datos al actualizar contrasena.", excepcion);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error de datos al actualizar contrasena.", excepcion);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error de datos al actualizar contrasena.", excepcion);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
        }
    }
}
