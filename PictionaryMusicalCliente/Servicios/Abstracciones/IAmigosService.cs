using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Amigos;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IAmigosService : IDisposable
    {
        event EventHandler<IReadOnlyCollection<SolicitudAmistad>> SolicitudesActualizadas;

        IReadOnlyCollection<SolicitudAmistad> SolicitudesPendientes { get; }

        Task SuscribirAsync(string nombreUsuario);

        Task CancelarSuscripcionAsync(string nombreUsuario);

        Task EnviarSolicitudAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor);

        Task ResponderSolicitudAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor);

        Task EliminarAmigoAsync(string nombreUsuarioA, string nombreUsuarioB);
    }
}
