using System.Threading.Tasks;
using DTOs = Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface IInvitacionesServicio
    {
        Task<DTOs.ResultadoOperacionDTO> EnviarInvitacionAsync(string codigoSala, string correoDestino);
    }
}
