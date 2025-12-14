using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace PictionaryMusicalServidor.Servicios.Servicios.Notificadores
{
    /// <summary>
    /// Implementacion del notificador que envia invitaciones a partidas por correo electronico.
    /// </summary>
    public class CorreoInvitacionNotificador : ICorreoInvitacionNotificador
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(CorreoInvitacionNotificador));

        private const string AsuntoPredeterminadoEs = "Invitación a partida de Pictionary Musical";
        private const string AsuntoPredeterminadoEn = "Pictionary Musical Game Invitation";

        /// <summary>
        /// Envia una invitacion a una partida al correo indicado de forma asincrona.
        /// </summary>
        /// <param name="correoDestino">Direccion de correo del destinatario.</param>
        /// <param name="codigoSala">Codigo de la sala.</param>
        /// <param name="creador">Nombre del creador de la sala.</param>
        /// <param name="idioma">Idioma del destinatario (es/en).</param>
        /// <returns>True si el correo se envio correctamente.</returns>
        public async Task<bool> EnviarInvitacionAsync(string correoDestino, string codigoSala, 
            string creador, string idioma)
        {
            if (string.IsNullOrWhiteSpace(correoDestino) || string.IsNullOrWhiteSpace(codigoSala))
            {
                return false;
            }

            var configuracion = ObtenerConfiguracionSmtp();
            if (!configuracion.EsValida)
            {
                _logger.Error("La configuracion de correo es invalida o esta incompleta.");
                return false;
            }

            string idiomaNormalizado = NormalizarIdioma(idioma);
            string asunto = ObtenerAsunto(idiomaNormalizado);
            string cuerpoHtml = ConstruirCuerpoMensaje(codigoSala, creador, idiomaNormalizado);

            return await EjecutarEnvioSmtpAsync(correoDestino, asunto, cuerpoHtml, configuracion);
        }

        private ConfiguracionSmtp ObtenerConfiguracionSmtp()
        {
            var config = new ConfiguracionSmtp
            {
                Remitente = ObtenerConfiguracion("CorreoRemitente", "Correo.Remitente.Direccion"),
                Contrasena = ObtenerConfiguracion("CorreoPassword", "Correo.Smtp.Contrasena"),
                Host = ObtenerConfiguracion("CorreoHost", "Correo.Smtp.Host"),
                Usuario = ObtenerConfiguracion("CorreoUsuario", "Correo.Smtp.Usuario"),
                PuertoString = ObtenerConfiguracion("CorreoPuerto", "Correo.Smtp.Puerto"),
                SslString = ObtenerConfiguracion("CorreoSsl", "Correo.Smtp.HabilitarSsl")
            };

            if (string.IsNullOrWhiteSpace(config.Usuario))
            {
                config.Usuario = config.Remitente;
            }

            return config;
        }

        private string ObtenerAsunto(string idiomaNormalizado)
        {
            string asuntoConfigurado = ObtenerConfiguracion(
                string.Format("CorreoAsuntoInvitacion.{0}", idiomaNormalizado),
                string.Format("Correo.Invitacion.Asunto.{0}", idiomaNormalizado),
                "CorreoAsuntoInvitacion",
                "Correo.Invitacion.Asunto");

            return string.IsNullOrWhiteSpace(asuntoConfigurado)
                ? ObtenerAsuntoPredeterminado(idiomaNormalizado)
                : asuntoConfigurado;
        }

        private async Task<bool> EjecutarEnvioSmtpAsync(string destinatario, string asunto, 
            string cuerpo, ConfiguracionSmtp config)
        {
            try
            {
                using (var mensaje = new MailMessage(config.Remitente, destinatario, asunto, 
                    cuerpo))
                {
                    mensaje.IsBodyHtml = true;
                    mensaje.BodyEncoding = Encoding.UTF8;
                    mensaje.SubjectEncoding = Encoding.UTF8;

                    using (var clienteSmtp = new SmtpClient(config.Host, config.Puerto))
                    {
                        clienteSmtp.EnableSsl = config.HabilitarSsl;

                        if (!string.IsNullOrWhiteSpace(config.Contrasena))
                        {
                            clienteSmtp.Credentials = new NetworkCredential(config.Usuario, 
                                config.Contrasena);
                        }

                        await clienteSmtp
                            .SendMailAsync(mensaje)
                            .ConfigureAwait(false);
                    }
                }

                return true;
            }
            catch (SmtpException excepcion)
            {
                _logger.Error("Error SMTP al enviar correo electronico.", excepcion);
                return false;
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Error("Operacion invalida al enviar correo.", excepcion);
                return false;
            }
            catch (ArgumentException excepcion)
            {
                _logger.Error("Argumentos invalidos para enviar correo.", excepcion);
                return false;
            }
            catch (FormatException excepcion)
            {
                _logger.Error("Formato de correo invalido al enviar invitacion.", excepcion);
                return false;
            }
            catch (Exception excepcion)
            {
                _logger.Error("Formato de correo invalido al enviar invitacion.", excepcion);
                return false;
            }
        }

        private class ConfiguracionSmtp
        {
            public string Remitente { get; set; }
            public string Contrasena { get; set; }
            public string Host { get; set; }
            public string Usuario { get; set; }
            public string PuertoString { get; set; }
            public string SslString { get; set; }

            public int Puerto => int.TryParse(PuertoString, out int p) ? p : 587;
            public bool HabilitarSsl => bool.TryParse(SslString, out bool ssl) && ssl;

            public bool EsValida => !string.IsNullOrWhiteSpace(Remitente)
                                 && !string.IsNullOrWhiteSpace(Host)
                                 && HabilitarSsl;
        }

        internal static string ConstruirCuerpoMensaje(string codigoSala, string creador, 
            string idioma)
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
            cuerpoHtml.Append
                ($"<h1 style='color:#4CAF50; letter-spacing: 2px;'>{codigoSala}</h1>");
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
                return string.Empty;
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

            return string.Empty;
        }
    }
}
