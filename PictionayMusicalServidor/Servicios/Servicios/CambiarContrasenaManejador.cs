using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using System;
using log4net;

namespace Servicios.Servicios
{
    public class CambiarContrasenaManejador : ICambiarContrasenaManejador
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CambiarContrasenaManejador));

        public ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud)
        {
            try
            {
                return CodigoVerificacionServicio.SolicitarCodigoRecuperacion(solicitud);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al solicitar código de recuperación", ex);
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CodigoEnviado = false,
                    Mensaje = ex.Message
                };
            }
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenviarCodigoDTO solicitud)
        {
            try
            {
                return CodigoVerificacionServicio.ReenviarCodigoRecuperacion(solicitud);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al reenviar código de recuperación", ex);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = ex.Message
                };
            }
        }

        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoDTO confirmacion)
        {
            try
            {
                return CodigoVerificacionServicio.ConfirmarCodigoRecuperacion(confirmacion);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al confirmar código de recuperación", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = ex.Message
                };
            }
        }

        public ResultadoOperacionDTO ActualizarContrasena(ActualizarContrasenaDTO solicitud)
        {
            try
            {
                return CodigoVerificacionServicio.ActualizarContrasena(solicitud);
            }
            catch (Exception ex)
            {
                Logger.Error("Error al actualizar la contraseña", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = ex.Message
                };
            }
        }
    }
}
