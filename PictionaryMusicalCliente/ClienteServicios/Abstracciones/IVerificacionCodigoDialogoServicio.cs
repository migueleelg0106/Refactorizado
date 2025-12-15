using PictionaryMusicalCliente.Utilidades;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Abstrae la logica de interfaz de usuario para mostrar ventanas emergentes de verificacion.
    /// </summary>
    public interface IVerificacionCodigoDialogoServicio
    {
        /// <summary>
        /// Despliega un dialogo modal para ingresar el codigo de verificacion enviado por correo.
        /// </summary>
        /// <param name="descripcion">Mensaje explicativo para el usuario.</param>
        /// <param name="tokenCodigo">Token de sesion asociado al codigo enviado.</param>
        /// <param name="codigoVerificacionServicio">Servicio encargado de validar el codigo.
        /// </param>
        /// <returns>El resultado de la operacion de verificacion.</returns>
        Task<DTOs.ResultadoRegistroCuentaDTO> MostrarDialogoAsync(
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionServicio codigoVerificacionServicio,
            IAvisoServicio avisoServicio,
            SonidoManejador sonidoManejador);
    }
}