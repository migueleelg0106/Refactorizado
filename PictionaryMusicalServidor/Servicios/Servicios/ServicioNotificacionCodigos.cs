using Servicios.Servicios.Utilidades;
using System.Threading.Tasks;

namespace Servicios.Servicios
{
    /// <summary>
    /// Maneja la abstracción del envío de notificaciones de códigos (ej. email).
    /// Mantiene la instancia estática del notificador.
    /// </summary>
    internal static class ServicioNotificacionCodigos
    {
        private static ICodigoVerificacionNotificador _notificador = new CorreoCodigoVerificacionNotificador();

        public static void ConfigurarNotificador(ICodigoVerificacionNotificador notificador)
        {
            _notificador = notificador ?? new CorreoCodigoVerificacionNotificador();
        }

        /// <summary>
        /// Envía una notificación de código al destinatario.
        /// </summary>
        public static bool EnviarNotificacion(string correoDestino, string codigo, string usuarioDestino)
        {
            if (string.IsNullOrWhiteSpace(correoDestino) || string.IsNullOrWhiteSpace(codigo))
            {
                return false;
            }

            try
            {
                // El código original usaba GetAwaiter().GetResult(), 
                // mantenemos esa lógica síncrona sobre asíncrona.
                var tarea = _notificador?.NotificarAsincrono(correoDestino, codigo, usuarioDestino);
                if (tarea == null)
                {
                    return false;
                }

                return tarea.GetAwaiter().GetResult();
            }
            catch
            {
                return false;
            }
        }
    }
}