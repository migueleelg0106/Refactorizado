using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de envio de invitaciones a salas.
    /// </summary>
    public interface IInvitacionesServicio
    {
        /// <summary>
        /// Envia una invitacion por correo electronico para unirse a una sala.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala a la que se invita.</param>
        /// <param name="correoDestino">Correo electronico del destinatario.</param>
        /// <returns>Resultado del envio de la invitacion.</returns>
        Task<DTOs.ResultadoOperacionDTO> EnviarInvitacionAsync(string codigoSala, string correoDestino);
    }
}
