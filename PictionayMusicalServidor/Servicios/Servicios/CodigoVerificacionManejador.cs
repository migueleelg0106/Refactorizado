using Servicios.Contratos;
using System;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Net.Mail;
using log4net;
using Servicios.Contratos.DTOs;
using Servicios.Servicios.Utilidades;

namespace Servicios.Servicios
{
    public class CodigoVerificacionManejador : ICodigoVerificacionManejador
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CodigoVerificacionManejador));

        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            try
            {
                return CodigoVerificacionServicio.SolicitarCodigo(nuevaCuenta);
            }
            catch (ArgumentException ex)
            {
                Logger.Warn("Datos inválidos al solicitar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (FormatException ex)
            {
                Logger.Warn("Formato inválido al solicitar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Estado inválido al solicitar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al solicitar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (EntityException ex)
            {
                Logger.Error("Error de entidad al solicitar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                Logger.Error("Error al actualizar la base de datos al solicitar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (SmtpException ex)
            {
                Logger.Error("Error de correo al solicitar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Error("Configuración inválida al solicitar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenviarCodigoVerificacionDTO solicitud)
        {
            try
            {
                return CodigoVerificacionServicio.ReenviarCodigo(solicitud);
            }
            catch (ArgumentException ex)
            {
                Logger.Warn("Datos inválidos al reenviar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (FormatException ex)
            {
                Logger.Warn("Formato inválido al reenviar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Estado inválido al reenviar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al reenviar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (EntityException ex)
            {
                Logger.Error("Error de entidad al reenviar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                Logger.Error("Error al actualizar la base de datos al reenviar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (SmtpException ex)
            {
                Logger.Error("Error de correo al reenviar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Error("Configuración inválida al reenviar código de verificación", ex);
                return CrearResultadoSolicitudCodigoFallido(ex.Message);
            }
        }

        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmarCodigoDTO confirmacion)
        {
            try
            {
                return CodigoVerificacionServicio.ConfirmarCodigo(confirmacion);
            }
            catch (ArgumentException ex)
            {
                Logger.Warn("Datos inválidos al confirmar el código de verificación", ex);
                return CrearResultadoRegistroFallido(ex.Message);
            }
            catch (FormatException ex)
            {
                Logger.Warn("Formato inválido al confirmar el código de verificación", ex);
                return CrearResultadoRegistroFallido(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Estado inválido al confirmar el código de verificación", ex);
                return CrearResultadoRegistroFallido(ex.Message);
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al confirmar el código de verificación", ex);
                return CrearResultadoRegistroFallido(ex.Message);
            }
            catch (EntityException ex)
            {
                Logger.Error("Error de entidad al confirmar el código de verificación", ex);
                return CrearResultadoRegistroFallido(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                Logger.Error("Error al actualizar la base de datos al confirmar el código de verificación", ex);
                return CrearResultadoRegistroFallido(ex.Message);
            }
            catch (SmtpException ex)
            {
                Logger.Error("Error de correo al confirmar el código de verificación", ex);
                return CrearResultadoRegistroFallido(ex.Message);
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Error("Configuración inválida al confirmar el código de verificación", ex);
                return CrearResultadoRegistroFallido(ex.Message);
            }
        }

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
            catch (EntityException ex)
            {
                Logger.Error("Error de entidad al solicitar código de recuperación", ex);
                return CrearResultadoSolicitudRecuperacionFallido(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                Logger.Error("Error al actualizar la base de datos al solicitar código de recuperación", ex);
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

        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoDTO confirmacion)
        {
            try
            {
                return CodigoVerificacionServicio.ConfirmarCodigoRecuperacion(confirmacion);
            }
            catch (ArgumentException ex)
            {
                Logger.Warn("Datos inválidos al confirmar el código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (FormatException ex)
            {
                Logger.Warn("Formato inválido al confirmar el código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("Estado inválido al confirmar el código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (DataException ex)
            {
                Logger.Error("Error de datos al confirmar el código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (EntityException ex)
            {
                Logger.Error("Error de entidad al confirmar el código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                Logger.Error("Error al actualizar la base de datos al confirmar el código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (SmtpException ex)
            {
                Logger.Error("Error de correo al confirmar el código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Error("Configuración inválida al confirmar el código de recuperación", ex);
                return CrearResultadoOperacionFallida(ex.Message);
            }
        }

        private static ResultadoSolicitudCodigoDTO CrearResultadoSolicitudCodigoFallido(string mensaje)
        {
            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = false,
                Mensaje = mensaje
            };
        }

        private static ResultadoRegistroCuentaDTO CrearResultadoRegistroFallido(string mensaje)
        {
            return new ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = false,
                Mensaje = mensaje
            };
        }

        private static ResultadoSolicitudRecuperacionDTO CrearResultadoSolicitudRecuperacionFallido(string mensaje)
        {
            return new ResultadoSolicitudRecuperacionDTO
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
