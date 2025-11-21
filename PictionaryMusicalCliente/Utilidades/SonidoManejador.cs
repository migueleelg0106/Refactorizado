using System;
using System.IO;
using System.Windows.Media;
using log4net;

namespace PictionaryMusicalCliente.ClienteServicios
{
    /// <summary>
    /// Provee métodos para reproducir efectos de sonido (SFX) cortos, respetando
    /// la preferencia de silencio del usuario.
    /// </summary>
    public static class SonidoManejador
    {
        private const double VolumenPredeterminado = 1.0;

        private static readonly ILog Log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Indica si los efectos de sonido están silenciados por preferencia del usuario.
        /// </summary>
        public static bool Silenciado
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
        public static void ReproducirSonido(string nombreArchivo, double volumen = VolumenPredeterminado)
        {
            if (Silenciado)
            {
                return;
            }

            try
            {
                string rutaSonido = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Recursos",
                    nombreArchivo);

                if (!File.Exists(rutaSonido))
                {
                    Log.Warn($"Sonido SFX no encontrado: {rutaSonido}");
                    return;
                }

                var player = new MediaPlayer();
                player.Open(new Uri(rutaSonido, UriKind.Absolute));
                player.Volume = Math.Max(0, Math.Min(VolumenPredeterminado, volumen));

                player.MediaEnded += (s, e) =>
                {
                    try
                    {
                        player.Stop();
                        player.Close();
                    }
                    catch (InvalidOperationException ex)
                    {
                        Log.Warn($"Error limpiando reproductor SFX: {ex.Message}");
                    }
                };

                player.Play();
            }
            catch (ArgumentException argEx)
            {
                Log.Error($"Argumentos de ruta inválidos para sonido: {nombreArchivo}", argEx);
            }
            catch (UriFormatException uriEx)
            {
                Log.Error($"Formato URI inválido para sonido: {nombreArchivo}", uriEx);
            }
            catch (FileNotFoundException fnfEx)
            {
                Log.Error($"Archivo perdido antes de reproducir: {fnfEx.FileName}", fnfEx);
            }
            catch (InvalidOperationException ioEx)
            {
                Log.Error($"Error de operación en MediaPlayer SFX: {ioEx.Message}", ioEx);
            }
        }

        /// <summary>
        /// Reproduce el sonido estándar de clic de botón.
        /// </summary>
        public static void ReproducirClick()
        {
            ReproducirSonido("piano_boton.mp3");
        }

        /// <summary>
        /// Reproduce el sonido estándar de error.
        /// </summary>
        public static void ReproducirError()
        {
            ReproducirSonido("error.mp3", 0.8);
        }

        /// <summary>
        /// Reproduce el sonido estándar de éxito o confirmación.
        /// </summary>
        public static void ReproducirExito()
        {
            ReproducirSonido("exito.mp3", 0.7);
        }
    }
}