using System.Collections.Generic;
using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IListaAmigosService
    {
        Task<IReadOnlyList<string>> ObtenerListaAmigosAsync(string nombreUsuario);
    }
}
