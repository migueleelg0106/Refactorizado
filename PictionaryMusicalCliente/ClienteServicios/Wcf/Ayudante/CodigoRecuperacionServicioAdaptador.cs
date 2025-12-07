using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using System;
using System.Threading.Tasks;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante
{
    /// <summary>
    /// Adapta la interfaz de verificacion para ser usada en el contexto de recuperacion de cuenta,
    /// redirigiendo las llamadas al servicio de cambio de contrasena.
    /// </summary>
    public class CodigoRecuperacionServicioAdaptador : ICodigoVerificacionServicio
    {
        private static readonly ILog _logger = LogManager.
            GetLogger(typeof(CodigoRecuperacionServicioAdaptador));
        private readonly ICambioContrasenaServicio _cambioContrasenaServicio;

        /// <summary>
        /// Inicializa una nueva instancia del adaptador inyectando el servicio de cambio de 
        /// contrasena.
        /// </summary>
        /// <param name="cambioContrasenaServicio">La instancia del servicio real que procesara
        /// las peticiones.</param>
        public CodigoRecuperacionServicioAdaptador(
            ICambioContrasenaServicio cambioContrasenaServicio)
        {
            _cambioContrasenaServicio = cambioContrasenaServicio ??
                                        throw new ArgumentNullException
                                            (nameof(cambioContrasenaServicio));
        }

        /// <summary>
        /// Operacion no soportada en este adaptador. El registro de cuentas no se maneja en el
        /// flujo de recuperacion.
        /// </summary>
        /// <param name="solicitud">Datos de la solicitud de registro.</param>
        /// <returns>Esta operacion siempre lanza una excepcion.</returns>
        public Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(
            DTOs.NuevaCuentaDTO solicitud)
        {
            var ex = new NotSupportedException
                ("No se puede solicitar registro desde el adaptador de recuperacion.");
            _logger.Error("Operacion no soportada invocada.", ex);
            throw ex;
        }

        /// <summary>
        /// Redirige la solicitud de reenvio de codigo al servicio de recuperacion de contrasena.
        /// </summary>
        /// <param name="tokenCodigo">El token asociado al codigo previamente solicitado.</param>
        /// <returns>El resultado de la operacion de reenvio.</returns>
        public Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync
            (string tokenCodigo)
        {
            return _cambioContrasenaServicio.ReenviarCodigoRecuperacionAsync(tokenCodigo);
        }

        /// <summary>
        /// Redirige la confirmacion del codigo al servicio de recuperacion y adapta la respuesta
        /// al formato de registro.
        /// </summary>
        /// <param name="tokenCodigo">El token asociado al codigo de recuperacion.</param>
        /// <param name="codigoIngresado">El codigo numerico ingresado por el usuario.</param>
        /// <returns>El resultado adaptado indicando si la operacion fue exitosa.</returns>
        public async Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(
            string tokenCodigo,
            string codigoIngresado)
        {
            DTOs.ResultadoOperacionDTO resultado =
                await _cambioContrasenaServicio.ConfirmarCodigoRecuperacionAsync(
                    tokenCodigo,
                    codigoIngresado).ConfigureAwait(true);

            if (resultado == null)
            {
                _logger.Warn("La confirmacion de codigo retorno null.");
                return null;
            }

            return new DTOs.ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = resultado.OperacionExitosa,
                Mensaje = resultado.Mensaje
            };
        }
    }
}