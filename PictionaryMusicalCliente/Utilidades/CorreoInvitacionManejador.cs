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
            if (!ValidarParametrosEntrada(correoDestino, codigoPartida))
            {
                return false;
            }

            ConfiguracionCorreo configuracion = ObtenerConfiguracionCorreo();
            if (!configuracion.EsValida())
            {
                return false;
            }

            string cuerpoHtml = ConstruirCuerpoMensaje(codigoPartida);

            return await IntentarEnviarCorreoAsync(configuracion, correoDestino, cuerpoHtml).ConfigureAwait(false);
        }

        private static bool ValidarParametrosEntrada(string correoDestino, string codigoPartida)
        {
            return !string.IsNullOrWhiteSpace(correoDestino) && !string.IsNullOrWhiteSpace(codigoPartida);
        }

        private ConfiguracionCorreo ObtenerConfiguracionCorreo()
        {
            string remitente = ObtenerConfiguracion("CorreoRemitente", "Correo.Remitente.Direccion");
            string contrasena = ObtenerConfiguracion("CorreoPassword", "Correo.Smtp.Contrasena");
            string host = ObtenerConfiguracion("CorreoHost", "Correo.Smtp.Host");
            string usuarioSmtp = ObtenerConfiguracion("CorreoUsuario", "Correo.Smtp.Usuario");
            string puertoConfigurado = ObtenerConfiguracion("CorreoPuerto", "Correo.Smtp.Puerto");
            string asunto = ObtenerConfiguracion("CorreoAsunto", "Correo.Invitacion.Asunto") ?? AsuntoPredeterminado;
            
            bool.TryParse(ObtenerConfiguracion("CorreoSsl", "Correo.Smtp.HabilitarSsl"), out bool habilitarSsl);
            
            if (string.IsNullOrWhiteSpace(usuarioSmtp))
            {
                usuarioSmtp = remitente;
            }

            if (!int.TryParse(puertoConfigurado, out int puerto))
            {
                puerto = 587;
            }

            return new ConfiguracionCorreo
            {
                Remitente = remitente,
                Contrasena = contrasena,
                Host = host,
                UsuarioSmtp = usuarioSmtp,
                Puerto = puerto,
                HabilitarSsl = habilitarSsl,
                Asunto = asunto
            };
        }

        private static async Task<bool> IntentarEnviarCorreoAsync(ConfiguracionCorreo configuracion, string correoDestino, string cuerpoHtml)
        {
            try
            {
                using (var mensajeCorreo = new MailMessage(configuracion.Remitente, correoDestino, configuracion.Asunto, cuerpoHtml))
                {
                    mensajeCorreo.IsBodyHtml = true;

                    using (var clienteSmtp = new SmtpClient(configuracion.Host, configuracion.Puerto))
                    {
                        clienteSmtp.EnableSsl = configuracion.HabilitarSsl;

                        if (!string.IsNullOrWhiteSpace(configuracion.Contrasena))
                        {
                            clienteSmtp.Credentials = new NetworkCredential(configuracion.UsuarioSmtp, configuracion.Contrasena);
                        }

                        await clienteSmtp.SendMailAsync(mensajeCorreo).ConfigureAwait(false);
                    }
                }

                return true;
            }
            catch (SmtpFailedRecipientsException)
            {
                return false;
            }
            catch (SmtpException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (ObjectDisposedException)
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

        private class ConfiguracionCorreo
        {
            public string Remitente { get; set; }
            public string Contrasena { get; set; }
            public string Host { get; set; }
            public string UsuarioSmtp { get; set; }
            public int Puerto { get; set; }
            public bool HabilitarSsl { get; set; }
            public string Asunto { get; set; }

            public bool EsValida()
            {
                return !string.IsNullOrWhiteSpace(Remitente) && !string.IsNullOrWhiteSpace(Host);
            }
        }
    }
}
