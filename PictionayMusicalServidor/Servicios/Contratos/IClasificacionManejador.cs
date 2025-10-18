using System.Collections.Generic;
using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IClasificacionManejador
    {
        [OperationContract]
        IList<ClasificacionUsuarioDTO> ObtenerTopJugadores();
    }
}
