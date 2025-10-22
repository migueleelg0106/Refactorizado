using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IPerfilService
    {
        Task<DTOs.UsuarioDTO> ObtenerPerfilAsync(int usuarioId);

        Task<DTOs.UsuarioDTO> ActualizarPerfilAsync(DTOs.ActualizarPerfilSolicitudDTO solicitud);

        Task<IReadOnlyList<ObjetoAvatar>> ObtenerAvataresDisponiblesAsync();
    }
}
