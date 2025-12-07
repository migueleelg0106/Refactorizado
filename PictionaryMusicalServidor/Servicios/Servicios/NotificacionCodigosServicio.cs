using PictionaryMusicalServidor.Servicios.Servicios.Notificadores;
using log4net;
using System;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Servicio interno para el envio de notificaciones de codigos de verificacion.
    /// Gestiona el envio de codigos por correo electronico a usuarios.
    /// </summary>
    public class NotificacionCodigosServicio : INotificacionCodigosServicio
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(NotificacionCodigosServicio));

        private readonly ICodigoVerificacionNotificador _notificador;

        /// <summary>
        /// Constructor por defecto para WCF.
        /// </summary>
        public NotificacionCodigosServicio() : this(new CorreoCodigoVerificacionNotificador())
        {
        }

        /// <summary>
        /// Constructor con inyeccion de dependencias.
        /// </summary>
        /// <param name="notificador">Instancia del notificador a usar.</param>
        public NotificacionCodigosServicio(ICodigoVerificacionNotificador notificador)
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
        /// <param name="idioma">Idioma para el correo.</param>
        /// <returns>True si el codigo fue enviado exitosamente, false en caso contrario.</returns>
        public bool EnviarNotificacion(
            string correoDestino,
            string codigo,
            string usuarioDestino,
            string idioma)
        {
            if (string.IsNullOrWhiteSpace(correoDestino) || string.IsNullOrWhiteSpace(codigo))
            {
                return false;
            }

            try
            {
                var tarea = _notificador?.NotificarAsync(
                    correoDestino,
                    codigo,
                    usuarioDestino,
                    idioma);

                if (tarea == null)
                {
                    return false;
                }

                return tarea.GetAwaiter().GetResult();
            }
            catch (AggregateException ex)
            {
                _logger.Error("Error critico al enviar notificacion de codigo.", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Error critico al enviar notificacion de codigo.", ex);
                return false;
            }
        }
    }
}