using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Interfaz que define las operaciones para la logica de recuperacion de cuentas.
    /// </summary>
    public interface IRecuperacionCuentaServicio
    {
        /// <summary>
        /// Procesa la solicitud de un codigo de recuperacion.
        /// </summary>
        ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(
            SolicitudRecuperarCuentaDTO solicitud);

        /// <summary>
        /// Reenvia un codigo de recuperacion existente o nuevo segun la validez del token.
        /// </summary>
        ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenvioCodigoDTO solicitud);

        /// <summary>
        /// Valida y confirma el codigo ingresado por el usuario.
        /// </summary>
        ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion);

        /// <summary>
        /// Actualiza la contrasena del usuario una vez verificado el codigo.
        /// </summary>
        ResultadoOperacionDTO ActualizarContrasena(ActualizacionContrasenaDTO solicitud);
    }
}