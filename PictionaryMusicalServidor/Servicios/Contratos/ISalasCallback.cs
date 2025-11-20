using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de callback para notificaciones de eventos en salas de juego.
    /// Permite al servidor notificar a los clientes sobre cambios en salas.
    /// </summary>
    [ServiceContract]
    public interface ISalasCallback
    {
        /// <summary>
        /// Notifica al cliente cuando un jugador se une a una sala.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <param name="nombreJugador">Nombre del jugador que se unio.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarJugadorSeUnio(string codigoSala, string nombreJugador);

        /// <summary>
        /// Notifica al cliente cuando un jugador sale de una sala.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <param name="nombreJugador">Nombre del jugador que salio.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarJugadorSalio(string codigoSala, string nombreJugador);

        /// <summary>
        /// Notifica al cliente sobre actualizaciones en la lista completa de salas disponibles.
        /// </summary>
        /// <param name="salas">Arreglo de salas actualizadas.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarListaSalasActualizada(DTOs.SalaDTO[] salas);

        /// <summary>
        /// Notifica al cliente sobre actualizaciones en una sala especifica.
        /// </summary>
        /// <param name="sala">Datos actualizados de la sala.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarSalaActualizada(DTOs.SalaDTO sala);

        /// <summary>
        /// Notifica al cliente cuando un jugador es expulsado de una sala.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <param name="nombreJugador">Nombre del jugador expulsado.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarJugadorExpulsado(string codigoSala, string nombreJugador);
    }
}