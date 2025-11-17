using System;
using System.Threading.Tasks;
using System.Windows;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Dialogos
{
    public class RecuperacionCuentaDialogoServicio : IRecuperacionCuentaServicio
    {
        private readonly IVerificacionCodigoDialogoServicio _verificarCodigoDialogoServicio;

        public RecuperacionCuentaDialogoServicio(IVerificacionCodigoDialogoServicio verificarCodigoDialogoServicio)
        {
            _verificarCodigoDialogoServicio = verificarCodigoDialogoServicio ?? throw new ArgumentNullException(nameof(verificarCodigoDialogoServicio));
        }

        public async Task<DTOs.ResultadoOperacionDTO> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaServicio cambioContrasenaServicio)
        {
            if (cambioContrasenaServicio == null)
                throw new ArgumentNullException(nameof(cambioContrasenaServicio));

            var (solicitudExitosa, solicitudDTO, errorSolicitud) =
                await SolicitarCodigoAsync(identificador, cambioContrasenaServicio).ConfigureAwait(true);

            if (!solicitudExitosa)
                return errorSolicitud;

            AvisoAyudante.Mostrar(Lang.avisoTextoCodigoEnviado);

            var (verificacionExitosa, errorVerificacion) =
                await VerificarCodigoAsync(solicitudDTO, cambioContrasenaServicio).ConfigureAwait(true);

            if (!verificacionExitosa)
                return errorVerificacion;

            AvisoAyudante.Mostrar(Lang.avisoTextoCodigoVerificadoCambio);

            return await MostrarDialogoCambioContrasenaAsync(solicitudDTO.TokenCodigo, cambioContrasenaServicio)
                .ConfigureAwait(true);
        }

        private async Task<(bool Exitoso, DTOs.ResultadoSolicitudRecuperacionDTO Resultado, DTOs.ResultadoOperacionDTO Error)>
            SolicitarCodigoAsync(string identificador, ICambioContrasenaServicio servicio)
        {
            DTOs.ResultadoSolicitudRecuperacionDTO resultadoSolicitud =
                await servicio.SolicitarCodigoRecuperacionAsync(identificador).ConfigureAwait(true);

            if (resultadoSolicitud == null)
                return (false, null, null);

            if (!resultadoSolicitud.CuentaEncontrada)
            {
                string mensaje = ObtenerMensaje(resultadoSolicitud.Mensaje, Lang.errorTextoCuentaNoRegistrada);
                return (false, null, new DTOs.ResultadoOperacionDTO { OperacionExitosa = false, Mensaje = mensaje });
            }

            if (!resultadoSolicitud.CodigoEnviado)
            {
                string mensaje = ObtenerMensaje(resultadoSolicitud.Mensaje, Lang.errorTextoServidorSolicitudCambioContrasena);
                return (false, null, new DTOs.ResultadoOperacionDTO { OperacionExitosa = false, Mensaje = mensaje });
            }

            return (true, resultadoSolicitud, null);
        }

        private async Task<(bool Exitoso, DTOs.ResultadoOperacionDTO Error)>
            VerificarCodigoAsync(DTOs.ResultadoSolicitudRecuperacionDTO solicitud, ICambioContrasenaServicio servicio)
        {
            var adaptador = new ServicioCodigoRecuperacionAdaptador(servicio);
            DTOs.ResultadoRegistroCuentaDTO resultadoVerificacion = await _verificarCodigoDialogoServicio
                .MostrarDialogoAsync(
                    Lang.cambiarContrasenaTextoCodigoVerificacion,
                    solicitud.TokenCodigo,
                    adaptador).ConfigureAwait(true);

            if (resultadoVerificacion == null)
                return (false, null);

            if (!resultadoVerificacion.RegistroExitoso)
            {
                string mensaje = ObtenerMensaje(resultadoVerificacion.Mensaje, Lang.errorTextoCodigoIncorrecto);
                return (false, new DTOs.ResultadoOperacionDTO { OperacionExitosa = false, Mensaje = mensaje });
            }

            return (true, null);
        }

        private Task<DTOs.ResultadoOperacionDTO> MostrarDialogoCambioContrasenaAsync(
            string token, ICambioContrasenaServicio servicio)
        {
            var ventana = new CambioContrasena();
            var vistaModelo = new CambioContrasenaVistaModelo(token, servicio);
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
            return finalizacion.Task;
        }

        private static string ObtenerMensaje(string mensaje, string fallback)
        {
            return string.IsNullOrWhiteSpace(mensaje) ? fallback : mensaje;
        }
    }
}