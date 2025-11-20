using System.Collections.Generic;
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la consulta y actualizacion de la lista de amigos.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IListaAmigosManejadorCallback))]
    public interface IListaAmigosManejador
    {
        /// <summary>
        /// Suscribe a un usuario para recibir actualizaciones de su lista de amigos.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario a suscribir.</param>
        [OperationContract]
        void Suscribir(string nombreUsuario);

        /// <summary>
        /// Cancela la suscripcion de un usuario para dejar de recibir actualizaciones.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario que cancela su suscripcion.</param>
        [OperationContract]
        void CancelarSuscripcion(string nombreUsuario);

        /// <summary>
        /// Obtiene la lista de amigos de un usuario.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        /// <returns>Lista de amigos del usuario.</returns>
        [OperationContract]
        List<AmigoDTO> ObtenerAmigos(string nombreUsuario);
    }
}
