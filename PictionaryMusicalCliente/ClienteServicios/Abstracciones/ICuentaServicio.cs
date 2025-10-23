using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface ICuentaServicio
    {
        Task<DTOs.ResultadoRegistroCuentaDTO> RegistrarCuentaAsync(DTOs.NuevaCuentaDTO solicitud);
    }
}
