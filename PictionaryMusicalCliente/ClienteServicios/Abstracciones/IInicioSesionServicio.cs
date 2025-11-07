using System.Threading.Tasks;
using DTOs = Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    public interface IInicioSesionServicio
    {
        Task<DTOs.ResultadoInicioSesionDTO> IniciarSesionAsync(DTOs.CredencialesInicioSesionDTO solicitud);
    }
}
