using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Utilidades
{
    public class CorreoInvitacionManejador
    {
        private const string AsuntoPredeterminado = "Invitación a Partida - Pictionary Musical";

        public async Task<bool> EnviarInvitacionAsync(string correoDestino, string codigoPartida)
        {
            if (string.IsNullOrWhiteSpace(correoDestino) || string.IsNullOrWhiteSpace(codigoPartida))
            {
                return false;
            }

            string remitente = ObtenerConfiguracion("CorreoRemitente", "Correo.Remitente.Direccion");
            string contrasena = ObtenerConfiguracion("CorreoPassword", "Correo.Smtp.Contrasena");
            string host = ObtenerConfiguracion("CorreoHost", "Correo.Smtp.Host");
            string usuarioSmtp = ObtenerConfiguracion("CorreoUsuario", "Correo.Smtp.Usuario");
            string puertoConfigurado = ObtenerConfiguracion("CorreoPuerto", "Correo.Smtp.Puerto");
            string asunto = ObtenerConfiguracion("CorreoAsunto", "Correo.Invitacion.Asunto") ?? AsuntoPredeterminado;

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

            string cuerpoHtml = ConstruirCuerpoMensaje(codigoPartida);

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

                        await clienteSmtp.SendMailAsync(mensajeCorreo).ConfigureAwait(false);
                    }
                }

                return true;
            }
            catch (Exception)
            {
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

        private static string ConstruirCuerpoMensaje(string codigoPartida)
        {
            var cuerpoHtml = new StringBuilder();
            cuerpoHtml.Append("<html><body>");
            cuerpoHtml.Append("<h2>¡Te han invitado a una partida de Pictionary Musical!</h2>");
            cuerpoHtml.Append("<p>Has recibido una invitación para unirte a una partida.</p>");
            cuerpoHtml.Append("<p>El código de la partida es:</p>");
            cuerpoHtml.Append($"<h1 style='color: #4CAF50;'>{codigoPartida}</h1>");
            cuerpoHtml.Append("<p>Ingresa este código en la aplicación para unirte a la partida.</p>");
            cuerpoHtml.Append("<p>¡Diviértete!</p>");
            cuerpoHtml.Append("</body></html>");

            return cuerpoHtml.ToString();
        }
    }
}
