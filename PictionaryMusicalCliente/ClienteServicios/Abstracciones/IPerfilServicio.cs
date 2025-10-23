using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IPerfilServicio
    {
        Task<DTOs.UsuarioDTO> ObtenerPerfilAsync(int usuarioId);

        Task<DTOs.ResultadoOperacionDTO> ActualizarPerfilAsync(DTOs.ActualizarPerfilDTO solicitud);

        Task<IReadOnlyList<ObjetoAvatar>> ObtenerAvataresDisponiblesAsync();
    }
}
