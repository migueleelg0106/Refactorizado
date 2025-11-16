using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface IInvitacionesServicio
    {
        Task<DTOs.ResultadoOperacionDTO> EnviarInvitacionAsync(string codigoSala, string correoDestino);
    }
}
