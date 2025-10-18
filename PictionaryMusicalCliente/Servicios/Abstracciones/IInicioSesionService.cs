using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Cuentas;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IInicioSesionService
    {
        Task<ResultadoInicioSesion> IniciarSesionAsync(SolicitudInicioSesion solicitud);
    }
}
