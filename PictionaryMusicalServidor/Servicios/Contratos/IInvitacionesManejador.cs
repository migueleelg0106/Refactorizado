using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de invitaciones a salas de juego.
    /// Proporciona operaciones para enviar invitaciones a usuarios.
    /// </summary>
    [ServiceContract]
    public interface IInvitacionesManejador
    {
        /// <summary>
        /// Envia una invitacion a una sala de juego a un usuario de forma asincrona.
        /// </summary>
        /// <param name="invitacion">Datos de la invitacion a enviar.</param>
        /// <returns>Resultado del envio de la invitacion.</returns>
        [OperationContract]
        Task<ResultadoOperacionDTO> EnviarInvitacionAsync(InvitacionSalaDTO invitacion);
    }
}
