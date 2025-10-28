using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IVerificacionCodigoDialogoServicio
    {
        Task<DTOs.ResultadoRegistroCuentaDTO> MostrarDialogoAsync(
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionServicio codigoVerificacionServicio);
    }
}
