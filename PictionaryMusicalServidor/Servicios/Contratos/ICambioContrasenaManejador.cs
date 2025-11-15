using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    [ServiceContract]
    public interface ICambioContrasenaManejador
    {
        [OperationContract]
        ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud);

        [OperationContract]
        ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenvioCodigoDTO solicitud);

        [OperationContract]
        ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion);

        [OperationContract]
        ResultadoOperacionDTO ActualizarContrasena(ActualizacionContrasenaDTO solicitud);
    }
}
