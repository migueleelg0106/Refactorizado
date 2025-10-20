using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IAmigosService
    {
        event EventHandler<SolicitudAmistadEventArgs> SolicitudRecibida;

        event EventHandler<RespuestaSolicitudAmistadEventArgs> SolicitudRespondida;

        event EventHandler<AmistadEliminadaEventArgs> AmistadEliminada;

        Task SuscribirseAsync(string nombreUsuario);

        Task DesuscribirseAsync(string nombreUsuario);

        Task<ResultadoOperacion> EnviarSolicitudAmistadAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor);

        Task<ResultadoOperacion> ResponderSolicitudAmistadAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor, bool aceptarSolicitud);

        Task<ResultadoOperacion> EliminarAmigoAsync(string nombreUsuarioA, string nombreUsuarioB);

        IReadOnlyCollection<string> ObtenerAmigos();

        IReadOnlyCollection<SolicitudAmistad> ObtenerSolicitudesPendientes();
    }
}
