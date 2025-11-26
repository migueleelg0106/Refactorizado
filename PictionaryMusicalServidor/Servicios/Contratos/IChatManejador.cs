using System.ServiceModel;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de chat entre jugadores.
    /// Proporciona operaciones para el sistema de mensajeria del juego.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IChatManejadorCallback))]
    public interface IChatManejador
    {
        /// <summary>
        /// Permite a un jugador unirse al chat de una sala especifica.
        /// Registra el callback del cliente y notifica a los demas participantes.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="nombreJugador">Nombre del jugador que se une.</param>
        [OperationContract]
        void Unirse(string idSala, string nombreJugador);

        /// <summary>
        /// Envia un mensaje a todos los participantes del chat de una sala.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="mensaje">Contenido del mensaje a enviar.</param>
        /// <param name="nombreJugador">Nombre del jugador que envia el mensaje.</param>
        [OperationContract(IsOneWay = true)]
        void EnviarMensaje(string idSala, string mensaje, string nombreJugador);

        /// <summary>
        /// Permite a un jugador salir del chat de una sala.
        /// Elimina el callback del cliente y notifica a los demas participantes.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="nombreJugador">Nombre del jugador que sale.</param>
        [OperationContract]
        void Salir(string idSala, string nombreJugador);
    }
}
