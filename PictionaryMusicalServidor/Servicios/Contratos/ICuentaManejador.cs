using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface ICuentaManejador
    {
        [OperationContract]
        ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta);

        [OperationContract]
        ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenvioCodigoVerificacionDTO solicitud);

        [OperationContract]
        ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion);

        [OperationContract]
        ResultadoRegistroCuentaDTO RegistrarCuenta(NuevaCuentaDTO nuevaCuenta);
    }
}
