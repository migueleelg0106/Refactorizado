using System;
using System.IO;
using System.Windows.Media;
using log4net;

namespace PictionaryMusicalCliente.Utilidades
{
    /// <summary>
    /// Provee metodos para reproducir efectos de sonido (SFX) cortos, respetando
    /// la preferencia de silencio del usuario.
    /// </summary>
    public class SonidoManejador : IDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const double VolumenPredeterminado = 1.0;
        private bool _desechado;

        /// <summary>
        /// Indica si los efectos de sonido estan silenciados por preferencia del usuario.
        /// </summary>
        public bool Silenciado
        {
            get => Properties.Settings.Default.efectosSilenciados;
            set
            {
                if (Properties.Settings.Default.efectosSilenciados != value)
                {
                    Properties.Settings.Default.efectosSilenciados = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// <summary>
        /// Reproduce el sonido de click estandar.
        /// </summary>
        public void ReproducirClick()
        {
            ReproducirArchivo("piano_boton.MP3");
        }

        /// <summary>
        /// Reproduce un sonido de notificacion o exito.
        /// </summary>
        public void ReproducirNotificacion()
        {
            ReproducirArchivo("exito.MP3");
        }

        /// <summary>
        /// Reproduce un sonido de error.
        /// </summary>
        public void ReproducirError()
        {
            ReproducirArchivo("error.MP3");
        }

        private void ReproducirArchivo(string nombreArchivo)
        {
            if (_desechado || Silenciado)
            {
                return;
            }

            try
            {
                var reproductor = new MediaPlayer();
                string ruta = ObtenerRutaAbsoluta(nombreArchivo);

                reproductor.Open(new Uri(ruta, UriKind.Absolute));
                reproductor.Volume = VolumenPredeterminado;

                reproductor.MediaEnded += (remitente, argumentosEvento) => 
                    LimpiarReproductor(reproductor);
                reproductor.MediaFailed += (remitente, argumentosEvento) => 
                    LimpiarReproductor(reproductor);

                reproductor.Play();
            }
            catch (UriFormatException excepcion)
            {
                _logger.ErrorFormat("La URI del sonido {0} no es valida: {1}", nombreArchivo,
                    excepcion);
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.WarnFormat("Operacion invalida al intentar reproducir {0}: {1}", 
                    nombreArchivo, excepcion);
            }
        }

        private void LimpiarReproductor(MediaPlayer reproductor)
        {
            if (reproductor == null) return;

            try
            {
                reproductor.Stop();
                reproductor.Close();
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Warn("El reproductor no estaba en un estado valido para cerrarse.", excepcion);
            }
        }

        private static string ObtenerRutaAbsoluta(string nombreArchivo)
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Recursos",
                nombreArchivo);
        }

        /// <summary>
        /// Libera los recursos utilizados por la instancia.
        /// </summary>
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

            _desechado = true;
        }
    }
}