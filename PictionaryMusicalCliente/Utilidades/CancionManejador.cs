using System;
using System.IO;
using System.Windows.Media;

namespace PictionaryMusicalCliente.Utilidades
{
    public class CancionManejador : IDisposable
    {
        private readonly MediaPlayer _reproductor;
        private bool _desechado;

        public double Volumen
        {
            get => _reproductor.Volume;
            set => _reproductor.Volume = Math.Max(0, Math.Min(1, value));
        }

        public bool EstaReproduciendo { get; private set; }

        public CancionManejador()
        {
            _reproductor = new MediaPlayer();
            _reproductor.MediaEnded += (s, e) => EstaReproduciendo = false;
            _reproductor.Volume = 1.0;
        }

        /// <summary>
        /// Reproduce una canción ubicada en la carpeta 'Recursos'.
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo con extensión (ej. "cancion.mp3")</param>
        public void Reproducir(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo)) return;

            try
            {
                string rutaCompleta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recursos", "Canciones", nombreArchivo);

                if (File.Exists(rutaCompleta))
                {
                    _reproductor.Open(new Uri(rutaCompleta, UriKind.Absolute));
                    _reproductor.Play();
                    EstaReproduciendo = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Archivo de audio no encontrado: {rutaCompleta}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al reproducir canción: {ex.Message}");
            }
        }

        /// <summary>
        /// Detiene la reproducción actual.
        /// </summary>
        public void Detener()
        {
            _reproductor.Stop();
            EstaReproduciendo = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_desechado)
            {
                if (disposing)
                {
                    _reproductor.Stop();
                    _reproductor.Close();
                }
                _desechado = true;
            }
        }
    }
}