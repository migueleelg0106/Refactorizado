using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract(CallbackContract = typeof(IAmigosManejadorCallback))]
    public interface IAmigosManejador
    {
        [OperationContract]
        void Suscribirse(string nombreUsuario);

        [OperationContract]
        void Desuscribirse(string nombreUsuario);

        [OperationContract]
        ResultadoOperacionDTO EnviarSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor);

        [OperationContract]
        ResultadoOperacionDTO ResponderSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor, bool aceptarSolicitud);

        [OperationContract]
        ResultadoOperacionDTO EliminarAmigo(string nombreUsuarioA, string nombreUsuarioB);
    }
}
