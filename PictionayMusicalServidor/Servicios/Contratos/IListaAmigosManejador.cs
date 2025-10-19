using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract(CallbackContract = typeof(IListaAmigosManejadorCallback))]
    public interface IListaAmigosManejador
    {
        [OperationContract]
        ListaAmigosDTO ObtenerListaAmigos(string nombreUsuario);
    }

    [ServiceContract]
    public interface IListaAmigosManejadorCallback
    {
        [OperationContract(IsOneWay = true)]
        void ListaAmigosActualizada(ListaAmigosDTO lista);
    }
}
