using System;

namespace PictionaryMusicalCliente.Utilidades.Abstracciones
{
    /// <summary>
    /// Define el contrato para el control de reproduccion de audio en el juego.
    /// </summary>
    public interface ICancionManejador : IDisposable
    {
        /// <summary>
        /// Obtiene o establece el volumen de la reproduccion (0.0 a 1.0).
        /// </summary>
        double Volumen { get; set; }

        /// <summary>
        /// Indica si actualmente se esta reproduciendo una cancion.
        /// </summary>
        bool EstaReproduciendo { get; }

        /// <summary>
        /// Reproduce una cancion ubicada en los recursos de la aplicacion.
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo de audio con extension.</param>
        void Reproducir(string nombreArchivo);

        /// <summary>
        /// Detiene la reproduccion actual de audio.
        /// </summary>
        void Detener();
    }

    /// <summary>
    /// Define el contrato para controlar la musica de fondo.
    /// </summary>
    public interface IMusicaManejador : IDisposable
    {
        /// <summary>
        /// Obtiene o establece el nivel de volumen (0.0 a 1.0).
        /// </summary>
        double Volumen { get; set; }

        /// <summary>
        /// Indica si el reproductor esta actualmente emitiendo sonido.
        /// </summary>
        bool EstaReproduciendo { get; }

        /// <summary>
        /// Indica si el volumen esta en cero.
        /// </summary>
        bool EstaSilenciado { get; }

        /// <summary>
        /// Alterna el estado de silencio (mute) del reproductor.
        /// </summary>
        bool AlternarSilencio();

        /// <summary>
        /// Inicia la reproduccion de un archivo de audio en bucle infinito.
        /// </summary>
        void ReproducirEnBucle(string nombreArchivo);

        /// <summary>
        /// Pausa la reproduccion actual.
        /// </summary>
        void Pausar();

        /// <summary>
        /// Reanudar la reproduccion si estaba pausada.
        /// </summary>
        void Reanudar();

        /// <summary>
        /// Detiene completamente la reproduccion y reinicia la posicion.
        /// </summary>
        void Detener();
    }

    /// <summary>
    /// Define el contrato para reproducir efectos de sonido cortos (SFX).
    /// </summary>
    public interface ISonidoManejador
    {
        /// <summary>
        /// Indica si los efectos de sonido estan silenciados por preferencia del usuario.
        /// </summary>
        bool Silenciado { get; set; }

        /// <summary>
        /// Reproduce un archivo de sonido ubicado en la carpeta Recursos.
        /// </summary>
        void ReproducirSonido(string nombreArchivo, double volumen = 1.0);

        /// <summary>
        /// Reproduce el sonido estandar de clic de boton.
        /// </summary>
        void ReproducirClick();

        /// <summary>
        /// Reproduce el sonido estandar de error.
        /// </summary>
        void ReproducirError();

        /// <summary>
        /// Reproduce el sonido estandar de exito o confirmacion.
        /// </summary>
        void ReproducirExito();
    }
}