using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface ICambioContrasenaServicio
    {
        Task<DTOs.ResultadoSolicitudRecuperacionDTO> SolicitarCodigoRecuperacionAsync(string identificador);

        Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRecuperacionAsync(string tokenCodigo);

        Task<DTOs.ResultadoOperacionDTO> ConfirmarCodigoRecuperacionAsync(string tokenCodigo, string codigoIngresado);

        Task<DTOs.ResultadoOperacionDTO> ActualizarContrasenaAsync(string tokenCodigo, string nuevaContrasena);
    }
}
