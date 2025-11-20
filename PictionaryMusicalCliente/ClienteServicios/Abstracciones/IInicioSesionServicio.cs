using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de inicio de sesion.
    /// </summary>
    public interface IInicioSesionServicio
    {
        /// <summary>
        /// Inicia sesion con las credenciales proporcionadas.
        /// </summary>
        /// <param name="solicitud">Credenciales del usuario.</param>
        /// <returns>Resultado del intento de inicio de sesion.</returns>
        Task<DTOs.ResultadoInicioSesionDTO> IniciarSesionAsync(DTOs.CredencialesInicioSesionDTO solicitud);
    }
}
