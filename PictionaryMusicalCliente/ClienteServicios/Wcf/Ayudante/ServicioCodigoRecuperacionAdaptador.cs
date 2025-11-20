using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using System;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante
{
    /// <summary>
    /// Adapta la interfaz de verificacion para ser usada en el contexto de recuperacion de cuenta.
    /// </summary>
    public class ServicioCodigoRecuperacionAdaptador : ICodigoVerificacionServicio
    {
        private readonly ICambioContrasenaServicio _cambioContrasenaServicio;

        /// <summary>
        /// Inicializa una nueva instancia del adaptador con el servicio de cambio de contrasena.
        /// </summary>
        /// <param name="cambioContrasenaServicio">Servicio real que procesara las peticiones.
        /// </param>
        public ServicioCodigoRecuperacionAdaptador(
            ICambioContrasenaServicio cambioContrasenaServicio)
        {
            _cambioContrasenaServicio = cambioContrasenaServicio ??
                                        throw new ArgumentNullException
                                            (nameof(cambioContrasenaServicio));
        }

        /// <summary>
        /// Operacion no soportada en este contexto ya que el registro es distinto a la recuperacion.
        /// </summary>
        public Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(
            DTOs.NuevaCuentaDTO solicitud) => throw new NotSupportedException();

        /// <summary>
        /// Redirige la solicitud de reenvio al servicio de recuperacion.
        /// </summary>
        public Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string 
                tokenCodigo)
            => _cambioContrasenaServicio.ReenviarCodigoRecuperacionAsync(tokenCodigo);

        /// <summary>
        /// Redirige la confirmacion del codigo al servicio de recuperacion y adapta la respuesta.
        /// </summary>
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