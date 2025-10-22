using System;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;

namespace PictionaryMusicalCliente.Servicios.Dialogos
{
    public class RecuperacionCuentaDialogService : IRecuperacionCuentaDialogService
    {
        private readonly IVerificarCodigoDialogService _verificarCodigoDialogService;

        public RecuperacionCuentaDialogService(IVerificarCodigoDialogService verificarCodigoDialogService)
        {
            _verificarCodigoDialogService = verificarCodigoDialogService ?? throw new ArgumentNullException(nameof(verificarCodigoDialogService));
        }

        public async Task<DTOs.ResultadoOperacionDTO> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaService cambioContrasenaService)
        {
            if (cambioContrasenaService == null)
            {
                throw new ArgumentNullException(nameof(cambioContrasenaService));
            }

            DTOs.ResultadoSolicitudRecuperacionDTO resultadoSolicitud = await cambioContrasenaService
                .SolicitarCodigoRecuperacionAsync(identificador).ConfigureAwait(true);

            if (resultadoSolicitud == null)
            {
                return null;
            }

            if (!resultadoSolicitud.CuentaEncontrada)
            {
                string mensaje = string.IsNullOrWhiteSpace(resultadoSolicitud.Mensaje)
                    ? Lang.errorTextoCuentaNoRegistrada
                    : resultadoSolicitud.Mensaje;
                return DTOs.ResultadoOperacionDTO.Fallo(mensaje);
            }

            if (!resultadoSolicitud.CodigoEnviado)
            {
                string mensaje = string.IsNullOrWhiteSpace(resultadoSolicitud.Mensaje)
                    ? Lang.errorTextoServidorSolicitudCambioContrasena
                    : resultadoSolicitud.Mensaje;
                return DTOs.ResultadoOperacionDTO.Fallo(mensaje);
            }

            AvisoHelper.Mostrar(Lang.avisoTextoCodigoEnviado);

            var adaptador = new ServicioCodigoRecuperacionAdapter(cambioContrasenaService);

            DTOs.ResultadoRegistroCuentaDTO resultadoVerificacion = await _verificarCodigoDialogService
                .MostrarDialogoAsync(
                    Lang.cambiarContrasenaTextoCodigoVerificacion,
                    resultadoSolicitud.TokenCodigo,
                    adaptador).ConfigureAwait(true);

            if (resultadoVerificacion == null)
            {
                return null;
            }

            if (!resultadoVerificacion.RegistroExitoso)
            {
                string mensaje = string.IsNullOrWhiteSpace(resultadoVerificacion.Mensaje)
                    ? Lang.errorTextoCodigoIncorrecto
                    : resultadoVerificacion.Mensaje;
                return DTOs.ResultadoOperacionDTO.Fallo(mensaje);
            }

            AvisoHelper.Mostrar(Lang.avisoTextoCodigoVerificadoCambio);

            var ventana = new CambioContrasena();
            var vistaModelo = new CambioContrasenaVistaModelo(resultadoSolicitud.TokenCodigo, cambioContrasenaService);
            var finalizacion = new TaskCompletionSource<DTOs.ResultadoOperacionDTO>();

            vistaModelo.CambioContrasenaCompletado = resultado =>
            {
                finalizacion.TrySetResult(resultado ?? DTOs.ResultadoOperacionDTO.Exitoso(Lang.avisoTextoContrasenaActualizada));
                ventana.Close();
            };

            vistaModelo.Cancelado = () =>
            {
                finalizacion.TrySetResult(null);
                ventana.Close();
            };

            ventana.ConfigurarVistaModelo(vistaModelo);

            ventana.Closed += (_, __) =>
            {
                if (!finalizacion.Task.IsCompleted)
                {
                    finalizacion.TrySetResult(null);
                }
            };

            ventana.ShowDialog();

            return await finalizacion.Task.ConfigureAwait(true);
        }

        private class ServicioCodigoRecuperacionAdapter : ICodigoVerificacionService
        {
            private readonly ICambioContrasenaService _cambioContrasenaService;

            public ServicioCodigoRecuperacionAdapter(ICambioContrasenaService cambioContrasenaService)
            {
                _cambioContrasenaService = cambioContrasenaService ?? throw new ArgumentNullException(nameof(cambioContrasenaService));
            }

            public Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(DTOs.SolicitudRegistroCuentaDTO solicitud)
            {
                throw new NotSupportedException();
            }

            public Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo)
            {
                return _cambioContrasenaService.ReenviarCodigoRecuperacionAsync(tokenCodigo);
            }

            public async Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado)
            {
                DTOs.ResultadoOperacionDTO resultado = await _cambioContrasenaService
                    .ConfirmarCodigoRecuperacionAsync(tokenCodigo, codigoIngresado).ConfigureAwait(true);

                if (resultado == null)
                {
                    return null;
                }

                return new DTOs.ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = resultado.Exito,
                    Mensaje = resultado.Mensaje
                };
            }
        }
    }
}
