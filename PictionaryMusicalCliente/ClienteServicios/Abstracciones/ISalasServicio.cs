using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface ISalasServicio : IDisposable
    {
        event EventHandler<string> JugadorSeUnio;
        event EventHandler<string> JugadorSalio;
        event EventHandler<IReadOnlyList<DTOs.SalaDTO>> ListaSalasActualizada;

        Task<DTOs.SalaDTO> CrearSalaAsync(string nombreCreador, DTOs.ConfiguracionPartidaDTO configuracion);

        Task<DTOs.SalaDTO> UnirseSalaAsync(string codigoSala, string nombreUsuario);

        Task AbandonarSalaAsync(string codigoSala, string nombreUsuario);

        Task SuscribirListaSalasAsync();

        Task CancelarSuscripcionListaSalasAsync();

        IReadOnlyList<DTOs.SalaDTO> ListaSalasActual { get; }
    }
}
