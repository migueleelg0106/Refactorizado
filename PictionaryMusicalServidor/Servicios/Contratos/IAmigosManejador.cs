using System.ServiceModel;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    [ServiceContract(CallbackContract = typeof(IAmigosManejadorCallback))]
    public interface IAmigosManejador
    {
        [OperationContract]
        void Suscribir(string nombreUsuario);

        [OperationContract]
        void CancelarSuscripcion(string nombreUsuario);

        [OperationContract]
        void EnviarSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor);

        [OperationContract]
        void ResponderSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor);

        [OperationContract]
        void EliminarAmigo(string nombreUsuarioA, string nombreUsuarioB);
    }
}
