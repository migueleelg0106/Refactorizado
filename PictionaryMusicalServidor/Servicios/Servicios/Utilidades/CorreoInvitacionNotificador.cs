using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using log4net;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Notificador que envia invitaciones a partidas por correo electronico.
    /// </summary>
    internal static class CorreoInvitacionNotificador
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(CorreoInvitacionNotificador));

        private const string AsuntoPredeterminadoEs = "Invitacion a partida";
        private const string AsuntoPredeterminadoEn = "Game invitation";

        /// <summary>
        /// Envia una invitacion a una partida al correo indicado.
        /// </summary>
        /// <param name="correoDestino">Correo electronico de destino.</param>
        /// <param name="codigoSala">Codigo de la sala.</param>
        /// <param name="creador">Nombre del creador de la sala.</param>
        /// <returns>
        /// True si el correo se envio correctamente; false en caso contrario.
        /// </returns>
        public static bool EnviarInvitacion(string correoDestino, string codigoSala, string creador, string idioma)
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
            string asuntoConfigurado = ObtenerConfiguracion("CorreoAsuntoInvitacion", "Correo.Invitacion.Asunto");
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
                _logger.Error("Configuracion invalida: Correo.Smtp.HabilitarSsl debe ser true.");
                return false;
            }

            string cuerpoHtml = ConstruirCuerpoMensaje(codigoSala, creador, idiomaNormalizado);

            try
            {
                using (var mensajeCorreo = new MailMessage(remitente, correoDestino, asunto, cuerpoHtml))
                {
                    mensajeCorreo.IsBodyHtml = true;

                    using (var clienteSmtp = new SmtpClient(host, puerto))
                    {
                        clienteSmtp.EnableSsl = true;

                        if (!string.IsNullOrWhiteSpace(contrasena))
                        {
                            clienteSmtp.Credentials = new NetworkCredential(usuarioSmtp, contrasena);
                        }

                        clienteSmtp.Send(mensajeCorreo);
                    }
                }

                _logger.Info($"Invitación enviada correctamente a '{correoDestino}' para la sala {codigoSala}.");
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

        internal static string ConstruirCuerpoMensaje(string codigoSala, string creador, string idioma)
        {
            string idiomaNormalizado = NormalizarIdioma(idioma);
            bool esIngles = idiomaNormalizado == "en";

            string encabezado = esIngles
                ? "You have been invited to a Musical Pictionary game."
                : "Has sido invitado a una partida de Pictionary Musical.";
            string mensajeCreador = esIngles
                ? "has invited you to their room."
                : "te ha invitado a su sala.";
            string mensajeCodigo = esIngles
                ? "Use the following code to join:"
                : "Utiliza el siguiente código para unirte:";
            string mensajeInstruccion = esIngles
                ? "Enter the code in the app to join the game."
                : "Ingresa el código en la aplicación para unirte a la partida.";
            string mensajeDespedida = esIngles ? "We hope to see you there!" : "¡Te esperamos!";

            var cuerpoHtml = new StringBuilder();

            cuerpoHtml.Append("<html><body>");
            cuerpoHtml.Append($"<h2>{encabezado}</h2>");

            if (!string.IsNullOrWhiteSpace(creador))
            {
                cuerpoHtml.Append($"<p>{creador} {mensajeCreador}</p>");
            }

            cuerpoHtml.Append($"<p>{mensajeCodigo}</p>");
            cuerpoHtml.Append($"<h1 style='color:#4CAF50;'>{codigoSala}</h1>");
            cuerpoHtml.Append($"<p>{mensajeInstruccion}</p>");
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
    }
}