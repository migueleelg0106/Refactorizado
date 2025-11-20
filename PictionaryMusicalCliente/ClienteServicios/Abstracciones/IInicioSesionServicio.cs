using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define las operaciones necesarias para autenticar a un usuario en el sistema.
    /// Permite el acceso a las funcionalidades restringidas de la aplicacion.
    /// </summary>
    public interface IInicioSesionServicio
    {
        /// <summary>
        /// Valida las credenciales proporcionadas para permitir el ingreso del usuario.
        /// </summary>
        /// <param name="solicitud">Objeto que contiene el correo y la contrasena cifrada.</param>
        /// <returns>El resultado del intento de inicio de sesion con el token o error.</returns>
        Task<DTOs.ResultadoInicioSesionDTO> IniciarSesionAsync(
            DTOs.CredencialesInicioSesionDTO solicitud);
    }
}