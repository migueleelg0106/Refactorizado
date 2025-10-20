using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IAmigosService
    {
        Task<ResultadoOperacion> EnviarSolicitudAmistadAsync(
            string nombreUsuarioRemitente,
            string nombreUsuarioReceptor);

        Task<ResultadoOperacion> EliminarAmigoAsync(
            string nombreUsuarioRemitente,
            string nombreUsuarioReceptor);
    }
}
