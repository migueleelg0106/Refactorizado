using System.Collections.Generic;
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de callback para notificaciones de actualizaciones de la lista de amigos.
    /// </summary>
    [ServiceContract]
    public interface IListaAmigosManejadorCallback
    {
        /// <summary>
        /// Notifica al cliente cuando la lista de amigos ha sido actualizada.
        /// </summary>
        /// <param name="amigos">Lista actualizada de amigos.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarListaAmigosActualizada(List<AmigoDTO> amigos);
    }
}
