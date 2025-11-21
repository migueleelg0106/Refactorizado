using System;
using System.IO;
using System.Windows.Media;
using log4net;

namespace PictionaryMusicalCliente.ClienteServicios
{
    /// <summary>
    /// Provee metodos estaticos para reproducir efectos de sonido (SFX) cortos.
    /// </summary>
    public static class ManejadorSonido
    {
        private static readonly ILog Log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Reproduce un archivo de sonido ubicado en la carpeta "Recursos" de la aplicación.
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo con extensión.</param>
        /// <param name="volumen">Volumen de 0.0 a 1.0 (por defecto 0.5)</param>
        public static void ReproducirSonido(string nombreArchivo, double volumen = 0.5)
        {
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
                player.Volume = volumen;

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
        /// Reproduce el sonido estandar de clic de boton.
        /// </summary>
        public static void ReproducirClick()
        {
            ReproducirSonido("piano_boton.mp3", 1.0);
        }

        /// <summary>
        /// Reproduce el sonido estandar de error.
        /// </summary>
        public static void ReproducirError()
        {
            ReproducirSonido("error.mp3", 0.8);
        }

        /// <summary>
        /// Reproduce el sonido estandar de exito o confirmacion.
        /// </summary>
        public static void ReproducirExito()
        {
            ReproducirSonido("exito.mp3", 0.7);
        }
    }
}