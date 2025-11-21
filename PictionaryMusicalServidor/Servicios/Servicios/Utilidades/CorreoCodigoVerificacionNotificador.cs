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

        private const string AsuntoPredeterminadoEs = "Código de verificación";
        private const string AsuntoPredeterminadoEn = "Verification code";

        public async Task<bool> NotificarAsync(string correoDestino, string codigo, string usuarioDestino, string idioma)
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
            string idiomaNormalizado = NormalizarIdioma(idioma);
            string asuntoConfigurado = ObtenerConfiguracion("CorreoAsunto", "Correo.Codigo.Asunto");
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

            string cuerpoHtml = ConstruirCuerpoMensaje(usuarioDestino, codigo, idiomaNormalizado);

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

                        await clienteSmtp
                            .SendMailAsync(mensajeCorreo)
                            .ConfigureAwait(false);
                    }
                }

                _logger.Info($"Código de verificación enviado a '{correoDestino}'.");
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

        internal static string ConstruirCuerpoMensaje(string usuarioDestino, string codigo, string idioma)
        {
            string idiomaNormalizado = NormalizarIdioma(idioma);
            bool esIngles = idiomaNormalizado == "en";

            string saludo = esIngles ? "Hello" : "Hola";
            string mensajeCodigo = esIngles
                ? "Your verification code is:"
                : "Tu código de verificación es:";
            string mensajeIgnorar = esIngles
                ? "If you did not request this code you can ignore this message."
                : "Si no solicitaste este código puedes ignorar este mensaje.";

            var cuerpoHtml = new StringBuilder();

            cuerpoHtml.Append("<html><body>");

            if (!string.IsNullOrWhiteSpace(usuarioDestino))
            {
                cuerpoHtml.Append($"<h2>{saludo} {usuarioDestino},</h2>");
            }
            else
            {
                cuerpoHtml.Append($"<h2>{saludo},</h2>");
            }

            cuerpoHtml.Append($"<p>{mensajeCodigo}</p>");
            cuerpoHtml.Append($"<h1>{codigo}</h1>");
            cuerpoHtml.Append($"<p>{mensajeIgnorar}</p>");
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