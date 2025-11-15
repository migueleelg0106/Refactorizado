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
    public class CodigoVerificacionManejador : ICodigoVerificacionManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CodigoVerificacionManejador));

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