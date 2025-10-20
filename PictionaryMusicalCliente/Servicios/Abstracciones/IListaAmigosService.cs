using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Servicios;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IListaAmigosService : IDisposable
    {
        event EventHandler<ListaAmigosActualizadaEventArgs> ListaActualizada;

        Task<IReadOnlyList<string>> SuscribirseAsync(string nombreUsuario);

        Task CancelarSuscripcionAsync();
    }
}
