using System.ServiceModel;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion del curso de una partida en progreso.
    /// Proporciona operaciones para el control del flujo de juego durante una partida activa.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(ICursoPartidaCallback))]
    public interface ICursoPartidaManejador
    {
        /// <summary>
        /// Registra un jugador en el curso de una partida para recibir notificaciones de juego.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala de juego.</param>
        /// <param name="nombreJugador">Nombre del jugador a registrar.</param>
        [OperationContract]
        void RegistrarEnPartida(string codigoSala, string nombreJugador);

        /// <summary>
        /// Envia un mensaje de juego que puede ser un intento de adivinanza o mensaje de chat.
        /// El servidor valida si el mensaje es una adivinanza correcta.
        /// </summary>
        /// <param name="mensaje">Contenido del mensaje.</param>
        /// <param name="codigoSala">Codigo de la sala donde se envia el mensaje.</param>
        [OperationContract]
        void EnviarMensajeJuego(string mensaje, string codigoSala);

        /// <summary>
        /// Desregistra un jugador del curso de la partida.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala de juego.</param>
        /// <param name="nombreJugador">Nombre del jugador a desregistrar.</param>
        [OperationContract]
        void DesregistrarDePartida(string codigoSala, string nombreJugador);
    }
}
