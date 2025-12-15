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
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const double VolumenPredeterminado = 0.5;
        private readonly MediaPlayer _reproductor;
        private bool _desechado;

        /// <summary>
        /// Inicializa una nueva instancia del manejador de canciones.
        /// </summary>
        public CancionManejador()
        {
            _reproductor = new MediaPlayer();
            _reproductor.MediaEnded += (remitente, argumentosEvento) => EstaReproduciendo = false;
            _reproductor.Volume = ObtenerVolumenGuardado();
        }

        /// <summary>
        /// Obtiene o establece el volumen de la reproduccion (0.0 a 1.0).
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
                GuardarPreferencia(ajustado);
            }
        }

        /// <summary>
        /// Indica si actualmente se esta reproduciendo una cancion.
        /// </summary>
        public bool EstaReproduciendo { get; private set; }

        /// <summary>
        /// Reproduce una cancion ubicada en la carpeta 'Recursos'.
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo con extension.</param>
        public void Reproducir(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                _logger.Warn("Intento de reproducir archivo con nombre vacio.");
                return;
            }

            try
            {
                string ruta = ConstruirRutaArchivo(nombreArchivo);

                if (File.Exists(ruta))
                {
                    IniciarReproduccion(ruta, nombreArchivo);
                }
                else
                {
                    _logger.ErrorFormat("Archivo de audio no encontrado: {0}", ruta);
                }
            }
            catch (Exception excepcion)
            {
                ManejarExcepcionReproduccion(excepcion, nombreArchivo);
            }
        }

        /// <summary>
        /// Detiene la reproduccion actual.
        /// </summary>
        public void Detener()
        {
            try
            {
                _reproductor.Stop();
                EstaReproduciendo = false;
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Error("Error al intentar detener la reproduccion.", excepcion);
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
            if (_desechado)
            {
                return;
            }

            if (disposing)
            {
                try
                {
                    _reproductor.Stop();
                    _reproductor.Close();
                }
                catch (Exception excepcion)
                {
                    // Se usa Exception aqui para evitar fugas en el Dispose, 
                    // pero se loguea como advertencia.
                    _logger.Warn("Excepcion durante Dispose de CancionManejador.", excepcion);
                }
            }
            _desechado = true;
        }

        private static string ConstruirRutaArchivo(string nombreArchivo)
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Recursos",
                "Canciones",
                nombreArchivo);
        }

        private void IniciarReproduccion(string ruta, string nombre)
        {
            _reproductor.Open(new Uri(ruta, UriKind.Absolute));
            _reproductor.Play();
            EstaReproduciendo = true;
            _logger.InfoFormat("Reproduciendo: {0}", nombre);
        }

        private static void ManejarExcepcionReproduccion(Exception excepcion, string nombre)
        {
            if (excepcion is UriFormatException)
            {
                _logger.ErrorFormat("URI invalido para cancion: {0}", nombre);
            }
            else if (excepcion is InvalidOperationException)
            {
                _logger.ErrorFormat("Error de operacion en reproductor para: {0}", nombre);
            }
            else
            {
                _logger.ErrorFormat("Error inesperado reproduciendo {0}: {1}", nombre, excepcion);
            }
        }

        private static double ObtenerVolumenGuardado()
        {
            double valor = Properties.Settings.Default.volumenCancion;
            if (double.IsNaN(valor) || double.IsInfinity(valor))
            {
                return VolumenPredeterminado;
            }
            return Math.Max(0, Math.Min(1, valor));
        }

        private static void GuardarPreferencia(double volumen)
        {
            Properties.Settings.Default.volumenCancion = volumen;
            Properties.Settings.Default.Save();
        }
    }
}