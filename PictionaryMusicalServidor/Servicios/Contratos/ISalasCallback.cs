using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de callback para notificaciones de actualizaciones en salas.
    /// </summary>
    [ServiceContract]
    public interface ISalasCallback
    {
        /// <summary>
        /// Notifica al cliente cuando un jugador se une a una sala.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala.</param>
        /// <param name="nombreJugador">Nombre del jugador que se unio.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarJugadorSeUnio(string codigoSala, string nombreJugador);

        /// <summary>
        /// Notifica al cliente cuando un jugador sale de una sala.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala.</param>
        /// <param name="nombreJugador">Nombre del jugador que salio.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarJugadorSalio(string codigoSala, string nombreJugador);

        /// <summary>
        /// Notifica al cliente cuando la lista de salas ha sido actualizada.
        /// </summary>
        /// <param name="salas">Lista actualizada de salas.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarListaSalasActualizada(DTOs.SalaDTO[] salas);

        /// <summary>
        /// Notifica al cliente cuando una sala especifica ha sido actualizada.
        /// </summary>
        /// <param name="sala">Informacion actualizada de la sala.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarSalaActualizada(DTOs.SalaDTO sala);

        /// <summary>
        /// Notifica al cliente cuando un jugador ha sido expulsado de una sala.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala.</param>
        /// <param name="nombreJugador">Nombre del jugador expulsado.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarJugadorExpulsado(string codigoSala, string nombreJugador);
    }
}