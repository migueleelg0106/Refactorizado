using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using System;
using System.Configuration;
using System.Data;
using System.Net.Mail;
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
            catch (ArgumentException ex)
            {
                Logger.Warn("Datos inválidos al solicitar código de recuperación", ex);
                return CrearResultadoSolicitudRecuperacionFallido(ex.Message);
            }
            catch (FormatException ex)
            {
                Logger.Warn("Formato inválido al solicitar código de recuperación", ex);
                return CrearResultadoSolicitudRecuperacionFallido(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Estado inválido al solicitar código de recuperación", ex);
                return CrearResultadoSolicitudRecuperacionFallido(ex.Message);
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al solicitar código de recuperación", ex);
                return CrearResultadoSolicitudRecuperacionFallido(ex.Message);
            }
            catch (SmtpException ex)
            {
                Logger.Error("Error de correo al solicitar código de recuperación", ex);
                return CrearResultadoSolicitudRecuperacionFallido(ex.Message);
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Error("Configuración inválida al solicitar código de recuperación", ex);
                return CrearResultadoSolicitudRecuperacionFallido(ex.Message);
            }
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenviarCodigoDTO solicitud)
        {
            try
            {
                return CodigoVerificacionServicio.ReenviarCodigoRecuperacion(solicitud);
            }
            catch (ArgumentException ex)
            {
                Logger.Warn("Datos inválidos al reenviar código de recuperación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (FormatException ex)
            {
                Logger.Warn("Formato inválido al reenviar código de recuperación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Estado inválido al reenviar código de recuperación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al reenviar código de recuperación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (SmtpException ex)
            {
                Logger.Error("Error de correo al reenviar código de recuperación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Error("Configuración inválida al reenviar código de recuperación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
        }

        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoDTO confirmacion)
        {
            try
            {
                return CodigoVerificacionServicio.ConfirmarCodigoRecuperacion(confirmacion);
            }
            catch (ArgumentException ex)
            {
                Logger.Warn("Datos inválidos al confirmar código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (FormatException ex)
            {
                Logger.Warn("Formato inválido al confirmar código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Estado inválido al confirmar código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al confirmar código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (SmtpException ex)
            {
                Logger.Error("Error de correo al confirmar código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Error("Configuración inválida al confirmar código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
        }

        public ResultadoOperacionDTO ActualizarContrasena(ActualizarContrasenaDTO solicitud)
        {
            try
            {
                return CodigoVerificacionServicio.ActualizarContrasena(solicitud);
            }
            catch (ArgumentException ex)
            {
                Logger.Warn("Datos inválidos al actualizar la contraseña", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (FormatException ex)
            {
                Logger.Warn("Formato inválido al actualizar la contraseña", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Estado inválido al actualizar la contraseña", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al actualizar la contraseña", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (SmtpException ex)
            {
                Logger.Error("Error de correo al actualizar la contraseña", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Error("Configuración inválida al actualizar la contraseña", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
        }

        private static ResultadoSolicitudRecuperacionDTO CrearResultadoSolicitudRecuperacionFallido(string mensaje)
        {
            return new ResultadoSolicitudRecuperacionDTO
            {
                CodigoEnviado = false,
                Mensaje = mensaje
            };
        }

        private static ResultadoSolicitudCodigoDTO CrearResultadoSolicitudCodigoFallido(string mensaje)
        {
            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = false,
                Mensaje = mensaje
            };
        }

        private static ResultadoOperacionDTO CrearResultadoOperacionFallida(string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = mensaje
            };
        }
    }
}
