using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IAmigosService
    {
        event EventHandler<SolicitudAmistadNotificacion> SolicitudAmistadNotificada;

        event EventHandler<RespuestaSolicitudAmistadNotificacion> SolicitudAmistadRespondidaNotificada;

        event EventHandler<AmistadEliminadaNotificacion> AmistadEliminadaNotificada;

        Task<ResultadoOperacion> EnviarSolicitudAmistadAsync(string nombreUsuarioRemitente, string nombreUsuarioReceptor);

        Task<ResultadoOperacion> ResponderSolicitudAmistadAsync(string nombreUsuarioRemitente, string nombreUsuarioReceptor, bool aceptada);

        Task<ResultadoOperacion> EliminarAmigoAsync(string nombreUsuarioRemitente, string nombreUsuarioReceptor);
    }
}
