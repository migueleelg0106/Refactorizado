using System.Collections.Generic;
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de listas de amigos.
    /// Proporciona operaciones para suscribirse y obtener listas de amigos con soporte de callbacks.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IListaAmigosManejadorCallback))]
    public interface IListaAmigosManejador
    {
        /// <summary>
        /// Suscribe un usuario para recibir notificaciones sobre cambios en su lista de amigos.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario a suscribir.</param>
        [OperationContract]
        void Suscribir(string nombreUsuario);

        /// <summary>
        /// Cancela la suscripcion de un usuario de notificaciones de lista de amigos.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario que cancela la suscripcion.</param>
        [OperationContract]
        void CancelarSuscripcion(string nombreUsuario);

        /// <summary>
        /// Obtiene la lista de amigos de un usuario especifico.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario cuya lista de amigos se desea obtener.</param>
        /// <returns>Lista de amigos del usuario.</returns>
        [OperationContract]
        List<AmigoDTO> ObtenerAmigos(string nombreUsuario);
    }
}
