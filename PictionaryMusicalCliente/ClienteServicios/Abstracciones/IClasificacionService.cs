using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface IClasificacionService
    {
        Task<IReadOnlyList<DTOs.ClasificacionUsuarioDTO>> ObtenerTopJugadoresAsync();
    }
}
