using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IAmigosService
    {
        Task<ResultadoOperacion> EnviarSolicitudAmistadAsync(SolicitudAmistad solicitud);

        Task<ResultadoOperacion> EliminarAmigoAsync(OperacionAmistad solicitud);
    }
}
