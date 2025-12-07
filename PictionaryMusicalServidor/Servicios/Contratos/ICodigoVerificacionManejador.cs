using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de codigos de verificacion.
    /// Proporciona operaciones para solicitar, reenviar y confirmar codigos de verificacion y 
    /// recuperacion.
    /// </summary>
    [ServiceContract]
    public interface ICodigoVerificacionManejador
    {
        /// <summary>
        /// Solicita un codigo de verificacion para registrar una nueva cuenta.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la nueva cuenta a verificar.</param>
        /// <returns>Resultado de la solicitud del codigo de verificacion.</returns>
        [OperationContract]
        ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta);

        /// <summary>
        /// Reenvia el codigo de verificacion para registro.
        /// </summary>
        /// <param name="solicitud">Datos para el reenvio del codigo de verificacion.</param>
        /// <returns>Resultado del reenvio del codigo de verificacion.</returns>
        [OperationContract]
        ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion
            (ReenvioCodigoVerificacionDTO solicitud);

        /// <summary>
        /// Confirma el codigo de verificacion ingresado para registro.
        /// </summary>
        /// <param name="confirmacion">Datos de confirmacion del codigo de verificacion.</param>
        /// <returns>Resultado de la confirmacion del codigo.</returns>
        [OperationContract]
        ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion);

        /// <summary>
        /// Solicita un codigo de verificacion para recuperar una cuenta.
        /// </summary>
        /// <param name="solicitud">Datos de la solicitud de recuperacion de cuenta.</param>
        /// <returns>Resultado de la solicitud del codigo de recuperacion.</returns>
        [OperationContract]
        ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion
            (SolicitudRecuperarCuentaDTO solicitud);

        /// <summary>
        /// Confirma el codigo de verificacion ingresado para recuperacion.
        /// </summary>
        /// <param name="confirmacion">Datos de confirmacion del codigo de recuperacion.</param>
        /// <returns>Resultado de la confirmacion del codigo.</returns>
        [OperationContract]
        ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion);
    }
}
