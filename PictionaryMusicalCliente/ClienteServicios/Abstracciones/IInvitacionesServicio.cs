using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Facilita la interaccion entre usuarios para unirse a partidas privadas mediante correo.
    /// </summary>
    public interface IInvitacionesServicio
    {
        /// <summary>
        /// Despacha una notificacion al correo destino con el codigo de acceso a la sala.
        /// </summary>
        /// <param name="codigoSala">El identificador unico de la sala de juego.</param>
        /// <param name="correoDestino">La direccion de correo del usuario invitado.</param>
        /// <returns>El resultado de la operacion de envio.</returns>
        Task<DTOs.ResultadoOperacionDTO> EnviarInvitacionAsync(
            string codigoSala,
            string correoDestino);
    }
}