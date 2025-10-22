using System.Threading.Tasks;
using Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IInicioSesionService
    {
        Task<ResultadoInicioSesionDTO> IniciarSesionAsync(CredencialesInicioSesionDTO solicitud);
    }
}
