using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IAmigosCallback
    {
        [OperationContract(IsOneWay = true)]
        void SolicitudRecibida(SolicitudAmistadNotificacionDTO solicitud);

        [OperationContract(IsOneWay = true)]
        void SolicitudActualizada(SolicitudAmistadEstadoDTO solicitud);

        [OperationContract(IsOneWay = true)]
        void AmigoAgregado(AmigoDTO amigo);

        [OperationContract(IsOneWay = true)]
        void AmistadEliminada(AmistadEliminadaDTO amistadEliminada);
    }
}
