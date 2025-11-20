using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de perfiles de usuario.
    /// Proporciona operaciones para obtener y actualizar informacion de perfil.
    /// </summary>
    [ServiceContract]
    public interface IPerfilManejador
    {
        /// <summary>
        /// Obtiene el perfil de un usuario especifico.
        /// </summary>
        /// <param name="idUsuario">Identificador unico del usuario.</param>
        /// <returns>Datos del perfil del usuario.</returns>
        [OperationContract]
        UsuarioDTO ObtenerPerfil(int idUsuario);

        /// <summary>
        /// Actualiza el perfil de un usuario.
        /// </summary>
        /// <param name="solicitud">Datos actualizados del perfil del usuario.</param>
        /// <returns>Resultado de la actualizacion del perfil.</returns>
        [OperationContract]
        ResultadoOperacionDTO ActualizarPerfil(ActualizacionPerfilDTO solicitud);
    }
}
