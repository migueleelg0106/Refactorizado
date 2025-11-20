using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Coordina el flujo de alto nivel para restablecer el acceso a una cuenta de usuario.
    /// </summary>
    public interface IRecuperacionCuentaServicio
    {
        /// <summary>
        /// Ejecuta el proceso completo de recuperacion, incluyendo solicitud y cambio de clave.
        /// </summary>
        /// <param name="identificador">Correo o usuario que perdio el acceso.</param>
        /// <param name="cambioContrasenaServicio">Dependencia para ejecutar el cambio.</param>
        /// <returns>Resultado final de la operacion de recuperacion.</returns>
        Task<DTOs.ResultadoOperacionDTO> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaServicio cambioContrasenaServicio);
    }
}