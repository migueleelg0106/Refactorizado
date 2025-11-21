using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using log4net;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Notificador que envía invitaciones a partidas por correo electrónico.
    /// Refactorizado para seguir el patrón de CorreoCodigoVerificacionNotificador.
    /// </summary>
    internal static class CorreoInvitacionNotificador
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(CorreoInvitacionNotificador));

        private const string AsuntoPredeterminadoEs = "Invitación a partida de Pictionary Musical";
        private const string AsuntoPredeterminadoEn = "Pictionary Musical Game Invitation";

        /// <summary>
        /// Envía una invitación a una partida al correo indicado de forma asíncrona.
        /// </summary>
        public static async Task<bool> EnviarInvitacionAsync(string correoDestino, string codigoSala, string creador, string idioma)
        {
            if (string.IsNullOrWhiteSpace(correoDestino) || string.IsNullOrWhiteSpace(codigoSala))
            {
                return false;
            }

            string remitente = ObtenerConfiguracion("CorreoRemitente", "Correo.Remitente.Direccion");
            string contrasena = ObtenerConfiguracion("CorreoPassword", "Correo.Smtp.Contrasena");
            string host = ObtenerConfiguracion("CorreoHost", "Correo.Smtp.Host");
            string usuarioSmtp = ObtenerConfiguracion("CorreoUsuario", "Correo.Smtp.Usuario");
            string puertoConfigurado = ObtenerConfiguracion("CorreoPuerto", "Correo.Smtp.Puerto");

            string idiomaNormalizado = NormalizarIdioma(idioma);

            string asuntoConfigurado = ObtenerConfiguracion(
                $"CorreoAsuntoInvitacion.{idiomaNormalizado}",
                $"Correo.Invitacion.Asunto.{idiomaNormalizado}",
                "CorreoAsuntoInvitacion",
                "Correo.Invitacion.Asunto");

            string asunto = string.IsNullOrWhiteSpace(asuntoConfigurado)
                ? ObtenerAsuntoPredeterminado(idiomaNormalizado)
                : asuntoConfigurado;

            bool.TryParse(
                ObtenerConfiguracion("CorreoSsl", "Correo.Smtp.HabilitarSsl"),
                out bool habilitarSsl);

            if (string.IsNullOrWhiteSpace(remitente) || string.IsNullOrWhiteSpace(host))
            {
                _logger.Error("Configuración de correo incompleta (Remitente o Host faltante).");
                return false;
            }

            if (string.IsNullOrWhiteSpace(usuarioSmtp))
            {
                usuarioSmtp = remitente;
            }

            if (!int.TryParse(puertoConfigurado, out int puerto))
            {
                puerto = 587;
            }

            if (!habilitarSsl)
            {
                _logger.Error("Configuración inválida: Correo.Smtp.HabilitarSsl debe ser true.");
                return false;
            }

            string cuerpoHtml = ConstruirCuerpoMensaje(codigoSala, creador, idiomaNormalizado);

            try
            {
                using (var mensaje = new MailMessage(remitente, correoDestino, asunto, cuerpoHtml))
                {
                    mensaje.IsBodyHtml = true;
                    mensaje.BodyEncoding = Encoding.UTF8;
                    mensaje.SubjectEncoding = Encoding.UTF8;

                    using (var clienteSmtp = new SmtpClient(host, puerto))
                    {
                        clienteSmtp.EnableSsl = true;

                        if (!string.IsNullOrWhiteSpace(contrasena))
                        {
                            clienteSmtp.Credentials = new NetworkCredential(usuarioSmtp, contrasena);
                        }

                        await clienteSmtp
                            .SendMailAsync(mensaje)
                            .ConfigureAwait(false);
                    }
                }

                _logger.Info($"Invitación enviada correctamente a '{correoDestino}'.");
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.Error(MensajesError.Log.CorreoSmtp, ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.CorreoOperacionInvalida, ex);
                return false;
            }
            catch (ArgumentException ex)
            {
                _logger.Error(MensajesError.Log.CorreoArgumentoInvalido, ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error inesperado al enviar invitación a {correoDestino}", ex);
                return false;
            }
        }

        internal static string ConstruirCuerpoMensaje(string codigoSala, string creador, string idioma)
        {
            string idiomaNormalizado = NormalizarIdioma(idioma);
            bool esIngles = idiomaNormalizado == "en";

            string saludo = esIngles
                ? "Hello!"
                : "¡Hola!";

            string mensajeBienvenida = esIngles
                ? "You have been invited to a Musical Pictionary game."
                : "Has sido invitado a una partida de Pictionary Musical.";

            string mensajeInvitacion = esIngles
                ? $"{creador} has invited you to their room."
                : $"{creador} te ha invitado a su sala.";

            string mensajeInstruccion = esIngles
                ? "Use the following code to join:"
                : "Utiliza el siguiente código para unirte:";

            string mensajeDespedida = esIngles
                ? "See you in the game!"
                : "¡Nos vemos en el juego!";

            var cuerpoHtml = new StringBuilder();

            cuerpoHtml.Append("<html><body style='font-family: Arial, sans-serif;'>");
            cuerpoHtml.Append($"<h2>{saludo}</h2>");
            cuerpoHtml.Append($"<p>{mensajeBienvenida}</p>");
            cuerpoHtml.Append($"<p>{mensajeInvitacion}</p>");
            cuerpoHtml.Append($"<p>{mensajeInstruccion}</p>");
            cuerpoHtml.Append($"<h1 style='color:#4CAF50; letter-spacing: 2px;'>{codigoSala}</h1>");
            cuerpoHtml.Append($"<p>{mensajeDespedida}</p>");
            cuerpoHtml.Append("</body></html>");

            return cuerpoHtml.ToString();
        }

        private static string NormalizarIdioma(string idioma)
        {
            if (string.IsNullOrWhiteSpace(idioma))
            {
                return "es";
            }

            return idioma.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "es";
        }

        private static string ObtenerAsuntoPredeterminado(string idiomaNormalizado)
        {
            return idiomaNormalizado == "en" ? AsuntoPredeterminadoEn : AsuntoPredeterminadoEs;
        }

        private static string ObtenerConfiguracion(params string[] claves)
        {
            if (claves == null)
            {
                return null;
            }

            foreach (string clave in claves)
            {
                if (string.IsNullOrWhiteSpace(clave))
                {
                    continue;
                }

                string valor = ConfigurationManager.AppSettings[clave];

                if (!string.IsNullOrWhiteSpace(valor))
                {
                    return valor;
                }
            }

            return null;
        }
    }
}