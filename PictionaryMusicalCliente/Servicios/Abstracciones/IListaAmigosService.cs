using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Amigos;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IListaAmigosService : IDisposable
    {
        event EventHandler<ListaAmigosResultado> ListaActualizada;

        Task<ListaAmigosResultado> ObtenerListaAmigosAsync(string nombreUsuario);
    }
}
