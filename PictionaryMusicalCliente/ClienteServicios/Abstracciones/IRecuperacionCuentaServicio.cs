using System.Threading.Tasks;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de recuperacion de cuenta mediante dialogo.
    /// </summary>
    public interface IRecuperacionCuentaServicio
    {
        /// <summary>
        /// Inicia el proceso de recuperacion de cuenta mediante dialogo interactivo.
        /// </summary>
        /// <param name="identificador">Nombre de usuario o correo electronico.</param>
        /// <param name="cambioContrasenaServicio">Servicio para cambio de contrasena.</param>
        /// <returns>Resultado de la operacion de recuperacion.</returns>
        Task<DTOs.ResultadoOperacionDTO> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaServicio cambioContrasenaServicio);
    }
}
