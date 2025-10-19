using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Amigos;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IListaAmigosService
    {
        event EventHandler<ListaAmigosActualizadaEventArgs> ListaActualizada;

        Task<ListaAmigosResultado> ObtenerListaAmigosAsync();
    }
}
