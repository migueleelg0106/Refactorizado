using System.Collections.Generic;
using System.ServiceModel;
using PictionaryMusical.Servicios.Contratos.DTOs;

namespace PictionaryMusical.Servicios.Contratos
{
    [ServiceContract]
    public interface IListaAmigosManejadorCallback
    {
        [OperationContract(IsOneWay = true)]
        void NotificarListaAmigosActualizada(List<AmigoDTO> amigos);
    }
}
