using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IAvatarServicio
    {
        Task<IReadOnlyList<ObjetoAvatar>> ObtenerCatalogoAsync();

        Task<int?> ObtenerIdPorRutaAsync(string rutaRelativa);
    }
}
