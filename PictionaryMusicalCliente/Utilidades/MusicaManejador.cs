using System;
using System.Windows.Media;
using log4net;
using PictionaryMusicalCliente.Utilidades.Abstracciones;

namespace PictionaryMusicalCliente.ClienteServicios
{
    /// <summary>
    /// Controla la reproduccion de musica de fondo en la aplicacion.
    /// </summary>
    public class MusicaManejador : IMusicaManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const double VolumenPredeterminado = 0.5;

        private readonly MediaPlayer _reproductor;
        private bool _desechado;
        private double _volumenGuardado;

        /// <summary>
        /// Inicializa una nueva instancia del manejador de musica.
        /// </summary>
        public MusicaManejador()
        {
            _reproductor = new MediaPlayer();
            _reproductor.MediaEnded += EnMedioTerminado;
            _reproductor.MediaOpened += EnMedioAbierto;
            _reproductor.MediaFailed += EnMedioFallido;

            double volumenPreferido = ObtenerVolumenGuardado();
            _volumenGuardado = volumenPreferido;
            Volumen = volumenPreferido;
            EstaSilenciado = _volumenGuardado < 0.0001;
        }

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
                double ajustado = Math.Max(0, Math.Min(1, value));
                if (Math.Abs(_reproductor.Volume - ajustado) < 0.0001)
                {
                    return;
                }

                _reproductor.Volume = ajustado;
                EstaSilenciado = ajustado < 0.0001;

                if (!EstaSilenciado)
                {
                    _volumenGuardado = ajustado;
                }
                GuardarPreferencia(ajustado);
            }
        }

        /// <summary>
        /// Alterna el estado de silencio (mute) del reproductor.
        /// </summary>
        /// <returns>True si esta silenciado, false si tiene volumen.</returns>
        public bool AlternarSilencio()
        {
            Volumen = EstaSilenciado ? _volumenGuardado : 0;
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
                _logger.Warn("Intento de reproducir musica con nombre vacio.");
                return;
            }

            DetenerReproduccionActual();

            try
            {
                var uri = new Uri($"Recursos/{nombreArchivo}", UriKind.Relative);
                _reproductor.Open(uri);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Error al cargar musica: {0}. Detalles: {1}", nombreArchivo, ex);
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
            DetenerReproduccionActual();
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

        private void DetenerReproduccionActual()
        {
            if (EstaReproduciendo)
            {
                _reproductor.Stop();
                EstaReproduciendo = false;
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
            _logger.ErrorFormat("Fallo crítico en reproducción de medio", e.ErrorException);
        }

        private static double ObtenerVolumenGuardado()
        {
            double volumenGuardado = Properties.Settings.Default.volumenMusica;
            if (double.IsNaN(volumenGuardado) || double.IsInfinity(volumenGuardado))
            {
                return VolumenPredeterminado;
            }

            return Math.Max(0, Math.Min(1, volumenGuardado));
        }

        private static void GuardarPreferencia(double volumen)
        {
            Properties.Settings.Default.volumenMusica = volumen;
            Properties.Settings.Default.Save();
        }
    }
}