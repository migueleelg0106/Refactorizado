using System.ServiceModel;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de callback para notificaciones de chat a los clientes.
    /// Permite al servidor notificar a los clientes sobre mensajes y eventos del chat.
    /// </summary>
    [ServiceContract]
    public interface IChatManejadorCallback
    {
        /// <summary>
        /// Notifica al cliente que un nuevo mensaje de chat ha sido recibido.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador que envio el mensaje.</param>
        /// <param name="mensaje">Contenido del mensaje.</param>
        [OperationContract(IsOneWay = true)]
        void RecibirMensaje(string nombreJugador, string mensaje);

        /// <summary>
        /// Notifica al cliente que un jugador se unio al chat de la sala.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador que se unio.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarJugadorUnido(string nombreJugador);

        /// <summary>
        /// Notifica al cliente que un jugador salio del chat de la sala.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador que salio.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarJugadorSalio(string nombreJugador);
    }
}
