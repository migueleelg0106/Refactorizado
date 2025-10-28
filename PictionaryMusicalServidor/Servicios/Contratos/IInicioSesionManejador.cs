
using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IInicioSesionManejador
    {
        [OperationContract]
        ResultadoInicioSesionDTO IniciarSesion(CredencialesInicioSesionDTO credenciales);
    }
}
