using System.Collections.Generic;
using System.Threading.Tasks;
using ClasificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioClasificacion;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IClasificacionService
    {
        Task<IReadOnlyList<ClasificacionSrv.ClasificacionUsuarioDTO>> ObtenerTopJugadoresAsync();
    }
}
