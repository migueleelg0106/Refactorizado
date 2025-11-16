using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using System;
using System.Threading.Tasks;

using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante
{
    public class ServicioCodigoRecuperacionAdaptador : ICodigoVerificacionServicio
    {
        private readonly ICambioContrasenaServicio _cambioContrasenaServicio;

        public ServicioCodigoRecuperacionAdaptador(ICambioContrasenaServicio cambioContrasenaServicio)
        {
            _cambioContrasenaServicio = cambioContrasenaServicio ?? throw new ArgumentNullException(nameof(cambioContrasenaServicio));
        }

        public Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(DTOs.NuevaCuentaDTO solicitud)
            => throw new NotSupportedException();

        public Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo)
            => _cambioContrasenaServicio.ReenviarCodigoRecuperacionAsync(tokenCodigo);

        public async Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado)
        {
            DTOs.ResultadoOperacionDTO resultado =
                await _cambioContrasenaServicio.ConfirmarCodigoRecuperacionAsync(tokenCodigo, codigoIngresado).ConfigureAwait(true);

            if (resultado == null) return null;

            return new DTOs.ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = resultado.OperacionExitosa,
                Mensaje = resultado.Mensaje
            };
        }
    }
}
