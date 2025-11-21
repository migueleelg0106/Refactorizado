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
        private static readonly ILog Log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const double VolumenPredeterminado = 1.0;

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
        public static void ReproducirSonido(string nombreArchivo, 
            double volumen = VolumenPredeterminado)
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
                    Log.WarnFormat("Sonido SFX no encontrado: {0}",
                        rutaSonido);
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
                        Log.WarnFormat("Error limpiando reproductor SFX: {0}",
                            ex.Message);
                    }
                };

                player.Play();
            }
            catch (ArgumentException argEx)
            {
                Log.ErrorFormat("Argumentos de ruta inválidos para sonido: {0}",
                    nombreArchivo, argEx);
            }
            catch (UriFormatException uriEx)
            {
                Log.ErrorFormat("Formato URI inválido para sonido: {0}",
                    nombreArchivo, uriEx);
            }
            catch (FileNotFoundException fnfEx)
            {
                Log.ErrorFormat("Archivo perdido antes de reproducir: {0}",
                    fnfEx.FileName, fnfEx);
            }
            catch (InvalidOperationException ioEx)
            {
                Log.ErrorFormat("Error de operación en MediaPlayer SFX: {0}",
                    ioEx.Message, ioEx);
            }
        }

        /// <summary>
        /// Reproduce el sonido estándar de clic de boton.
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
        /// Reproduce el sonido estándar de éxito o confirmacion.
        /// </summary>
        public static void ReproducirExito()
        {
            ReproducirSonido("exito.mp3", 0.7);
        }
    }
}