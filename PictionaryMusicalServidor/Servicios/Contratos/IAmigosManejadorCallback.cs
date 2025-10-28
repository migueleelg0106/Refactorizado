using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
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
