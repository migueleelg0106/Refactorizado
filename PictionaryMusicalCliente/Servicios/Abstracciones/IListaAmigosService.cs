using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Amigos;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IListaAmigosService
    {
        Task<IReadOnlyList<Amigo>> ObtenerAmigosAsync(int jugadorId);
    }
}
