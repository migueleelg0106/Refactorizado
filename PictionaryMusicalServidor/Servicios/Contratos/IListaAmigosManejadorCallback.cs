using System.Collections.Generic;
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de callback para notificaciones de cambios en lista de amigos.
    /// Permite al servidor notificar a los clientes sobre actualizaciones en su lista de amigos.
    /// </summary>
    [ServiceContract]
    public interface IListaAmigosManejadorCallback
    {
        /// <summary>
        /// Notifica al cliente sobre actualizaciones en su lista de amigos.
        /// </summary>
        /// <param name="amigos">Lista actualizada de amigos del usuario.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarListaAmigosActualizada(List<AmigoDTO> amigos);
    }
}
