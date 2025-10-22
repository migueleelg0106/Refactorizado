using System.Threading.Tasks;
using Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface ICodigoVerificacionService
    {
        Task<ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(NuevaCuentaDTO solicitud);

        Task<ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo);

        Task<ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado);
    }
}
