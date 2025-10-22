using System.Threading.Tasks;
using Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface ICuentaService
    {
        Task<ResultadoRegistroCuentaDTO> RegistrarCuentaAsync(NuevaCuentaDTO solicitud);
    }
}
