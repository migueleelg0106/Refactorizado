using System.ServiceModel;
using PictionaryMusical.Servicios.Contratos.DTOs;

namespace PictionaryMusical.Servicios.Contratos
{
    [ServiceContract]
    public interface IAmigosManejadorCallback
    {
        [OperationContract(IsOneWay = true)]
        void NotificarSolicitudActualizada(SolicitudAmistadDTO solicitud);

        [OperationContract(IsOneWay = true)]
        void NotificarAmistadEliminada(SolicitudAmistadDTO solicitud);
    }
}
