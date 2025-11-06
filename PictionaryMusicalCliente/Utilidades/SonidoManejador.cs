using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace PictionaryMusicalCliente.ClienteServicios
{
    public static class ManejadorSonido
    {
        /// <summary>
        /// Reproduce un archivo de sonido ubicado en la carpeta "Recursos" de la aplicación.
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo con extensión (ej. "click.mp3")</param>
        /// <param name="volumen">Volumen de 0.0 a 1.0 (por defecto 0.5)</param>
        public static void ReproducirSonido(string nombreArchivo, double volumen = 0.5)
        {
            try
            {
                string rutaSonido = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recursos", nombreArchivo);

                if (!File.Exists(rutaSonido))
                {
                    Debug.WriteLine($"Sonido no encontrado en: {rutaSonido}");
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
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error limpiando reproductor SFX: {ex.Message}");
                    }
                };

                player.Play();
            }
            catch (ArgumentException argEx)
            {
                Debug.WriteLine($"Error en los argumentos de la ruta: {argEx.Message}");
            }
            catch (UriFormatException uriEx)
            {
                Debug.WriteLine($"Formato de URI inválido para el sonido: {uriEx.Message}");
            }
            catch (FileNotFoundException fnfEx)
            {
                Debug.WriteLine($"Archivo perdido antes de reproducir: {fnfEx.FileName}");
            }
            catch (InvalidOperationException ioEx)
            {
                Debug.WriteLine($"Error de operación en MediaPlayer: {ioEx.Message}");
            }
        }

        public static void ReproducirClick()
        {
            ReproducirSonido("piano_boton.mp3", 1.0);
        }

        public static void ReproducirError()
        {
            ReproducirSonido("error.mp3", 0.8);
        }

        public static void ReproducirExito()
        {
            ReproducirSonido("exito.mp3", 0.7);
        }
    }
}