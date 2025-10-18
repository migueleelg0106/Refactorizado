using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Cuentas;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IPerfilService
    {
        Task<UsuarioAutenticado> ObtenerPerfilAsync(int usuarioId);

        Task<ResultadoOperacion> ActualizarPerfilAsync(ActualizarPerfilSolicitud solicitud);

        Task<IReadOnlyList<ObjetoAvatar>> ObtenerAvataresDisponiblesAsync();
    }
}
