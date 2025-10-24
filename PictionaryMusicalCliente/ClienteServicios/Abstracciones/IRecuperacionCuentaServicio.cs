using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IRecuperacionCuentaServicio
    {
        Task<DTOs.ResultadoOperacionDTO> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaServicio cambioContrasenaServicio);
    }
}
