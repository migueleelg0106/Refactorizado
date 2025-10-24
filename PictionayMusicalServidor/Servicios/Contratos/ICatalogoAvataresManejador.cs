using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface ICatalogoAvataresManejador
    {
        [OperationContract]
        AvatarDTO[] ObtenerAvataresDisponibles();
    }
}
