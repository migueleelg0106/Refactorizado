using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de cambio y recuperacion de contrasena.
    /// </summary>
    public interface ICambioContrasenaServicio
    {
        /// <summary>
        /// Solicita un codigo de recuperacion de cuenta.
        /// </summary>
        /// <param name="identificador">Nombre de usuario o correo electronico.</param>
        /// <returns>Resultado de la solicitud.</returns>
        Task<DTOs.ResultadoSolicitudRecuperacionDTO> SolicitarCodigoRecuperacionAsync(string identificador);

        /// <summary>
        /// Reenvia el codigo de recuperacion.
        /// </summary>
        /// <param name="tokenCodigo">Token del codigo a reenviar.</param>
        /// <returns>Resultado del reenvio.</returns>
        Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRecuperacionAsync(string tokenCodigo);

        /// <summary>
        /// Confirma un codigo de recuperacion ingresado.
        /// </summary>
        /// <param name="tokenCodigo">Token del codigo.</param>
        /// <param name="codigoIngresado">Codigo ingresado por el usuario.</param>
        /// <returns>Resultado de la confirmacion.</returns>
        Task<DTOs.ResultadoOperacionDTO> ConfirmarCodigoRecuperacionAsync(string tokenCodigo, string codigoIngresado);

        /// <summary>
        /// Actualiza la contrasena del usuario.
        /// </summary>
        /// <param name="tokenCodigo">Token del codigo validado.</param>
        /// <param name="nuevaContrasena">Nueva contrasena.</param>
        /// <returns>Resultado de la actualizacion.</returns>
        Task<DTOs.ResultadoOperacionDTO> ActualizarContrasenaAsync(string tokenCodigo, string nuevaContrasena);
    }
}
