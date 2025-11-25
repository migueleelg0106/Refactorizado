using System.ServiceModel;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de callback para notificaciones del curso de partida.
    /// Permite al servidor notificar a los clientes sobre eventos del juego.
    /// </summary>
    [ServiceContract]
    public interface ICursoPartidaCallback
    {
        /// <summary>
        /// Notifica al cliente que un jugador ha adivinado correctamente la cancion.
        /// No se revela el mensaje original para evitar spoilers.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador que adivino.</param>
        /// <param name="puntaje">Puntos obtenidos por adivinar.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarAcierto(string nombreJugador, int puntaje);

        /// <summary>
        /// Envia un mensaje de chat normal a todos los jugadores de la sala.
        /// Se usa para mensajes que no son adivinanzas correctas.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador que envio el mensaje.</param>
        /// <param name="mensaje">Contenido del mensaje.</param>
        [OperationContract(IsOneWay = true)]
        void RecibirMensajeChat(string nombreJugador, string mensaje);

        /// <summary>
        /// Notifica al cliente que la ronda ha terminado.
        /// </summary>
        /// <param name="nombreCancion">Nombre de la cancion que se debia adivinar.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarFinRonda(string nombreCancion);

        /// <summary>
        /// Notifica al cliente que la partida ha terminado.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotificarFinPartida();
    }
}
