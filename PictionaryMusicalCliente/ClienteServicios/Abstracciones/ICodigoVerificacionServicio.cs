using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Maneja la verificacion de dos pasos durante el proceso de registro de cuenta.
    /// </summary>
    public interface ICodigoVerificacionServicio
    {
        /// <summary>
        /// Genera y envia un codigo de verificacion al correo proporcionado en el registro.
        /// </summary>
        /// <param name="solicitud">Datos preliminares del usuario a registrar.</param>
        /// <returns>Token asociado al envio del codigo.</returns>
        Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(
            DTOs.NuevaCuentaDTO solicitud);

        /// <summary>
        /// Solicita un nuevo codigo de verificacion si el anterior no llego o expiro.
        /// </summary>
        /// <param name="tokenCodigo">El token de la sesion de registro actual.</param>
        /// <returns>Nuevo token asociado al reenvio.</returns>
        Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo);

        /// <summary>
        /// Valida el codigo para finalizar la creacion de la cuenta.
        /// </summary>
        /// <param name="tokenCodigo">Token de sesion de verificacion.</param>
        /// <param name="codigoIngresado">Codigo numerico ingresado por el usuario.</param>
        /// <returns>Resultado final del registro de la cuenta.</returns>
        Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(
            string tokenCodigo,
            string codigoIngresado);
    }
}