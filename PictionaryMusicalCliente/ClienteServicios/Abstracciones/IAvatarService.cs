using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IAvatarService
    {
        Task<IReadOnlyList<ObjetoAvatar>> ObtenerCatalogoAsync();

        Task<int?> ObtenerIdPorRutaAsync(string rutaRelativa);
    }
}
