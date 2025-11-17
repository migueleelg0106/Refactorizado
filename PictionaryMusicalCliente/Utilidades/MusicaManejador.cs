using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace PictionaryMusicalCliente.ClienteServicios
{
    public class MusicaManejador : IDisposable
    {
        private readonly MediaPlayer _reproductor;
        private readonly Dispatcher _dispatcher;
        private readonly bool _entornoWpfDisponible;
        private bool _desechado;
        private double _volumenGuardado;
        private double _volumenSimulado;

        public bool EstaReproduciendo { get; private set; }
        public bool EstaSilenciado { get; private set; }

        public double Volumen
        {
            get
            {
                if (!PuedeUsarMediaPlayer)
                {
                    return _volumenSimulado;
                }

                return _dispatcher.Invoke(() => _reproductor.Volume);
            }
            set
            {
                double clamped = Math.Max(0, Math.Min(1, value));

                if (!PuedeUsarMediaPlayer)
                {
                    _volumenSimulado = clamped;
                    ActualizarEstadoSilencio(clamped);
                    return;
                }

                _dispatcher.Invoke(() =>
                {
                    _reproductor.Volume = clamped;
                    ActualizarEstadoSilencio(clamped);
                });
            }
        }

        public MusicaManejador()
        {
            _entornoWpfDisponible = EsEntornoWpfDisponible();
            _dispatcher = null;
            _reproductor = null;

            if (_entornoWpfDisponible)
            {
                _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
                _reproductor = new MediaPlayer();
                _reproductor.MediaEnded += EnMedioTerminado;
                _reproductor.MediaOpened += EnMedioAbierto;
                _reproductor.MediaFailed += EnMedioFallido;
            }

            _volumenSimulado = 0.4;
            this.Volumen = 0.4;
            EstaSilenciado = false;
        }

        /// <summary>
        /// Alterna el estado de silencio (mute) del reproductor.
        /// </summary>
        /// <returns>Devuelve true si el reproductor está AHORA silenciado, false si no lo está.</returns>
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

        public void ReproducirEnBucle(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                Debug.WriteLine("El nombre del archivo no puede ser vacio.");
                return;
            }

            if (!PuedeUsarMediaPlayer)
            {
                EstaReproduciendo = true;
                return;
            }

            _dispatcher.Invoke(() =>
            {
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
            });
        }

        public void Pausar()
        {
            if (!PuedeUsarMediaPlayer)
            {
                EstaReproduciendo = false;
                return;
            }

            _dispatcher.Invoke(() =>
            {
                if (EstaReproduciendo)
                {
                    _reproductor.Pause();
                    EstaReproduciendo = false;
                }
            });
        }

        public void Reanudar()
        {
            if (!PuedeUsarMediaPlayer)
            {
                EstaReproduciendo = true;
                return;
            }

            _dispatcher.Invoke(() =>
            {
                if (!EstaReproduciendo)
                {
                    _reproductor.Play();
                    EstaReproduciendo = true;
                }
            });
        }

        private void EnMedioAbierto(object sender, EventArgs e)
        {
            if (!PuedeUsarMediaPlayer)
            {
                return;
            }

            _dispatcher.Invoke(() =>
            {
                _reproductor.Play();
                EstaReproduciendo = true;
            });
        }

        private void EnMedioFallido(object sender, ExceptionEventArgs e)
        {
            if (!PuedeUsarMediaPlayer)
            {
                return;
            }

            _dispatcher.Invoke(() =>
            {
                EstaReproduciendo = false;
                Debug.WriteLine($"Error al cargar la música: {e.ErrorException.Message}");
            });
        }

        public virtual void Detener()
        {
            if (!PuedeUsarMediaPlayer)
            {
                EstaReproduciendo = false;
                return;
            }

            _dispatcher.Invoke(() =>
            {
                if (EstaReproduciendo)
                {
                    _reproductor.Stop();
                    EstaReproduciendo = false;
                }
            });
        }

        public void EnMedioTerminado(object sender, EventArgs e)
        {
            if (!PuedeUsarMediaPlayer)
            {
                return;
            }

            _dispatcher.Invoke(() =>
            {
                _reproductor.Position = TimeSpan.Zero;
                _reproductor.Play();
                EstaReproduciendo = true;
            });
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
                if (PuedeUsarMediaPlayer)
                {
                    _dispatcher.Invoke(() =>
                    {
                        _reproductor.MediaEnded -= EnMedioTerminado;
                        _reproductor.MediaOpened -= EnMedioAbierto;
                        _reproductor.MediaFailed -= EnMedioFallido;

                        _reproductor.Stop();
                        _reproductor.Close();
                    });
                }
            }

            _desechado = true;
        }

        private void ActualizarEstadoSilencio(double volumen)
        {
            EstaSilenciado = volumen < 0.0001;
            if (!EstaSilenciado)
            {
                _volumenGuardado = volumen;
            }
        }

        private static bool EsEntornoWpfDisponible()
        {
            return Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;
        }

        private bool PuedeUsarMediaPlayer => _entornoWpfDisponible && _dispatcher != null && _reproductor != null;
    }
}