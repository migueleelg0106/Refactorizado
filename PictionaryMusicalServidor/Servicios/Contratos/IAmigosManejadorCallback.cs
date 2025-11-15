using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
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
