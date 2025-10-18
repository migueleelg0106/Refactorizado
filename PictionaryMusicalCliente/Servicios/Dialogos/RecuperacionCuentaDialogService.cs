using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using PictionaryMusicalCliente;

namespace PictionaryMusicalCliente.Servicios.Dialogos
{
    public class RecuperacionCuentaDialogService : IRecuperacionCuentaDialogService
    {
        private readonly IVerificarCodigoDialogService _verificarCodigoDialogService;

        public RecuperacionCuentaDialogService(IVerificarCodigoDialogService verificarCodigoDialogService)
        {
            _verificarCodigoDialogService = verificarCodigoDialogService ?? throw new ArgumentNullException(nameof(verificarCodigoDialogService));
        }

        public async Task<ResultadoOperacion> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaService cambioContrasenaService)
        {
            if (cambioContrasenaService == null)
            {
                throw new ArgumentNullException(nameof(cambioContrasenaService));
            }

            ResultadoSolicitudRecuperacion resultadoSolicitud = await cambioContrasenaService
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
                return ResultadoOperacion.Fallo(mensaje);
            }

            if (!resultadoSolicitud.CodigoEnviado)
            {
                string mensaje = string.IsNullOrWhiteSpace(resultadoSolicitud.Mensaje)
                    ? Lang.errorTextoServidorSolicitudCambioContrasena
                    : resultadoSolicitud.Mensaje;
                return ResultadoOperacion.Fallo(mensaje);
            }

            AvisoHelper.Mostrar(Lang.avisoTextoCodigoEnviado);

            var adaptador = new ServicioCodigoRecuperacionAdapter(cambioContrasenaService);

            ResultadoRegistroCuenta resultadoVerificacion = await _verificarCodigoDialogService
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
                return ResultadoOperacion.Fallo(mensaje);
            }

            AvisoHelper.Mostrar(Lang.avisoTextoCodigoVerificadoCambio);

            var ventana = new CambioContrasena();
            var vistaModelo = new CambioContrasenaVistaModelo(resultadoSolicitud.TokenCodigo, cambioContrasenaService);
            var finalizacion = new TaskCompletionSource<ResultadoOperacion>();

            vistaModelo.CambioContrasenaCompletado = resultado =>
            {
                finalizacion.TrySetResult(resultado ?? ResultadoOperacion.Exitoso(Lang.avisoTextoContrasenaActualizada));
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

            public Task<ResultadoSolicitudCodigo> SolicitarCodigoRegistroAsync(SolicitudRegistroCuenta solicitud)
            {
                throw new NotSupportedException();
            }

            public Task<ResultadoSolicitudCodigo> ReenviarCodigoRegistroAsync(string tokenCodigo)
            {
                return _cambioContrasenaService.ReenviarCodigoRecuperacionAsync(tokenCodigo);
            }

            public async Task<ResultadoRegistroCuenta> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado)
            {
                ResultadoOperacion resultado = await _cambioContrasenaService
                    .ConfirmarCodigoRecuperacionAsync(tokenCodigo, codigoIngresado).ConfigureAwait(true);

                if (resultado == null)
                {
                    return null;
                }

                return new ResultadoRegistroCuenta
                {
                    RegistroExitoso = resultado.Exito,
                    Mensaje = resultado.Mensaje
                };
            }
        }
    }
}
