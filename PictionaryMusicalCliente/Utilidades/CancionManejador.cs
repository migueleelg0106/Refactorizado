using System;
using System.IO;
using System.Windows.Media;
using log4net;

namespace PictionaryMusicalCliente.Utilidades
{
    /// <summary>
    /// Maneja la reproduccion de canciones especificas del juego.
    /// </summary>
    public class CancionManejador : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const double VolumenPredeterminado = 0.5;
        private readonly MediaPlayer _reproductor;
        private bool _desechado;

        /// <summary>
        /// Obtiene o establece el volumen de la reproduccion (0.0 a 1.0).
        /// </summary>
        public double Volumen
        {
            get => _reproductor.Volume;
            set
            {
                double clamped = Math.Max(0, Math.Min(1, value));
                if (Math.Abs(_reproductor.Volume - clamped) < 0.0001)
                {
                    return;
                }

                _reproductor.Volume = clamped;
                GuardarPreferencia(clamped);
            }
        }

        /// <summary>
        /// Indica si actualmente se esta reproduciendo una cancion.
        /// </summary>
        public bool EstaReproduciendo { get; private set; }

        /// <summary>
        /// Inicializa una nueva instancia del manejador de canciones.
        /// </summary>
        public CancionManejador()
        {
            _reproductor = new MediaPlayer();
            _reproductor.MediaEnded += (s, e) => EstaReproduciendo = false;
            _reproductor.Volume = ObtenerVolumenGuardado();
        }

        /// <summary>
        /// Reproduce una canción ubicada en la carpeta 'Recursos'.
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo con extensión.</param>
        public void Reproducir(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                Log.Warn("Se intentó reproducir una canción con nombre de archivo vacío.");
                return;
            }

            try
            {
                string rutaCompleta = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Recursos",
                    "Canciones",
                    nombreArchivo);

                if (File.Exists(rutaCompleta))
                {
                    _reproductor.Open(new Uri(rutaCompleta, UriKind.Absolute));
                    _reproductor.Play();
                    EstaReproduciendo = true;
                    Log.InfoFormat("Reproduciendo canción: {0}", nombreArchivo);
                }
                else
                {
                    Log.ErrorFormat("Archivo de audio no encontrado en ruta: {0}",
                        rutaCompleta);
                }
            }
            catch (UriFormatException uriEx)
            {
                Log.ErrorFormat("Formato de URI inválido para canción: {0}",
                    nombreArchivo, uriEx);
            }
            catch (InvalidOperationException opEx)
            {
                Log.ErrorFormat("Error de operación en reproductor para: {0}", 
                    nombreArchivo, opEx);
            }
            catch (ArgumentException argEx)
            {
                Log.ErrorFormat("Argumento inválido en ruta de canción: {0}",
                    nombreArchivo, argEx);
            }
        }

        /// <summary>
        /// Detiene la reproducción actual.
        /// </summary>
        public void Detener()
        {
            try
            {
                _reproductor.Stop();
                EstaReproduciendo = false;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error("Error al intentar detener la reproducción.", ex);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera los recursos del reproductor.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_desechado)
            {
                if (disposing)
                {
                    try
                    {
                        _reproductor.Stop();
                        _reproductor.Close();
                    }
                    catch (Exception ex)
                    {
                        // Se usa Exception aqui para evitar fugas en el Dispose, 
                        // pero se loguea como advertencia.
                        Log.Warn("Excepción durante Dispose de CancionManejador.", ex);
                    }
                }
                _desechado = true;
            }
        }

        private static double ObtenerVolumenGuardado()
        {
            double volumenGuardado = Properties.Settings.Default.volumenCancion;
            if (double.IsNaN(volumenGuardado) || double.IsInfinity(volumenGuardado))
            {
                return VolumenPredeterminado;
            }

            return Math.Max(0, Math.Min(1, volumenGuardado));
        }

        private static void GuardarPreferencia(double volumen)
        {
            Properties.Settings.Default.volumenCancion = volumen;
            Properties.Settings.Default.Save();
        }
    }
}