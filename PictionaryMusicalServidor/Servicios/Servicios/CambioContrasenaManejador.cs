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
    /// Implementacion del servicio de cambio y recuperacion de contrasena.
    /// Delega las operaciones a servicios especializados y maneja excepciones.
    /// </summary>
    public class CambioContrasenaManejador : ICambioContrasenaManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CambioContrasenaManejador));

        /// <summary>
        /// Solicita el envio de un codigo para recuperar la cuenta.
        /// Valida el identificador y envia el codigo al correo asociado.
        /// </summary>
        /// <param name="solicitud">Datos del usuario que solicita recuperar su cuenta.</param>
        /// <returns>Resultado de la solicitud del codigo de recuperacion.</returns>
        /// <exception cref="ArgumentNullException">Se captura cuando los datos son nulos.</exception>
        /// <exception cref="EntityException">Se captura cuando hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
        /// <exception cref="InvalidOperationException">Se captura cuando hay operaciones invalidas.</exception>
        public ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud)
        {
            try
            {
                return ServicioRecuperacionCuenta.SolicitarCodigoRecuperacion(solicitud);
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
        /// Reenvia un codigo de recuperacion previamente solicitado.
        /// Valida el token y reenvia el codigo al correo asociado.
        /// </summary>
        /// <param name="solicitud">Solicitud con el token del codigo a reenviar.</param>
        /// <returns>Resultado del reenvio del codigo de recuperacion.</returns>
        /// <exception cref="ArgumentNullException">Se captura cuando los datos son nulos.</exception>
        /// <exception cref="EntityException">Se captura cuando hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
        public ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenvioCodigoDTO solicitud)
        {
            try
            {
                return ServicioRecuperacionCuenta.ReenviarCodigoRecuperacion(solicitud);
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
        /// Confirma un codigo de recuperacion ingresado por el usuario.
        /// Valida el codigo y el token para permitir el cambio de contrasena.
        /// </summary>
        /// <param name="confirmacion">Datos para confirmar el codigo de recuperacion.</param>
        /// <returns>Resultado de la confirmacion del codigo.</returns>
        /// <exception cref="ArgumentNullException">Se captura cuando los datos son nulos.</exception>
        /// <exception cref="EntityException">Se captura cuando hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
        /// <exception cref="InvalidOperationException">Se captura cuando hay operaciones invalidas.</exception>
        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion)
        {
            try
            {
                return ServicioRecuperacionCuenta.ConfirmarCodigoRecuperacion(confirmacion);
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
        /// Actualiza la contrasena de un usuario tras validar el codigo de recuperacion.
        /// Verifica el token, valida la nueva contrasena y la encripta antes de guardarla.
        /// </summary>
        /// <param name="solicitud">Datos con el token y la nueva contrasena.</param>
        /// <returns>Resultado de la actualizacion de la contrasena.</returns>
        /// <exception cref="ArgumentNullException">Se captura cuando los datos son nulos.</exception>
        /// <exception cref="DbEntityValidationException">Se captura cuando hay errores de validacion de entidades.</exception>
        /// <exception cref="DbUpdateException">Se captura cuando hay errores al actualizar la base de datos.</exception>
        /// <exception cref="EntityException">Se captura cuando hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
        /// <exception cref="InvalidOperationException">Se captura cuando hay operaciones invalidas.</exception>
        public ResultadoOperacionDTO ActualizarContrasena(ActualizacionContrasenaDTO solicitud)
        {
            try
            {
                return ServicioRecuperacionCuenta.ActualizarContrasena(solicitud);
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