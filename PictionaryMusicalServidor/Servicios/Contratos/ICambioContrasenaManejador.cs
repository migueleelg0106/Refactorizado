using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para el cambio y recuperacion de contrasena.
    /// </summary>
    [ServiceContract]
    public interface ICambioContrasenaManejador
    {
        /// <summary>
        /// Solicita el envio de un codigo para recuperar la cuenta.
        /// </summary>
        /// <param name="solicitud">Datos del usuario que solicita recuperar su cuenta.</param>
        /// <returns>Resultado de la solicitud del codigo de recuperacion.</returns>
        [OperationContract]
        ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud);

        /// <summary>
        /// Reenvia un codigo de recuperacion previamente solicitado.
        /// </summary>
        /// <param name="solicitud">Solicitud con el token del codigo a reenviar.</param>
        /// <returns>Resultado del reenvio del codigo de recuperacion.</returns>
        [OperationContract]
        ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenvioCodigoDTO solicitud);

        /// <summary>
        /// Confirma un codigo de recuperacion ingresado por el usuario.
        /// </summary>
        /// <param name="confirmacion">Datos para confirmar el codigo de recuperacion.</param>
        /// <returns>Resultado de la confirmacion del codigo.</returns>
        [OperationContract]
        ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion);

        /// <summary>
        /// Actualiza la contrasena de un usuario tras validar el codigo de recuperacion.
        /// </summary>
        /// <param name="solicitud">Datos con el token y la nueva contrasena.</param>
        /// <returns>Resultado de la actualizacion de la contrasena.</returns>
        [OperationContract]
        ResultadoOperacionDTO ActualizarContrasena(ActualizacionContrasenaDTO solicitud);
    }
}
