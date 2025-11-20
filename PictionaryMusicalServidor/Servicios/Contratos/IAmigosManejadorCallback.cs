using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de callback para notificaciones de actualizaciones de amistad.
    /// </summary>
    [ServiceContract]
    public interface IAmigosManejadorCallback
    {
        /// <summary>
        /// Notifica al cliente cuando una solicitud de amistad ha sido actualizada.
        /// </summary>
        /// <param name="solicitud">Informacion de la solicitud de amistad actualizada.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarSolicitudActualizada(SolicitudAmistadDTO solicitud);

        /// <summary>
        /// Notifica al cliente cuando una amistad ha sido eliminada.
        /// </summary>
        /// <param name="solicitud">Informacion de la amistad eliminada.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarAmistadEliminada(SolicitudAmistadDTO solicitud);
    }
}
