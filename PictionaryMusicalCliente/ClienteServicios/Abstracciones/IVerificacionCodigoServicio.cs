using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de verificacion de codigo.
    /// </summary>
    public interface IVerificacionCodigoServicio
    {
        /// <summary>
        /// Confirma un codigo de registro ingresado.
        /// </summary>
        /// <param name="tokenCodigo">Token del codigo.</param>
        /// <param name="codigoIngresado">Codigo ingresado por el usuario.</param>
        /// <returns>Resultado de la confirmacion.</returns>
        Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado);
        
        /// <summary>
        /// Reenvia un codigo de registro.
        /// </summary>
        /// <param name="tokenCodigo">Token del codigo a reenviar.</param>
        /// <returns>Resultado del reenvio.</returns>
        Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo);
    }
}