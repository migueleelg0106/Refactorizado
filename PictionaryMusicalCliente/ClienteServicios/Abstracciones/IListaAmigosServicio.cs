using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IListaAmigosServicio : IDisposable
    {
        event EventHandler<IReadOnlyList<DTOs.AmigoDTO>> ListaActualizada;

        IReadOnlyList<DTOs.AmigoDTO> ListaActual { get; }

        Task SuscribirAsync(string nombreUsuario);

        Task CancelarSuscripcionAsync(string nombreUsuario);
    }
}
