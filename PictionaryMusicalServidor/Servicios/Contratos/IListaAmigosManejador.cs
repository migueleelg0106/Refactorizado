using System.Collections.Generic;
using System.ServiceModel;
using PictionaryMusical.Servicios.Contratos.DTOs;

namespace PictionaryMusical.Servicios.Contratos
{
    [ServiceContract(CallbackContract = typeof(IListaAmigosManejadorCallback))]
    public interface IListaAmigosManejador
    {
        [OperationContract]
        void Suscribir(string nombreUsuario);

        [OperationContract]
        void CancelarSuscripcion(string nombreUsuario);

        [OperationContract]
        List<AmigoDTO> ObtenerAmigos(string nombreUsuario);
    }
}
