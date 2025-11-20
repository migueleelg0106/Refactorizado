using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Provee metodos especificos para confirmar o reenviar codigos durante el registro.
    /// </summary>
    public interface IVerificacionCodigoServicio
    {
        /// <summary>
        /// Valida el codigo ingresado contra el token generado por el servidor.
        /// </summary>
        /// <param name="tokenCodigo">Token de la sesion de verificacion.</param>
        /// <param name="codigoIngresado">El codigo numerico proveido por el usuario.</param>
        /// <returns>Resultado indicando exito o error en la confirmacion.</returns>
        Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(
            string tokenCodigo,
            string codigoIngresado);

        /// <summary>
        /// Solicita un nuevo envio del codigo de verificacion al correo registrado.
        /// </summary>
        /// <param name="tokenCodigo">Token de la sesion actual.</param>
        /// <returns>Nuevo token asociado al reenvio.</returns>
        Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo);
    }
}