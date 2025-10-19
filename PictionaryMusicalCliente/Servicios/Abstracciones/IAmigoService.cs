using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IAmigosService
    {
        event EventHandler<SolicitudAmistadNotificacion> SolicitudAmistadRecibida;

        event EventHandler<RespuestaSolicitudAmistadNotificacion> SolicitudAmistadRespondida;

        event EventHandler<AmistadEliminadaNotificacion> AmistadEliminada;

        Task<ResultadoOperacion> EnviarSolicitudAsync(string nombreUsuarioReceptor);

        Task<ResultadoOperacion> ResponderSolicitudAsync(string nombreUsuarioRemitente, bool aceptada);

        Task<ResultadoOperacion> EliminarAmigoAsync(string nombreUsuarioAmigo);
    }
}
