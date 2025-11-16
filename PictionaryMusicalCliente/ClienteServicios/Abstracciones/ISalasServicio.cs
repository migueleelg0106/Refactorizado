using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface ISalasServicio : IDisposable
    {
        event EventHandler<string> JugadorSeUnio;
        event EventHandler<string> JugadorSalio;
        event EventHandler<string> JugadorExpulsado;
        event EventHandler<IReadOnlyList<DTOs.SalaDTO>> ListaSalasActualizada;
        event EventHandler<DTOs.SalaDTO> SalaActualizada;

        Task<DTOs.SalaDTO> CrearSalaAsync(string nombreCreador, DTOs.ConfiguracionPartidaDTO configuracion);

        Task<DTOs.SalaDTO> UnirseSalaAsync(string codigoSala, string nombreUsuario);

        Task AbandonarSalaAsync(string codigoSala, string nombreUsuario);

        Task ExpulsarJugadorAsync(string codigoSala, string nombreHost, string nombreJugadorAExpulsar);

        Task SuscribirListaSalasAsync();

        Task CancelarSuscripcionListaSalasAsync();

        IReadOnlyList<DTOs.SalaDTO> ListaSalasActual { get; }
    }
}
