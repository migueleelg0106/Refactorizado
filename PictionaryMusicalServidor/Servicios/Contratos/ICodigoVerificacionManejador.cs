using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de codigos de verificacion.
    /// </summary>
    [ServiceContract]
    public interface ICodigoVerificacionManejador
    {
        /// <summary>
        /// Solicita el envio de un codigo de verificacion para registrar una nueva cuenta.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la nueva cuenta a registrar.</param>
        /// <returns>Resultado de la solicitud del codigo de verificacion.</returns>
        [OperationContract]
        ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta);

        /// <summary>
        /// Reenvia un codigo de verificacion previamente solicitado.
        /// </summary>
        /// <param name="solicitud">Solicitud con el token del codigo a reenviar.</param>
        /// <returns>Resultado del reenvio del codigo de verificacion.</returns>
        [OperationContract]
        ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenvioCodigoVerificacionDTO solicitud);

        /// <summary>
        /// Confirma un codigo de verificacion ingresado por el usuario.
        /// </summary>
        /// <param name="confirmacion">Datos para confirmar el codigo de verificacion.</param>
        /// <returns>Resultado de la confirmacion del codigo.</returns>
        [OperationContract]
        ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion);

        /// <summary>
        /// Solicita el envio de un codigo para recuperar una cuenta.
        /// </summary>
        /// <param name="solicitud">Datos del usuario que solicita recuperar su cuenta.</param>
        /// <returns>Resultado de la solicitud del codigo de recuperacion.</returns>
        [OperationContract]
        ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud);

        /// <summary>
        /// Confirma un codigo de recuperacion ingresado por el usuario.
        /// </summary>
        /// <param name="confirmacion">Datos para confirmar el codigo de recuperacion.</param>
        /// <returns>Resultado de la confirmacion del codigo.</returns>
        [OperationContract]
        ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion);
    }
}
