using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la recuperacion y cambio de contrasena de usuarios.
    /// Proporciona operaciones para solicitar, verificar codigos y actualizar contrasenas.
    /// </summary>
    [ServiceContract]
    public interface ICambioContrasenaManejador
    {
        /// <summary>
        /// Solicita un codigo de verificacion para recuperar la cuenta.
        /// </summary>
        /// <param name="solicitud">Datos de la solicitud de recuperacion de cuenta.</param>
        /// <returns>Resultado de la solicitud del codigo de recuperacion.</returns>
        [OperationContract]
        ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion
            (SolicitudRecuperarCuentaDTO solicitud);

        /// <summary>
        /// Reenvia el codigo de recuperacion previamente solicitado.
        /// </summary>
        /// <param name="solicitud">Datos para el reenvio del codigo de recuperacion.</param>
        /// <returns>Resultado del reenvio del codigo de recuperacion.</returns>
        [OperationContract]
        ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenvioCodigoDTO solicitud);

        /// <summary>
        /// Confirma el codigo de recuperacion ingresado por el usuario.
        /// </summary>
        /// <param name="confirmacion">Datos de confirmacion del codigo de recuperacion.</param>
        /// <returns>Resultado de la confirmacion del codigo.</returns>
        [OperationContract]
        ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion);

        /// <summary>
        /// Actualiza la contrasena del usuario despues de confirmar el codigo.
        /// </summary>
        /// <param name="solicitud">Datos de actualizacion de contrasena.</param>
        /// <returns>Resultado de la actualizacion de la contrasena.</returns>
        [OperationContract]
        ResultadoOperacionDTO ActualizarContrasena(ActualizacionContrasenaDTO solicitud);
    }
}
