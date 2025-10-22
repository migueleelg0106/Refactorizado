using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IRecuperacionCuentaDialogService
    {
        Task<DTOs.ResultadoOperacionDTO> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaService cambioContrasenaService);
    }
}
