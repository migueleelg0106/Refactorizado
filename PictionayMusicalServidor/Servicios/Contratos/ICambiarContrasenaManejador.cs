using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface ICambiarContrasenaManejador
    {
        [OperationContract]
        ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud);

        [OperationContract]
        ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenviarCodigoDTO solicitud);

        [OperationContract]
        ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoDTO confirmacion);

        [OperationContract]
        ResultadoOperacionDTO ActualizarContrasena(ActualizarContrasenaDTO solicitud);
    }
}
