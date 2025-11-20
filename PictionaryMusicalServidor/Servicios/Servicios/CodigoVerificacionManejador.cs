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
    /// Delega las operaciones de verificacion y recuperacion a servicios especializados y maneja excepciones.
    /// </summary>
    public class CodigoVerificacionManejador : ICodigoVerificacionManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CodigoVerificacionManejador));

        /// <summary>
        /// Solicita el envio de un codigo de verificacion para registrar una nueva cuenta.
        /// Delega la operacion al servicio de verificacion de registro.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la nueva cuenta a registrar.</param>
        /// <returns>Resultado de la solicitud del codigo de verificacion.</returns>
        /// <exception cref="ArgumentNullException">Se captura cuando los datos son nulos.</exception>
        /// <exception cref="EntityException">Se captura cuando hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            try
            {
                return ServicioVerificacionRegistro.SolicitarCodigo(nuevaCuenta);
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
        /// Reenvia un codigo de verificacion previamente solicitado.
        /// Delega la operacion al servicio de verificacion de registro.
        /// </summary>
        /// <param name="solicitud">Solicitud con el token del codigo a reenviar.</param>
        /// <returns>Resultado del reenvio del codigo de verificacion.</returns>
        /// <exception cref="ArgumentNullException">Se captura cuando los datos son nulos.</exception>
        /// <exception cref="EntityException">Se captura cuando hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenvioCodigoVerificacionDTO solicitud)
        {
            try
            {
                return ServicioVerificacionRegistro.ReenviarCodigo(solicitud);
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
        /// Confirma un codigo de verificacion ingresado por el usuario.
        /// Delega la operacion al servicio de verificacion de registro.
        /// </summary>
        /// <param name="confirmacion">Datos para confirmar el codigo de verificacion.</param>
        /// <returns>Resultado de la confirmacion del codigo.</returns>
        /// <exception cref="ArgumentNullException">Se captura cuando los datos son nulos.</exception>
        /// <exception cref="DbEntityValidationException">Se captura cuando hay errores de validacion de entidades.</exception>
        /// <exception cref="DbUpdateException">Se captura cuando hay errores al actualizar la base de datos.</exception>
        /// <exception cref="EntityException">Se captura cuando hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
        /// <exception cref="InvalidOperationException">Se captura cuando hay operaciones invalidas.</exception>
        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion)
        {
            try
            {
                return ServicioVerificacionRegistro.ConfirmarCodigo(confirmacion);
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
        /// Solicita el envio de un codigo para recuperar una cuenta.
        /// Delega la operacion al servicio de recuperacion de cuenta.
        /// </summary>
        /// <param name="solicitud">Datos del usuario que solicita recuperar su cuenta.</param>
        /// <returns>Resultado de la solicitud del codigo de recuperacion.</returns>
        /// <exception cref="ArgumentNullException">Se captura cuando los datos son nulos.</exception>
        /// <exception cref="EntityException">Se captura cuando hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se captura cuando hay errores relacionados con los datos.</exception>
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
        /// Confirma un codigo de recuperacion ingresado por el usuario.
        /// Delega la operacion al servicio de recuperacion de cuenta.
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