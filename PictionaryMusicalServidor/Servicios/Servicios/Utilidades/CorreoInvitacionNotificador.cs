using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using log4net;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    internal static class CorreoInvitacionNotificador
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(CorreoInvitacionNotificador));
    
        private const string AsuntoPredeterminado = "Invitación a partida";

        public static bool EnviarInvitacion(string correoDestino, string codigoSala, string creador)
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
            string asunto = ObtenerConfiguracion("CorreoAsuntoInvitacion", "Correo.Invitacion.Asunto") ?? AsuntoPredeterminado;

            bool.TryParse(ObtenerConfiguracion("CorreoSsl", "Correo.Smtp.HabilitarSsl"), out bool habilitarSsl);

            if (string.IsNullOrWhiteSpace(remitente) || string.IsNullOrWhiteSpace(host))
            {
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

            string cuerpoHtml = ConstruirCuerpoMensaje(codigoSala, creador);

            try
            {
                using (var mensajeCorreo = new MailMessage(remitente, correoDestino, asunto, cuerpoHtml))
                {
                    mensajeCorreo.IsBodyHtml = true;

                    using (var clienteSmtp = new SmtpClient(host, puerto))
                    {
                        clienteSmtp.EnableSsl = habilitarSsl;

                        if (!string.IsNullOrWhiteSpace(contrasena))
                        {
                            clienteSmtp.Credentials = new NetworkCredential(usuarioSmtp, contrasena);
                        }

                        clienteSmtp.Send(mensajeCorreo);
                    }
                }

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

        private static string ConstruirCuerpoMensaje(string codigoSala, string creador)
        {
            var cuerpoHtml = new StringBuilder();
            cuerpoHtml.Append("<html><body>");
            cuerpoHtml.Append("<h2>¡Has sido invitado a una partida de Pictionary Musical!</h2>");

            if (!string.IsNullOrWhiteSpace(creador))
            {
                cuerpoHtml.Append($"<p>{creador} te ha invitado a su sala.</p>");
            }

            cuerpoHtml.Append("<p>Utiliza el siguiente código para unirte:</p>");
            cuerpoHtml.Append($"<h1 style='color:#4CAF50;'>{codigoSala}</h1>");
            cuerpoHtml.Append("<p>Ingresa el código en la aplicación para unirte a la partida.</p>");
            cuerpoHtml.Append("<p>¡Te esperamos!</p>");
            cuerpoHtml.Append("</body></html>");

            return cuerpoHtml.ToString();
        }
    }
}
