using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Servicios;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IAmigosService
    {
        event EventHandler<SolicitudAmistadRecibidaEventArgs> SolicitudRecibida;

        event EventHandler<RespuestaSolicitudAmistadEventArgs> SolicitudRespondida;

        event EventHandler<AmistadEliminadaEventArgs> AmistadEliminada;

        Task<ResultadoOperacion> EnviarSolicitudAsync(string nombreUsuarioRemitente, string nombreUsuarioReceptor);

        Task<ResultadoOperacion> EliminarAmigoAsync(string nombreUsuario, string nombreAmigo);
    }
}
