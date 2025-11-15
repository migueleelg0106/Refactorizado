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
    public class CorreoCodigoVerificacionNotificador : ICodigoVerificacionNotificador
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(CorreoCodigoVerificacionNotificador));

        private const string AsuntoPredeterminado = "Codigo de verificacion";

        public async Task<bool> NotificarAsync(string correoDestino, string codigo, string usuarioDestino)
        {
            if (string.IsNullOrWhiteSpace(correoDestino) || string.IsNullOrWhiteSpace(codigo))
            {
                return false;
            }

            string remitente = ObtenerConfiguracion("CorreoRemitente", "Correo.Remitente.Direccion");
            string contrasena = ObtenerConfiguracion("CorreoPassword", "Correo.Smtp.Contrasena");
            string host = ObtenerConfiguracion("CorreoHost", "Correo.Smtp.Host");
            string usuarioSmtp = ObtenerConfiguracion("CorreoUsuario", "Correo.Smtp.Usuario");
            string puertoConfigurado = ObtenerConfiguracion("CorreoPuerto", "Correo.Smtp.Puerto");
            string asunto = ObtenerConfiguracion("CorreoAsunto", "Correo.Codigo.Asunto") ?? AsuntoPredeterminado;

            bool.TryParse(
                ObtenerConfiguracion("CorreoSsl", "Correo.Smtp.HabilitarSsl"),
                out bool habilitarSsl);

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

            string cuerpoHtml = ConstruirCuerpoMensaje(usuarioDestino, codigo);

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

                        await clienteSmtp
                            .SendMailAsync(mensajeCorreo)
                            .ConfigureAwait(false);
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

        private static string ConstruirCuerpoMensaje(string usuarioDestino, string codigo)
        {
            var cuerpoHtml = new StringBuilder();

            cuerpoHtml.Append("<html><body>");

            if (!string.IsNullOrWhiteSpace(usuarioDestino))
            {
                cuerpoHtml.Append($"<h2>Hola {usuarioDestino},</h2>");
            }
            else
            {
                cuerpoHtml.Append("<h2>Hola,</h2>");
            }

            cuerpoHtml.Append("<p>Tu codigo de verificacion es:</p>");
            cuerpoHtml.Append($"<h1>{codigo}</h1>");
            cuerpoHtml.Append("<p>Si no solicitaste este codigo puedes ignorar este mensaje.</p>");
            cuerpoHtml.Append("</body></html>");

            return cuerpoHtml.ToString();
        }
    }
}
