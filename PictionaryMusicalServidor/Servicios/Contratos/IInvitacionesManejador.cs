using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para el envio de invitaciones a salas.
    /// </summary>
    [ServiceContract]
    public interface IInvitacionesManejador
    {
        /// <summary>
        /// Envia una invitacion por correo electronico para unirse a una sala.
        /// </summary>
        /// <param name="invitacion">Datos de la invitacion a enviar.</param>
        /// <returns>Resultado del envio de la invitacion.</returns>
        [OperationContract]
        ResultadoOperacionDTO EnviarInvitacion(InvitacionSalaDTO invitacion);
    }
}
