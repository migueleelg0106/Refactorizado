using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface ICambioContrasenaService
    {
        Task<ResultadoSolicitudRecuperacionDTO> SolicitarCodigoRecuperacionAsync(string identificador);

        Task<ResultadoSolicitudCodigoDTO> ReenviarCodigoRecuperacionAsync(string tokenCodigo);

        Task<ResultadoOperacion> ConfirmarCodigoRecuperacionAsync(string tokenCodigo, string codigoIngresado);

        Task<ResultadoOperacion> ActualizarContrasenaAsync(string tokenCodigo, string nuevaContrasena);
    }
}
