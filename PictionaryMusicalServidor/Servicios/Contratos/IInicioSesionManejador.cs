
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para el inicio de sesion de usuarios.
    /// </summary>
    [ServiceContract]
    public interface IInicioSesionManejador
    {
        /// <summary>
        /// Inicia sesion con las credenciales proporcionadas.
        /// </summary>
        /// <param name="credenciales">Credenciales del usuario para autenticacion.</param>
        /// <returns>Resultado del intento de inicio de sesion.</returns>
        [OperationContract]
        ResultadoInicioSesionDTO IniciarSesion(CredencialesInicioSesionDTO credenciales);
    }
}
