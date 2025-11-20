using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Administra la informacion personal y configuracion del perfil del usuario.
    /// </summary>
    public interface IPerfilServicio
    {
        /// <summary>
        /// Obtiene la informacion detallada del perfil de un usuario especifico.
        /// </summary>
        /// <param name="usuarioId">Identificador unico del usuario.</param>
        /// <returns>DTO con los datos del perfil.</returns>
        Task<DTOs.UsuarioDTO> ObtenerPerfilAsync(int usuarioId);

        /// <summary>
        /// Envia los datos modificados del perfil para su persistencia en el servidor.
        /// </summary>
        /// <param name="solicitud">Objeto con los cambios a aplicar.</param>
        /// <returns>Resultado de la operacion de actualizacion.</returns>
        Task<DTOs.ResultadoOperacionDTO> ActualizarPerfilAsync(
            DTOs.ActualizacionPerfilDTO solicitud);
    }
}