using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Amigos;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IListaAmigosService : IDisposable
    {
        event EventHandler<IReadOnlyList<Amigo>> ListaActualizada;

        IReadOnlyList<Amigo> ListaActual { get; }

        Task SuscribirAsync(string nombreUsuario);

        Task CancelarSuscripcionAsync(string nombreUsuario);
    }
}
