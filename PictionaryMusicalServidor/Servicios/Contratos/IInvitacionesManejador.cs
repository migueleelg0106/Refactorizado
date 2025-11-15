using System.ServiceModel;
using PictionaryMusical.Servicios.Contratos.DTOs;

namespace PictionaryMusical.Servicios.Contratos
{
    [ServiceContract]
    public interface IInvitacionesManejador
    {
        [OperationContract]
        ResultadoOperacionDTO EnviarInvitacion(InvitacionSalaDTO invitacion);
    }
}
