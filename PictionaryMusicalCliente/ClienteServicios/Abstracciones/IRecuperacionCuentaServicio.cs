using System.Threading.Tasks;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IRecuperacionCuentaServicio
    {
        Task<DTOs.ResultadoOperacionDTO> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaServicio cambioContrasenaServicio);
    }
}
