
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    [ServiceContract]
    public interface IInicioSesionManejador
    {
        [OperationContract]
        ResultadoInicioSesionDTO IniciarSesion(CredencialesInicioSesionDTO credenciales);
    }
}
