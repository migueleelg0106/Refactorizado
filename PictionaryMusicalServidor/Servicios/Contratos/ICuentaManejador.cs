using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de cuentas de usuario.
    /// Proporciona operaciones para solicitar, verificar y registrar nuevas cuentas.
    /// </summary>
    [ServiceContract]
    public interface ICuentaManejador
    {
        /// <summary>
        /// Solicita un codigo de verificacion para registrar una nueva cuenta.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la nueva cuenta a registrar.</param>
        /// <returns>Resultado de la solicitud del codigo de verificacion.</returns>
        [OperationContract]
        ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta);

        /// <summary>
        /// Reenvia un codigo de verificacion previamente solicitado.
        /// </summary>
        /// <param name="solicitud">Datos para el reenvio del codigo de verificacion.</param>
        /// <returns>Resultado del reenvio del codigo de verificacion.</returns>
        [OperationContract]
        ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenvioCodigoVerificacionDTO solicitud);

        /// <summary>
        /// Confirma el codigo de verificacion ingresado por el usuario.
        /// </summary>
        /// <param name="confirmacion">Datos de confirmacion del codigo de verificacion.</param>
        /// <returns>Resultado de la confirmacion del codigo.</returns>
        [OperationContract]
        ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmacionCodigoDTO confirmacion);

        /// <summary>
        /// Registra una nueva cuenta de usuario en el sistema.
        /// </summary>
        /// <param name="nuevaCuenta">Datos completos de la cuenta a registrar.</param>
        /// <returns>Resultado del registro de la cuenta.</returns>
        [OperationContract]
        ResultadoRegistroCuentaDTO RegistrarCuenta(NuevaCuentaDTO nuevaCuenta);
    }
}
