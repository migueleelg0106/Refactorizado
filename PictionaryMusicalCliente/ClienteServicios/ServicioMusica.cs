using System;
using System.Diagnostics;
using System.Windows.Media;

namespace PictionaryMusicalCliente.ClienteServicios
{
    public class ServicioMusica : IDisposable
    {
        private readonly MediaPlayer _reproductor;
        private bool _desechado;

        public bool EstaReproduciendo { get; private set; }

        public double Volume
        {
            get => _reproductor.Volume;
            set
            {
                double clamped = Math.Max(0, Math.Min(1, value));
                _reproductor.Volume = clamped;
            }
        }

        public ServicioMusica()
        {
            _reproductor = new MediaPlayer();
            _reproductor.MediaEnded += EnMedioTerminado;
            _reproductor.MediaOpened += EnMedioAbierto;
            _reproductor.MediaFailed += EnMedioFallido;
            this.Volume = 0.4;
        }

        public void ReproducirEnBucle(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                Debug.WriteLine("El nombre del archivo no puede ser vacio.");
                return;
            }

            if (EstaReproduciendo)
            {
                _reproductor.Stop();
                EstaReproduciendo = false;
            }

            try
            {
                var uri = new Uri($"pack://application:,,,/Recursos/{nombreArchivo}", UriKind.Absolute);
                _reproductor.Open(uri);
            }
            catch (UriFormatException ex)
            {
                Debug.WriteLine($"Error en el formato de la URI: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"Error al intentar abrir: {ex.Message}");
            }
            catch (System.IO.IOException ex)
            {
                Debug.WriteLine($"Error de E/S: {ex.Message}");
            }
        }

        public void Pausar()
        {
            if (EstaReproduciendo)
            {
                _reproductor.Pause();
                EstaReproduciendo = false;
            }
        }

        public void Reanudar()
        {
            if (!EstaReproduciendo)
            {
                _reproductor.Play();
                EstaReproduciendo = true;
            }
        }

        private void EnMedioAbierto(object sender, EventArgs e)
        {
            _reproductor.Play();
            EstaReproduciendo = true;
        }

        private void EnMedioFallido(object sender, ExceptionEventArgs e)
        {
            EstaReproduciendo = false;
            Debug.WriteLine($"Error al cargar la música: {e.ErrorException.Message}");
        }

        public void Detener()
        {
            if (EstaReproduciendo)
            {
                _reproductor.Stop();
                EstaReproduciendo = false;
            }
        }

        public void EnMedioTerminado(object sender, EventArgs e)
        {
            _reproductor.Position = TimeSpan.Zero;
            _reproductor.Play();
            EstaReproduciendo = true; 
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_desechado)
            {
                return;
            }

            if (disposing)
            {
                _reproductor.MediaEnded -= EnMedioTerminado;
                _reproductor.MediaOpened -= EnMedioAbierto;
                _reproductor.MediaFailed -= EnMedioFallido;

                _reproductor.Stop();
                _reproductor.Close();
            }

            _desechado = true;
        }
    }
}