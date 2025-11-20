using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de gestion de perfiles de usuario.
    /// </summary>
    public interface IPerfilServicio
    {
        /// <summary>
        /// Obtiene el perfil de un usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <returns>Datos del perfil del usuario.</returns>
        Task<DTOs.UsuarioDTO> ObtenerPerfilAsync(int usuarioId);

        /// <summary>
        /// Actualiza la informacion del perfil de un usuario.
        /// </summary>
        /// <param name="solicitud">Datos actualizados del perfil.</param>
        /// <returns>Resultado de la actualizacion.</returns>
        Task<DTOs.ResultadoOperacionDTO> ActualizarPerfilAsync(DTOs.ActualizacionPerfilDTO solicitud);
    }
}
