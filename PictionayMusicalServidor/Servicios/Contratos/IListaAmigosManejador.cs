using System.Collections.Generic;
using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract(CallbackContract = typeof(IAmigosCallback))]
    public interface IListaAmigosManejador
    {
        [OperationContract]
        IList<AmigoDTO> ObtenerListaAmigos(int jugadorId);
    }
}
