using System;
using System.Diagnostics;
using System.Windows.Media;

namespace PictionaryMusicalCliente.ClienteServicios
{
    /// <summary>
    /// Controla la reproduccion de musica de fondo en la aplicacion.
    /// </summary>
    public class MusicaManejador : IDisposable
    {
        private readonly MediaPlayer _reproductor;
        private bool _desechado;
        private double _volumenGuardado;

        /// <summary>
        /// Indica si el reproductor esta actualmente emitiendo sonido.
        /// </summary>
        public bool EstaReproduciendo { get; private set; }

        /// <summary>
        /// Indica si el volumen esta en cero.
        /// </summary>
        public bool EstaSilenciado { get; private set; }

        /// <summary>
        /// Obtiene o establece el nivel de volumen (0.0 a 1.0).
        /// </summary>
        public double Volumen
        {
            get => _reproductor.Volume;
            set
            {
                double clamped = Math.Max(0, Math.Min(1, value));
                _reproductor.Volume = clamped;

                EstaSilenciado = clamped < 0.0001;
                if (!EstaSilenciado)
                {
                    _volumenGuardado = clamped;
                }
            }
        }

        /// <summary>
        /// Inicializa una nueva instancia del manejador de musica.
        /// </summary>
        public MusicaManejador()
        {
            _reproductor = new MediaPlayer();
            _reproductor.MediaEnded += EnMedioTerminado;
            _reproductor.MediaOpened += EnMedioAbierto;
            _reproductor.MediaFailed += EnMedioFallido;
            this.Volumen = 0.4;
            EstaSilenciado = false;
        }

        /// <summary>
        /// Alterna el estado de silencio (mute) del reproductor.
        /// </summary>
        /// <returns>True si esta silenciado, false si tiene volumen.</returns>
        public bool AlternarSilencio()
        {
            if (EstaSilenciado)
            {
                this.Volumen = _volumenGuardado;
            }
            else
            {
                this.Volumen = 0;
            }
            return EstaSilenciado;
        }

        /// <summary>
        /// Inicia la reproduccion de un archivo de audio en bucle infinito.
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo en la carpeta Recursos.</param>
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
                var uri = new Uri($"Recursos/{nombreArchivo}", UriKind.Relative);
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

        /// <summary>
        /// Pausa la reproduccion actual.
        /// </summary>
        public void Pausar()
        {
            if (EstaReproduciendo)
            {
                _reproductor.Pause();
                EstaReproduciendo = false;
            }
        }

        /// <summary>
        /// Reanudar la reproduccion si estaba pausada.
        /// </summary>
        public void Reanudar()
        {
            if (!EstaReproduciendo)
            {
                _reproductor.Play();
                EstaReproduciendo = true;
            }
        }

        /// <summary>
        /// Detiene completamente la reproduccion y reinicia la posicion.
        /// </summary>
        public void Detener()
        {
            if (EstaReproduciendo)
            {
                _reproductor.Stop();
                EstaReproduciendo = false;
            }
        }

        /// <summary>
        /// Manejador de evento para reiniciar la musica cuando termina (Loop).
        /// </summary>
        public void EnMedioTerminado(object sender, EventArgs e)
        {
            _reproductor.Position = TimeSpan.Zero;
            _reproductor.Play();
            EstaReproduciendo = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera los recursos no administrados y opcionalmente los administrados.
        /// </summary>
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
    }
}