using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IVerificarCodigoDialogService
    {
        Task<DTOs.ResultadoRegistroCuentaDTO> MostrarDialogoAsync(
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionService codigoVerificacionService);
    }
}
