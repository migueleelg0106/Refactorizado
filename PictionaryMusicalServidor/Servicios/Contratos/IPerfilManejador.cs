using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de perfiles de usuario.
    /// </summary>
    [ServiceContract]
    public interface IPerfilManejador
    {
        /// <summary>
        /// Obtiene el perfil de un usuario.
        /// </summary>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <returns>Datos del perfil del usuario.</returns>
        [OperationContract]
        UsuarioDTO ObtenerPerfil(int idUsuario);

        /// <summary>
        /// Actualiza la informacion del perfil de un usuario.
        /// </summary>
        /// <param name="solicitud">Datos actualizados del perfil.</param>
        /// <returns>Resultado de la actualizacion del perfil.</returns>
        [OperationContract]
        ResultadoOperacionDTO ActualizarPerfil(ActualizacionPerfilDTO solicitud);
    }
}
