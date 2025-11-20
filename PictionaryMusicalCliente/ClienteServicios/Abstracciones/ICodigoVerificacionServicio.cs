using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de gestion de codigos de verificacion.
    /// </summary>
    public interface ICodigoVerificacionServicio
    {
        /// <summary>
        /// Solicita el envio de un codigo de verificacion para registro.
        /// </summary>
        /// <param name="solicitud">Datos de la nueva cuenta.</param>
        /// <returns>Resultado de la solicitud del codigo.</returns>
        Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(DTOs.NuevaCuentaDTO solicitud);

        /// <summary>
        /// Reenvia un codigo de verificacion de registro.
        /// </summary>
        /// <param name="tokenCodigo">Token del codigo a reenviar.</param>
        /// <returns>Resultado del reenvio del codigo.</returns>
        Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo);

        /// <summary>
        /// Confirma un codigo de verificacion de registro.
        /// </summary>
        /// <param name="tokenCodigo">Token del codigo.</param>
        /// <param name="codigoIngresado">Codigo ingresado por el usuario.</param>
        /// <returns>Resultado de la confirmacion.</returns>
        Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado);
    }
}
