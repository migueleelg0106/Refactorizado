using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract(CallbackContract = typeof(IAmigosCallback))]
    public interface IAmigosManejador
    {
        [OperationContract]
        ResultadoOperacionDTO EnviarSolicitudAmistad(SolicitudAmistadDTO solicitud);

        [OperationContract]
        ResultadoOperacionDTO ResponderSolicitudAmistad(RespuestaSolicitudAmistadDTO respuesta);

        [OperationContract]
        ResultadoOperacionDTO EliminarAmigo(OperacionAmistadDTO solicitud);
    }
}
