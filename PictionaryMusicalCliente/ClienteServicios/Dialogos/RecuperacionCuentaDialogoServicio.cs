using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Dialogos
{
    public class RecuperacionCuentaDialogoServicio : IRecuperacionCuentaServicio
    {
        private readonly IVerificarCodigoDialogoServicio _verificarCodigoDialogService;

        public RecuperacionCuentaDialogoServicio(IVerificarCodigoDialogoServicio verificarCodigoDialogService)
        {
            _verificarCodigoDialogService = verificarCodigoDialogService ?? throw new ArgumentNullException(nameof(verificarCodigoDialogService));
        }

        public async Task<DTOs.ResultadoOperacionDTO> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaServicio cambioContrasenaService)
        {
            if (cambioContrasenaService == null)
                throw new ArgumentNullException(nameof(cambioContrasenaService));

            DTOs.ResultadoSolicitudRecuperacionDTO resultadoSolicitud =
                await cambioContrasenaService.SolicitarCodigoRecuperacionAsync(identificador).ConfigureAwait(true);

            if (resultadoSolicitud == null) return null;

            if (!resultadoSolicitud.CuentaEncontrada)
            {
                string mensaje = string.IsNullOrWhiteSpace(resultadoSolicitud.Mensaje)
                    ? Lang.errorTextoCuentaNoRegistrada
                    : resultadoSolicitud.Mensaje;

                return new DTOs.ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = mensaje
                };
            }

            if (!resultadoSolicitud.CodigoEnviado)
            {
                string mensaje = string.IsNullOrWhiteSpace(resultadoSolicitud.Mensaje)
                    ? Lang.errorTextoServidorSolicitudCambioContrasena
                    : resultadoSolicitud.Mensaje;

                return new DTOs.ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = mensaje
                };
            }

            AvisoAyudante.Mostrar(Lang.avisoTextoCodigoEnviado);

            var adaptador = new ServicioCodigoRecuperacionAdapter(cambioContrasenaService);

            DTOs.ResultadoRegistroCuentaDTO resultadoVerificacion = await _verificarCodigoDialogService
                .MostrarDialogoAsync(
                    Lang.cambiarContrasenaTextoCodigoVerificacion,
                    resultadoSolicitud.TokenCodigo,
                    adaptador).ConfigureAwait(true);

            if (resultadoVerificacion == null) return null;

            if (!resultadoVerificacion.RegistroExitoso)
            {
                string mensaje = string.IsNullOrWhiteSpace(resultadoVerificacion.Mensaje)
                    ? Lang.errorTextoCodigoIncorrecto
                    : resultadoVerificacion.Mensaje;

                return new DTOs.ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = mensaje
                };
            }

            AvisoAyudante.Mostrar(Lang.avisoTextoCodigoVerificadoCambio);

            var ventana = new CambioContrasena();
            var vistaModelo = new CambioContrasenaVistaModelo(resultadoSolicitud.TokenCodigo, cambioContrasenaService);
            var finalizacion = new TaskCompletionSource<DTOs.ResultadoOperacionDTO>();

            vistaModelo.CambioContrasenaCompletado = resultado =>
            {
                finalizacion.TrySetResult(
                    resultado ?? new DTOs.ResultadoOperacionDTO
                    {
                        OperacionExitosa = true,
                        Mensaje = Lang.avisoTextoContrasenaActualizada
                    });
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

        private class ServicioCodigoRecuperacionAdapter : ICodigoVerificacionServicio
        {
            private readonly ICambioContrasenaServicio _cambioContrasenaService;

            public ServicioCodigoRecuperacionAdapter(ICambioContrasenaServicio cambioContrasenaService)
            {
                _cambioContrasenaService = cambioContrasenaService ?? throw new ArgumentNullException(nameof(cambioContrasenaService));
            }

            public Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(DTOs.NuevaCuentaDTO solicitud)
                => throw new NotSupportedException();

            public Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo)
                => _cambioContrasenaService.ReenviarCodigoRecuperacionAsync(tokenCodigo);

            public async Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado)
            {
                DTOs.ResultadoOperacionDTO resultado =
                    await _cambioContrasenaService.ConfirmarCodigoRecuperacionAsync(tokenCodigo, codigoIngresado).ConfigureAwait(true);

                if (resultado == null) return null;

                return new DTOs.ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = resultado.OperacionExitosa,
                    Mensaje = resultado.Mensaje
                };
            }
        }
    }
}
