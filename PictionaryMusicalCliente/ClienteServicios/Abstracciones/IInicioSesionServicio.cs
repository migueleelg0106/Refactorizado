using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IInicioSesionServicio
    {
        Task<DTOs.ResultadoInicioSesionDTO> IniciarSesionAsync(DTOs.CredencialesInicioSesionDTO solicitud);
    }
}
