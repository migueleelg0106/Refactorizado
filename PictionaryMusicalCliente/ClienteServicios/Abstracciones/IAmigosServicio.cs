using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface IAmigosServicio : IDisposable
    {
        event EventHandler<IReadOnlyCollection<DTOs.SolicitudAmistadDTO>> SolicitudesActualizadas;

        IReadOnlyCollection<DTOs.SolicitudAmistadDTO> SolicitudesPendientes { get; }

        Task SuscribirAsync(string nombreUsuario);

        Task CancelarSuscripcionAsync(string nombreUsuario);

        Task EnviarSolicitudAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor);

        Task ResponderSolicitudAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor);

        Task EliminarAmigoAsync(string nombreUsuarioA, string nombreUsuarioB);
    }
}
