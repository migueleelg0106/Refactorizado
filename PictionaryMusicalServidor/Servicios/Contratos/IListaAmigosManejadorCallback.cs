using System.Collections.Generic;
using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IListaAmigosManejadorCallback
    {
        [OperationContract(IsOneWay = true)]
        void NotificarListaAmigosActualizada(List<AmigoDTO> amigos);
    }
}
