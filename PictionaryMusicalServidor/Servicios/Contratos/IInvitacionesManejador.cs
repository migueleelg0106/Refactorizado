using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    [ServiceContract]
    public interface IInvitacionesManejador
    {
        [OperationContract]
        ResultadoOperacionDTO EnviarInvitacion(InvitacionSalaDTO invitacion);
    }
}
