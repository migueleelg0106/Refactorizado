using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface IListaAmigosServicio : IDisposable
    {
        event EventHandler<IReadOnlyList<DTOs.AmigoDTO>> ListaActualizada;

        IReadOnlyList<DTOs.AmigoDTO> ListaActual { get; }

        Task SuscribirAsync(string nombreUsuario);

        Task CancelarSuscripcionAsync(string nombreUsuario);

        Task<IReadOnlyList<DTOs.AmigoDTO>> ObtenerAmigosAsync(string nombreUsuario);
    }
}
