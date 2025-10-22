using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IPerfilService
    {
        Task<UsuarioAutenticado> ObtenerPerfilAsync(int usuarioId);

        Task<ResultadoOperacion> ActualizarPerfilAsync(ActualizarPerfilDTO solicitud);

        Task<IReadOnlyList<ObjetoAvatar>> ObtenerAvataresDisponiblesAsync();
    }
}
