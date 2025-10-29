using Servicios.Contratos;
using System;
using log4net;
using Servicios.Contratos.DTOs;
// Removimos "Servicios.Servicios.Utilidades"
// Agregamos el using a los nuevos servicios
using Servicios.Servicios;

namespace Servicios.Servicios
{
    public class CodigoVerificacionManejador : ICodigoVerificacionManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CodigoVerificacionManejador));

        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            try
            {
                // DELEGA A: ServicioVerificacionRegistro
                return ServicioVerificacionRegistro.SolicitarCodigo(nuevaCuenta);
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
                // DELEGA A: ServicioVerificacionRegistro
                return ServicioVerificacionRegistro.ReenviarCodigo(solicitud);
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
                // DELEGA A: ServicioVerificacionRegistro
                return ServicioVerificacionRegistro.ConfirmarCodigo(confirmacion);
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

        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion)
        {
            try
            {
                // DELEGA A: ServicioRecuperacionCuenta
                return ServicioRecuperacionCuenta.ConfirmarCodigoRecuperacion(confirmacion);
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