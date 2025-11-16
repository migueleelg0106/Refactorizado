using System.Threading.Tasks;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface IRecuperacionCuentaServicio
    {
        Task<DTOs.ResultadoOperacionDTO> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaServicio cambioContrasenaServicio);
    }
}
