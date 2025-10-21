using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IAmigosManejadorCallback
    {
        [OperationContract(IsOneWay = true)]
        void SolicitudActualizada(SolicitudAmistadDTO solicitud);

        [OperationContract(IsOneWay = true)]
        void AmistadEliminada(SolicitudAmistadDTO solicitud);
    }
}
