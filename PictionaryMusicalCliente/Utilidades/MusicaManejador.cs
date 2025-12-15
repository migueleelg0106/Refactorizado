using System;
using System.IO;
using System.Windows.Media;
using log4net;

namespace PictionaryMusicalCliente.Utilidades
{
    /// <summary>
    /// Controla la reproduccion de musica de fondo en la aplicacion.
    /// </summary>
    public class MusicaManejador : IDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private const double VolumenPredeterminado = 0.5;

        private readonly MediaPlayer _reproductor;
        private bool _desechado;
        private bool _estaReproduciendo;

        /// <summary>
        /// Inicializa una nueva instancia del manejador de musica.
        /// </summary>
        public MusicaManejador()
        {
            _reproductor = new MediaPlayer();
            _reproductor.MediaEnded += EnMedioTerminado;
            _reproductor.MediaFailed += EnMedioFallido;

            double volumenGuardado = Properties.Settings.Default.volumenMusica;
            if (double.IsNaN(volumenGuardado) || double.IsInfinity(volumenGuardado))
            {
                 volumenGuardado = VolumenPredeterminado;
            }
            
            _reproductor.Volume = Math.Max(0, Math.Min(1, volumenGuardado));
        }

        /// <summary>
        /// Obtiene o establece el volumen de la musica (0.0 a 1.0).
        /// </summary>
        public double Volumen
        {
            get => _reproductor.Volume;
            set
            {
                if (_desechado) return;
                
                double valorSeguro = Math.Max(0, Math.Min(1, value));
                _reproductor.Volume = valorSeguro;
                
                Properties.Settings.Default.volumenMusica = valorSeguro;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Inicia la reproduccion en bucle del archivo especificado.
        /// </summary>
        public void ReproducirEnBucle(string nombreArchivo)
        {
            if (_desechado || string.IsNullOrWhiteSpace(nombreArchivo)) return;

            try
            {
                string ruta = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Recursos",
                    nombreArchivo);

                _reproductor.Open(new Uri(ruta, UriKind.Absolute));
                _reproductor.Play();
                _estaReproduciendo = true;
            }
            catch (UriFormatException excepcion)
            {
                _logger.ErrorFormat("URI de musica invalida ({0}): {1}", nombreArchivo, excepcion);
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.WarnFormat("No se pudo iniciar la musica ({0}) por estado invalido: {1}", 
                    nombreArchivo, excepcion);
            }
        }

        /// <summary>
        /// Detiene la reproduccion actual.
        /// </summary>
        public void Detener()
        {
            if (_desechado || !_estaReproduciendo) return;

            try
            {
                _reproductor.Stop();
                _estaReproduciendo = false;
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Warn("Error al detener el reproductor de musica.", excepcion);
            }
        }
        
        /// <summary>
        /// Alterna entre silenciar y restaurar el volumen.
        /// </summary>
        /// <returns>True si quedo silenciado, False si tiene volumen.</returns>
        public bool AlternarSilencio()
        {
            bool estaSilenciado = _reproductor.IsMuted;
            _reproductor.IsMuted = !estaSilenciado;
            return !estaSilenciado;
        }

        private void EnMedioTerminado(object remitente, EventArgs argumentosEvento)
        {
            if (!_desechado && _estaReproduciendo)
            {
                try
                {
                    _reproductor.Position = TimeSpan.Zero;
                    _reproductor.Play();
                }
                catch (InvalidOperationException excepcion)
                {
                    _logger.Warn("Error al reiniciar el bucle de musica.", excepcion);
                }
            }
        }

        private void EnMedioFallido(object remitente, ExceptionEventArgs argumentosEvento)
        {
            _logger.ErrorFormat("Fallo critico en MediaPlayer de musica: {0}", argumentosEvento.ErrorException);
            _estaReproduciendo = false;
        }

        /// <summary>
        /// Libera los recursos del reproductor.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_desechado) return;

            if (disposing)
            {
                _reproductor.MediaEnded -= EnMedioTerminado;
                _reproductor.MediaFailed -= EnMedioFallido;
                
                try 
                {
                    _reproductor.Stop();
                    _reproductor.Close();
                }
                catch (InvalidOperationException) 
                {
                    _logger.Warn(
                        "El reproductor de musica no estaba en un estado valido para cerrarse.");
                }
            }

            _desechado = true;
        }
    }
}