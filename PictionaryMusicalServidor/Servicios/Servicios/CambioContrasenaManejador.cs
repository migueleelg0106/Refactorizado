using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using System;
using log4net;
// Agregamos el using a los nuevos servicios
using Servicios.Servicios;

namespace Servicios.Servicios
{
    public class CambioContrasenaManejador : ICambioContrasenaManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CambioContrasenaManejador));

        public ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud)
        {
            try
            {
                // DELEGA A: ServicioRecuperacionCuenta
                return ServicioRecuperacionCuenta.SolicitarCodigoRecuperacion(solicitud);
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

        public ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenvioCodigoDTO solicitud)
        {
            try
            {
                // DELEGA A: ServicioRecuperacionCuenta
                return ServicioRecuperacionCuenta.ReenviarCodigoRecuperacion(solicitud);
            }
            catch (Exception ex)
            {
                _logger.Error("Error al reenviar código de recuperación", ex);
                return new ResultadoSolicitudCodigoDTO
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
                // DELEGA A: ServicioRecuperacionCuenta
                return ServicioRecuperacionCuenta.ConfirmarCodigoRecuperacion(confirmacion);
            }
            catch (Exception ex)
            {
                _logger.Error("Error al confirmar código de recuperación", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = ex.Message
                };
            }
        }


        public ResultadoOperacionDTO ActualizarContrasena(ActualizacionContrasenaDTO solicitud)
        {
            try
            {
                // DELEGA A: ServicioRecuperacionCuenta
                return ServicioRecuperacionCuenta.ActualizarContrasena(solicitud);
            }
            catch (Exception ex)
            {
                _logger.Error("Error al actualizar la contraseña", ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = ex.Message
                };
            }
        }
    }
}