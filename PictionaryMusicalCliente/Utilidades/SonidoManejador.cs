using System;
using System.IO;
using System.Windows.Media;
using log4net;
using PictionaryMusicalCliente.Utilidades.Abstracciones;

namespace PictionaryMusicalCliente.ClienteServicios
{
    /// <summary>
    /// Provee métodos para reproducir efectos de sonido (SFX) cortos, respetando
    /// la preferencia de silencio del usuario.
    /// </summary>
    public class SonidoManejador : ISonidoManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const double VolumenPredeterminado = 1.0;

        /// <summary>
        /// Indica si los efectos de sonido están silenciados por preferencia del usuario.
        /// </summary>
        public bool Silenciado
        {
            get => Properties.Settings.Default.efectosSilenciados;
            set
            {
                if (Properties.Settings.Default.efectosSilenciados != value)
                {
                    Properties.Settings.Default.efectosSilenciados = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// <summary>
        /// Reproduce un archivo de sonido ubicado en la carpeta "Recursos" de la aplicación
        /// si la preferencia de usuario lo permite.
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo con extensión.</param>
        /// <param name="volumen">Volumen de 0.0 a 1.0 (por defecto 1.0)</param>
        public void ReproducirSonido(string nombreArchivo, 
            double volumen = VolumenPredeterminado)
        {
            if (Silenciado || string.IsNullOrWhiteSpace(nombreArchivo))
            {
                return;
            }

            try
            {
                string rutaSonido = ObtenerRutaAbsoluta(nombreArchivo);

                if (!File.Exists(rutaSonido))
                {
                    _logger.WarnFormat("Sonido SFX no encontrado: {0}", rutaSonido);
                    return;
                }

                IniciarReproductor(rutaSonido, volumen);
            }
            catch (ArgumentException argEx)
            {
                _logger.ErrorFormat("Argumentos de ruta inválidos para sonido: {0}",
                    nombreArchivo, argEx);
            }
            catch (UriFormatException uriEx)
            {
                _logger.ErrorFormat("Formato URI inválido para sonido: {0}",
                    nombreArchivo, uriEx);
            }
            catch (FileNotFoundException fnfEx)
            {
                _logger.ErrorFormat("Archivo perdido antes de reproducir: {0}",
                    fnfEx.FileName, fnfEx);
            }
            catch (InvalidOperationException ioEx)
            {
                _logger.ErrorFormat("Error de operación en MediaPlayer SFX: {0}",
                    ioEx.Message, ioEx);
            }
        }

        /// <summary>
        /// Reproduce el sonido estándar de clic de boton.
        /// </summary>
        public void ReproducirClick()
        {
            ReproducirSonido("piano_boton.mp3");
        }

        /// <summary>
        /// Reproduce el sonido estándar de error.
        /// </summary>
        public void ReproducirError()
        {
            ReproducirSonido("error.mp3", 0.8);
        }

        /// <summary>
        /// Reproduce el sonido estándar de éxito o confirmacion.
        /// </summary>
        public void ReproducirExito()
        {
            ReproducirSonido("exito.mp3", 0.7);
        }

        private void IniciarReproductor(string ruta, double volumen)
        {
            var player = new MediaPlayer();
            player.Open(new Uri(ruta, UriKind.Absolute));
            player.Volume = Math.Max(0, Math.Min(VolumenPredeterminado, volumen));

            player.MediaEnded += (s, e) => LimpiarReproductor(player);
            player.Play();
        }

        private void LimpiarReproductor(MediaPlayer player)
        {
            try
            {
                player.Stop();
                player.Close();
            }
            catch (Exception ex)
            {
                _logger.Warn("Error limpiando reproductor SFX.", ex);
            }
        }

        private static string ObtenerRutaAbsoluta(string nombreArchivo)
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Recursos",
                nombreArchivo);
        }
    }
}