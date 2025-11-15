using System.Collections.Generic;
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    [ServiceContract]
    public interface IClasificacionManejador
    {
        [OperationContract]
        IList<ClasificacionUsuarioDTO> ObtenerTopJugadores();
    }
}
