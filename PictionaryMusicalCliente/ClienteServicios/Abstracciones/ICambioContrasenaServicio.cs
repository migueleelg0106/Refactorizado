using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Orquesta el flujo de recuperacion y actualizacion de credenciales de seguridad.
    /// </summary>
    public interface ICambioContrasenaServicio
    {
        /// <summary>
        /// Inicia el proceso de recuperacion generando un codigo para el usuario identificado.
        /// </summary>
        /// <param name="identificador">Correo electronico o nombre de usuario.</param>
        /// <returns>Resultado indicando si se pudo generar y enviar el codigo.</returns>
        Task<DTOs.ResultadoSolicitudRecuperacionDTO> SolicitarCodigoRecuperacionAsync(
            string identificador);

        /// <summary>
        /// Vuelve a emitir el codigo de recuperacion en caso de perdida o expiracion.
        /// </summary>
        /// <param name="tokenCodigo">Token temporal asociado a la solicitud.</param>
        /// <returns>El resultado del reenvio del codigo.</returns>
        Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRecuperacionAsync(string tokenCodigo);

        /// <summary>
        /// Valida que el codigo ingresado por el usuario coincida con el generado.
        /// </summary>
        /// <param name="tokenCodigo">Token temporal de la transaccion.</param>
        /// <param name="codigoIngresado">El codigo de verificacion introducido por el usuario.
        /// </param>
        /// <returns>Confirmacion de validez del codigo.</returns>
        Task<DTOs.ResultadoOperacionDTO> ConfirmarCodigoRecuperacionAsync(
            string tokenCodigo,
            string codigoIngresado);

        /// <summary>
        /// Finaliza el proceso estableciendo la nueva clave de acceso.
        /// </summary>
        /// <param name="tokenCodigo">Token validado que autoriza el cambio.</param>
        /// <param name="nuevaContrasena">La nueva credencial de acceso.</param>
        /// <returns>Resultado de la actualizacion en base de datos.</returns>
        Task<DTOs.ResultadoOperacionDTO> ActualizarContrasenaAsync(
            string tokenCodigo,
            string nuevaContrasena);
    }
}