using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System.Threading.Tasks;

namespace PictionaryMusicalServidor.Servicios.Servicios
{

    internal static class ServicioNotificacionCodigos
    {
        private static ICodigoVerificacionNotificador _notificador = new CorreoCodigoVerificacionNotificador();

        public static void ConfigurarNotificador(ICodigoVerificacionNotificador notificador)
        {
            _notificador = notificador ?? new CorreoCodigoVerificacionNotificador();
        }

        public static bool EnviarNotificacion(string correoDestino, string codigo, string usuarioDestino)
        {
            if (string.IsNullOrWhiteSpace(correoDestino) || string.IsNullOrWhiteSpace(codigo))
            {
                return false;
            }

            try
            {
  
                var tarea = _notificador?.NotificarAsync(correoDestino, codigo, usuarioDestino);
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