using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Interfaz que define las operaciones para la logica de verificacion de nuevos registros.
    /// </summary>
    public interface IVerificacionRegistroServicio
    {
        /// <summary>
        /// Solicita un codigo para una nueva cuenta.
        /// </summary>
        ResultadoSolicitudCodigoDTO SolicitarCodigo(NuevaCuentaDTO nuevaCuenta);

        /// <summary>
        /// Reenvia el codigo de verificacion existente.
        /// </summary>
        ResultadoSolicitudCodigoDTO ReenviarCodigo(ReenvioCodigoVerificacionDTO solicitud);

        /// <summary>
        /// Confirma el codigo y marca la verificacion como lista.
        /// </summary>
        ResultadoRegistroCuentaDTO ConfirmarCodigo(ConfirmacionCodigoDTO confirmacion);

        /// <summary>
        /// Verifica si una cuenta ya paso el proceso de verificacion.
        /// </summary>
        bool EstaVerificacionConfirmada(NuevaCuentaDTO nuevaCuenta);

        /// <summary>
        /// Limpia el estado de verificacion una vez que la cuenta ha sido registrada.
        /// </summary>
        void LimpiarVerificacion(NuevaCuentaDTO nuevaCuenta);
    }
}