using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Mapeo;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IPerfilService
    {
        Task<DTOs.UsuarioDTO> ObtenerPerfilAsync(int usuarioId);

        Task<DTOs.ResultadoOperacionDTO> ActualizarPerfilAsync(ActualizarPerfilSolicitud solicitud);

        Task<IReadOnlyList<ObjetoAvatar>> ObtenerAvataresDisponiblesAsync();
    }
}
