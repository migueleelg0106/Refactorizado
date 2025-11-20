using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de callback para notificaciones de eventos de amistad.
    /// Permite al servidor notificar a los clientes sobre cambios en solicitudes de amistad.
    /// </summary>
    [ServiceContract]
    public interface IAmigosManejadorCallback
    {
        /// <summary>
        /// Notifica al cliente sobre actualizaciones en una solicitud de amistad.
        /// </summary>
        /// <param name="solicitud">Datos de la solicitud de amistad actualizada.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarSolicitudActualizada(SolicitudAmistadDTO solicitud);

        /// <summary>
        /// Notifica al cliente sobre la eliminacion de una amistad.
        /// </summary>
        /// <param name="solicitud">Datos de la amistad eliminada.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarAmistadEliminada(SolicitudAmistadDTO solicitud);
    }
}
