using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using log4net;
using System;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Servicio interno para el envio de notificaciones de codigos de verificacion.
    /// Gestiona el envio de codigos por correo electronico a usuarios.
    /// </summary>
    internal static class NotificacionCodigosServicio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificacionCodigosServicio));
        private static ICodigoVerificacionNotificador _notificador = new CorreoCodigoVerificacionNotificador();

        /// <summary>
        /// Configura el notificador que se usara para enviar codigos de verificacion.
        /// Permite inyectar una implementacion personalizada del notificador.
        /// </summary>
        /// <param name="notificador">Instancia del notificador a usar, o null para usar el predeterminado.</param>
        public static void ConfigurarNotificador(ICodigoVerificacionNotificador notificador)
        {
            _notificador = notificador ?? new CorreoCodigoVerificacionNotificador();
        }

        /// <summary>
        /// Envia un codigo de verificacion por correo electronico a un usuario.
        /// Valida que el correo y codigo no esten vacios antes de enviar.
        /// </summary>
        /// <param name="correoDestino">Direccion de correo electronico del destinatario.</param>
        /// <param name="codigo">Codigo de verificacion a enviar.</param>
        /// <param name="usuarioDestino">Nombre del usuario destinatario.</param>
        /// <returns>True si el codigo fue enviado exitosamente, false en caso contrario.</returns>
        public static bool EnviarNotificacion(string correoDestino, string codigo, string usuarioDestino, string idioma)
        {
            if (string.IsNullOrWhiteSpace(correoDestino) || string.IsNullOrWhiteSpace(codigo))
            {
                return false;
            }

            try
            {

                var tarea = _notificador?.NotificarAsync(correoDestino, codigo, usuarioDestino, idioma);
                if (tarea == null)
                {
                    return false;
                }

                return tarea.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error crítico al enviar notificación a {0}.", correoDestino), ex);
                return false;
            }
        }
    }
}