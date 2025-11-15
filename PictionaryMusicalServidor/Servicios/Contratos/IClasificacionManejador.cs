using System.Collections.Generic;
using System.ServiceModel;
using PictionaryMusical.Servicios.Contratos.DTOs;

namespace PictionaryMusical.Servicios.Contratos
{
    [ServiceContract]
    public interface IClasificacionManejador
    {
        [OperationContract]
        IList<ClasificacionUsuarioDTO> ObtenerTopJugadores();
    }
}
