using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IAmigosService
    {
        Task<ResultadoOperacion> EnviarSolicitudAsync(string nombreUsuarioRemitente, string nombreUsuarioReceptor);

        Task<ResultadoOperacion> EliminarAmigoAsync(string nombreUsuario, string nombreAmigo);
    }
}
