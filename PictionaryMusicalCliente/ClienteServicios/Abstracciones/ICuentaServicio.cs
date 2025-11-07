using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface ICuentaServicio
    {
        Task<DTOs.ResultadoRegistroCuentaDTO> RegistrarCuentaAsync(DTOs.NuevaCuentaDTO solicitud);
    }
}
