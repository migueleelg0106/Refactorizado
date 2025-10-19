using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract(CallbackContract = typeof(IAmigosManejadorCallback))]
    public interface IAmigosManejador
    {
        [OperationContract]
        ResultadoOperacionDTO EnviarSolicitudAmistad(string nombreUsuarioRemitente, string nombreUsuarioReceptor);

        [OperationContract]
        ResultadoOperacionDTO ResponderSolicitudAmistad(string nombreUsuarioRemitente, string nombreUsuarioReceptor, bool aceptada);

        [OperationContract]
        ResultadoOperacionDTO EliminarAmigo(string nombreUsuarioRemitente, string nombreUsuarioReceptor);
    }

    [ServiceContract]
    public interface IAmigosManejadorCallback
    {
        [OperationContract(IsOneWay = true)]
        void SolicitudAmistadRecibida(SolicitudAmistadNotificacionDTO notificacion);

        [OperationContract(IsOneWay = true)]
        void SolicitudAmistadRespondida(RespuestaSolicitudAmistadNotificacionDTO notificacion);

        [OperationContract(IsOneWay = true)]
        void AmistadEliminada(AmistadEliminadaNotificacionDTO notificacion);
    }
}
