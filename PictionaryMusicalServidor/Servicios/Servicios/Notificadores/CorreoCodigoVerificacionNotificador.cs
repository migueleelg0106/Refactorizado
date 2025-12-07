using log4net;
using PictionaryMusicalServidor.Servicios.Servicios.Notificadores;
using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Servicio para el envio de codigos de verificacion por correo electronico.
    /// Implementa la interfaz ICodigoVerificacionNotificador.
    /// </summary>
    public class CorreoCodigoVerificacionNotificador : ICodigoVerificacionNotificador
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(CorreoCodigoVerificacionNotificador));

        private const string AsuntoPredeterminadoEs = "Codigo de verificacion";
        private const string AsuntoPredeterminadoEn = "Verification code";

        /// <summary>
        /// Envia un codigo de verificacion al usuario especificado de forma asincrona.
        /// </summary>
        /// <param name="correoDestino">Correo electronico del destinatario.</param>
        /// <param name="codigo">Codigo de verificacion a enviar.</param>
        /// <param name="usuarioDestino">Nombre del usuario destinatario.</param>
        /// <param name="idioma">Idioma preferido para el correo (es/en).</param>
        /// <returns>True si el envio fue exitoso, False en caso contrario.</returns>
        public async Task<bool> NotificarAsync(string correoDestino, string codigo,
            string usuarioDestino, string idioma)
        {
            if (string.IsNullOrWhiteSpace(correoDestino) || string.IsNullOrWhiteSpace(codigo))
            {
                return false;
            }

            var config = ObtenerConfiguracionSmtp();
            if (!config.EsValida)
            {
                _logger.Error("La configuracion de correo es invalida o esta incompleta.");
                return false;
            }

            string idiomaNormalizado = NormalizarIdioma(idioma);
            string asunto = ObtenerAsunto(idiomaNormalizado);
            string cuerpoHtml = ConstruirCuerpoMensaje(usuarioDestino, codigo, idiomaNormalizado);

            return await EjecutarEnvioSmtpAsync(correoDestino, asunto, cuerpoHtml, config);
        }

        /// <summary>
        /// Construye el cuerpo HTML del mensaje de correo con el codigo de verificacion.
        /// Metodo interno para facilitar pruebas unitarias.
        /// </summary>
        /// <param name="usuarioDestino">Nombre del usuario al que se dirige el correo.</param>
        /// <param name="codigo">Codigo numerico de verificacion.</param>
        /// <param name="idioma">Idioma para el texto del mensaje.</param>
        /// <returns>Cadena con el HTML del cuerpo del correo.</returns>
        internal static string ConstruirCuerpoMensaje(string usuarioDestino, string codigo,
            string idioma)
        {
            string idiomaNormalizado = NormalizarIdioma(idioma);
            bool esIngles = idiomaNormalizado == "en";

            string saludo = esIngles ? "Hello" : "Hola";
            string mensajeCodigo = esIngles
                ? "Your verification code is:"
                : "Tu codigo de verificacion es:";

            string mensajeIgnorar = esIngles
                ? "If you did not request this code you can ignore this message."
                : "Si no solicitaste este codigo puedes ignorar este mensaje.";

            var cuerpoHtml = new StringBuilder();

            cuerpoHtml.Append("<html><body style='font-family: Arial, sans-serif;'>");

            if (!string.IsNullOrWhiteSpace(usuarioDestino))
            {
                cuerpoHtml.Append($"<h2>{saludo} {usuarioDestino},</h2>");
            }
            else
            {
                cuerpoHtml.Append($"<h2>{saludo},</h2>");
            }

            cuerpoHtml.Append($"<p>{mensajeCodigo}</p>");
            cuerpoHtml.Append($"<h1 style='color:#0078D7; letter-spacing: 5px;'>{codigo}</h1>");
            cuerpoHtml.Append($"<p style='color: gray; font-size: 0.9em;'>{mensajeIgnorar}</p>");
            cuerpoHtml.Append("</body></html>");

            return cuerpoHtml.ToString();
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
                "CorreoAsunto",
                "Correo.Codigo.Asunto");

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
                            clienteSmtp.Credentials =
                                new NetworkCredential(config.Usuario, config.Contrasena);
                        }

                        await clienteSmtp
                            .SendMailAsync(mensaje)
                            .ConfigureAwait(false);
                    }
                }

                return true;
            }
            catch (SmtpException ex)
            {
                _logger.Error("Error SMTP al enviar correo electronico.", ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operacion invalida al enviar correo.", ex);
                return false;
            }
            catch (ArgumentException ex)
            {
                _logger.Error("Argumentos invalidos para enviar correo.", ex);
                return false;
            }
            catch (FormatException ex)
            {
                _logger.Error("Formato de correo invalido al enviar codigo de verificacion.", ex);
                return false;
            }
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
    }
}