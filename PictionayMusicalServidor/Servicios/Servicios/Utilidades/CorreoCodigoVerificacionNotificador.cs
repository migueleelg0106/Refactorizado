using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Servicios.Servicios.Utilidades
{
    public class CorreoCodigoVerificacionNotificador : ICodigoVerificacionNotificador
    {
        private const string AsuntoPredeterminado = "Código de verificación";

        public async Task<bool> NotificarAsync(string correoDestino, string codigo, string usuarioDestino)
        {
            if (string.IsNullOrWhiteSpace(correoDestino) || string.IsNullOrWhiteSpace(codigo))
            {
                return false;
            }

            string remitente = ConfigurationManager.AppSettings["CorreoRemitente"];
            string contrasena = ConfigurationManager.AppSettings["CorreoPassword"];
            string host = ConfigurationManager.AppSettings["CorreoHost"];
            string puertoConfigurado = ConfigurationManager.AppSettings["CorreoPuerto"];
            string asunto = ConfigurationManager.AppSettings["CorreoAsunto"] ?? AsuntoPredeterminado;
            bool.TryParse(ConfigurationManager.AppSettings["CorreoSsl"], out bool habilitarSsl);

            if (string.IsNullOrWhiteSpace(remitente) || string.IsNullOrWhiteSpace(host))
            {
                return false;
            }

            if (!int.TryParse(puertoConfigurado, out int puerto))
            {
                puerto = 587;
            }

            string cuerpo = ConstruirCuerpoMensaje(usuarioDestino, codigo);

            try
            {
                using (var mensaje = new MailMessage(remitente, correoDestino, asunto, cuerpo))
                {
                    mensaje.IsBodyHtml = false;

                    using (var cliente = new SmtpClient(host, puerto))
                    {
                        cliente.EnableSsl = habilitarSsl;

                        if (!string.IsNullOrWhiteSpace(contrasena))
                        {
                            cliente.Credentials = new NetworkCredential(remitente, contrasena);
                        }

                        await cliente.SendMailAsync(mensaje).ConfigureAwait(false);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string ConstruirCuerpoMensaje(string usuarioDestino, string codigo)
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(usuarioDestino))
            {
                builder.AppendLine($"Hola {usuarioDestino},");
            }
            else
            {
                builder.AppendLine("Hola,");
            }

            builder.AppendLine();
            builder.AppendLine("Gracias por registrarte en Pictionary Musical.");
            builder.AppendLine($"Tu código de verificación es: {codigo}");
            builder.AppendLine();
            builder.AppendLine("Si no solicitaste este código puedes ignorar este mensaje.");

            return builder.ToString();
        }
    }
}
