
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la autenticacion de usuarios.
    /// Proporciona operaciones para iniciar sesion en el sistema.
    /// </summary>
    [ServiceContract]
    public interface IInicioSesionManejador
    {
        /// <summary>
        /// Inicia sesion de un usuario en el sistema.
        /// </summary>
        /// <param name="credenciales">Credenciales de inicio de sesion del usuario.</param>
        /// <returns>Resultado del inicio de sesion con informacion del usuario autenticado.
        /// </returns>
        [OperationContract]
        ResultadoInicioSesionDTO IniciarSesion(CredencialesInicioSesionDTO credenciales);
    }
}
