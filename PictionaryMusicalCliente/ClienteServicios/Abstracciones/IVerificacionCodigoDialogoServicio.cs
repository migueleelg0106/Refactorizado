using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de dialogo de verificacion de codigo.
    /// </summary>
    public interface IVerificacionCodigoDialogoServicio
    {
        /// <summary>
        /// Muestra el dialogo de verificacion de codigo.
        /// </summary>
        /// <param name="descripcion">Descripcion a mostrar en el dialogo.</param>
        /// <param name="tokenCodigo">Token del codigo de verificacion.</param>
        /// <param name="codigoVerificacionServicio">Servicio para verificar el codigo.</param>
        /// <returns>Resultado del registro tras la verificacion.</returns>
        Task<DTOs.ResultadoRegistroCuentaDTO> MostrarDialogoAsync(
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionServicio codigoVerificacionServicio);
    }
}
