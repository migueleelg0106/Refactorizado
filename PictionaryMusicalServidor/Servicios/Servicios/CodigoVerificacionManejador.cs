using Servicios.Contratos;
using System;
using log4net;
using Servicios.Contratos.DTOs;
using Servicios.Servicios.Utilidades;

namespace Servicios.Servicios
{
    public class CodigoVerificacionManejador : ICodigoVerificacionManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CodigoVerificacionManejador));

        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            try
            {
                return CodigoVerificacionServicio.SolicitarCodigo(nuevaCuenta);
            }
            catch (Exception ex)
            {
                _logger.Error("Error al solicitar código de verificación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = ex.Message
                };
            }
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenvioCodigoVerificacionDTO solicitud)
        {
            try
            {
                return CodigoVerificacionServicio.ReenviarCodigo(solicitud);
            }
            catch (Exception ex)
            {
                _logger.Error("Error al reenviar código de verificación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = ex.Message
                };
            }
        }

        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion)
        {
            try
            {
                return CodigoVerificacionServicio.ConfirmarCodigo(confirmacion);
            }
            catch (Exception ex)
            {
                _logger.Error("Error al confirmar el código de verificación", ex);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = ex.Message
                };
            }
        }

        public ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud)
        {
            try
            {
                return CodigoVerificacionServicio.SolicitarCodigoRecuperacion(solicitud);
            }
            catch (Exception ex)
            {
                _logger.Error("Error al solicitar código de recuperación", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = ex.Message
                };
            }
        }

        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion)
        {
            try
            {
                return CodigoVerificacionServicio.ConfirmarCodigoRecuperacion(confirmacion);
            }
            catch (Exception ex)
            {
                _logger.Error("Error al confirmar el código de recuperación", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = ex.Message
                };
            }
        }
    }
}
