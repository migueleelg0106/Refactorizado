using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IAmigosManejadorCallback
    {
        [OperationContract(IsOneWay = true)]
        void NotificarSolicitudAmistad(SolicitudAmistadDTO solicitud);

        [OperationContract(IsOneWay = true)]
        void NotificarRespuestaSolicitud(RespuestaSolicitudAmistadDTO respuesta);

        [OperationContract(IsOneWay = true)]
        void NotificarAmistadEliminada(AmistadEliminadaDTO amistadEliminada);
    }
}
