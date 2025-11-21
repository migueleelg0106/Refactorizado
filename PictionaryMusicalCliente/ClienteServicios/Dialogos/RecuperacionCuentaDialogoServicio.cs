using System;
using System.Threading.Tasks;
using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Dialogos
{
    /// <summary>
    /// Gestiona el flujo visual completo para la recuperacion de una cuenta de usuario.
    /// </summary>
    public class RecuperacionCuentaDialogoServicio : IRecuperacionCuentaServicio
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(RecuperacionCuentaDialogoServicio));
        private readonly IVerificacionCodigoDialogoServicio _verificarCodigoDialogoServicio;

        /// <summary>
        /// Inicializa el servicio con la dependencia de dialogos de verificacion.
        /// </summary>
        public RecuperacionCuentaDialogoServicio(
            IVerificacionCodigoDialogoServicio verificarCodigoDialogoServicio)
        {
            _verificarCodigoDialogoServicio = verificarCodigoDialogoServicio ??
                throw new ArgumentNullException(nameof(verificarCodigoDialogoServicio));
        }

        /// <summary>
        /// Ejecuta la orquestacion de pasos para recuperar la cuenta.
        /// </summary>
        public async Task<DTOs.ResultadoOperacionDTO> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaServicio cambioContrasenaServicio)
        {
            if (cambioContrasenaServicio == null)
            {
                throw new ArgumentNullException(nameof(cambioContrasenaServicio));
            }

            _logger.InfoFormat("Iniciando flujo de recuperación de cuenta para: {0}", 
                identificador);

            var (solicitudExitosa, solicitudDTO, errorSolicitud) =
                await SolicitarCodigoAsync(
                    identificador,
                    cambioContrasenaServicio).ConfigureAwait(true);

            if (!solicitudExitosa)
            {
                _logger.WarnFormat("Flujo detenido: No se pudo solicitar el código. Mensaje: {0}",
                    errorSolicitud?.Mensaje);
                return errorSolicitud;
            }

            AvisoAyudante.Mostrar(Lang.avisoTextoCodigoEnviado);

            var (verificacionExitosa, errorVerificacion) =
                await VerificarCodigoAsync(
                    solicitudDTO,
                    cambioContrasenaServicio).ConfigureAwait(true);

            if (!verificacionExitosa)
            {
                _logger.Warn("Flujo detenido: Verificación de código fallida o cancelada.");
                return errorVerificacion;
            }

            AvisoAyudante.Mostrar(Lang.avisoTextoCodigoVerificadoCambio);

            return await MostrarDialogoCambioContrasenaAsync(
                solicitudDTO.TokenCodigo,
                cambioContrasenaServicio).ConfigureAwait(true);
        }

        private async Task<(bool Exitoso,
            DTOs.ResultadoSolicitudRecuperacionDTO Resultado,
            DTOs.ResultadoOperacionDTO Error)> SolicitarCodigoAsync(
            string identificador,
            ICambioContrasenaServicio servicio)
        {
            DTOs.ResultadoSolicitudRecuperacionDTO resultadoSolicitud =
                await servicio.SolicitarCodigoRecuperacionAsync(identificador).
                    ConfigureAwait(true);

            if (resultadoSolicitud == null)
            {
                return (false, null, null);
            }

            if (!resultadoSolicitud.CuentaEncontrada)
            {
                string mensaje = ObtenerMensaje(
                    resultadoSolicitud.Mensaje,
                    Lang.errorTextoCuentaNoRegistrada);
                return (false, null, new DTOs.ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = mensaje
                });
            }

            if (!resultadoSolicitud.CodigoEnviado)
            {
                string mensaje = ObtenerMensaje(
                    resultadoSolicitud.Mensaje,
                    Lang.errorTextoServidorSolicitudCambioContrasena);
                return (false, null, new DTOs.ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = mensaje
                });
            }

            return (true, resultadoSolicitud, null);
        }

        private async Task<(bool Exitoso, DTOs.ResultadoOperacionDTO Error)> VerificarCodigoAsync(
            DTOs.ResultadoSolicitudRecuperacionDTO solicitud,
            ICambioContrasenaServicio servicio)
        {
            var adaptador = new ServicioCodigoRecuperacionAdaptador(servicio);
            DTOs.ResultadoRegistroCuentaDTO resultadoVerificacion =
                await _verificarCodigoDialogoServicio.MostrarDialogoAsync(
                    Lang.cambiarContrasenaTextoCodigoVerificacion,
                    solicitud.TokenCodigo,
                    adaptador).ConfigureAwait(true);

            if (resultadoVerificacion == null)
            {
                return (false, null);
            }

            if (!resultadoVerificacion.RegistroExitoso)
            {
                string mensaje = ObtenerMensaje(
                    resultadoVerificacion.Mensaje,
                    Lang.errorTextoCodigoIncorrecto);
                return (false, new DTOs.ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = mensaje
                });
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
                _logger.Info("Cambio de contraseña completado exitosamente.");
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
                _logger.Info("Diálogo de cambio de contraseña cancelado.");
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