using System;
using System.Threading.Tasks;
using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Vista;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Dialogos
{
    /// <summary>
    /// Gestiona el flujo visual completo para la recuperacion de una cuenta de usuario.
    /// Orquesta la solicitud de codigo, verificacion y cambio de contrasena.
    /// </summary>
    public class RecuperacionCuentaDialogoServicio : IRecuperacionCuentaServicio
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(RecuperacionCuentaDialogoServicio));

        private readonly IVerificacionCodigoDialogoServicio _verificarCodigoDialogoServicio;
        private readonly IAvisoServicio _avisoServicio;
        private readonly SonidoManejador _sonidoManejador;
        private readonly ILocalizadorServicio _localizador;

        public RecuperacionCuentaDialogoServicio(
            IVerificacionCodigoDialogoServicio verificarCodigoDialogoServicio,
            IAvisoServicio avisoServicio,
            SonidoManejador sonidoManejador, ILocalizadorServicio localizador)
        {
            _verificarCodigoDialogoServicio = verificarCodigoDialogoServicio ??
                throw new ArgumentNullException(nameof(verificarCodigoDialogoServicio));
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _localizador = localizador ??
                throw new ArgumentNullException(nameof(localizador));
        }

        /// <summary>
        /// Ejecuta el proceso de recuperacion de cuenta paso a paso.
        /// </summary>
        public async Task<DTOs.ResultadoOperacionDTO> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaServicio servicio)
        {
            if (servicio == null) throw new ArgumentNullException(nameof(servicio));

            var resultadoSolicitud = await ProcesarSolicitudCodigo(identificador, servicio);
            if (!resultadoSolicitud.Exito)
            {
                return resultadoSolicitud.Error;
            }

            var resultadoVerificacion = await ProcesarVerificacionCodigo(
                resultadoSolicitud.Token,
                servicio);

            if (!resultadoVerificacion.Exito)
            {
                return resultadoVerificacion.Error;
            }

            return await ProcesarCambioContrasena(
                resultadoSolicitud.Token,
                servicio);
        }

        private async Task<(bool Exito, string Token, DTOs.ResultadoOperacionDTO Error)>
            ProcesarSolicitudCodigo(string identificador, ICambioContrasenaServicio servicio)
        {
            var respuestaServidor = await servicio.SolicitarCodigoRecuperacionAsync(identificador)
                .ConfigureAwait(true);

            var validacion = ValidarRespuestaSolicitud(respuestaServidor);
            if (!validacion.Exito)
            {
                return (false, null, validacion.Error);
            }

            _avisoServicio.Mostrar(Lang.avisoTextoCodigoEnviado);
            return (true, respuestaServidor.TokenCodigo, null);
        }

        private (bool Exito, DTOs.ResultadoOperacionDTO Error) ValidarRespuestaSolicitud(
            DTOs.ResultadoSolicitudRecuperacionDTO respuesta)
        {
            if (respuesta == null)
            {
                return (false, CrearError(null, Lang.errorTextoServidorNoDisponible));
            }

            if (!respuesta.CuentaEncontrada)
            {
                return (false, CrearError(respuesta.Mensaje, Lang.errorTextoCuentaNoRegistrada));
            }

            if (!respuesta.CodigoEnviado)
            {
                return (false, CrearError(
                    respuesta.Mensaje,
                    Lang.errorTextoServidorSolicitudCambioContrasena));
            }

            return (true, null);
        }

        private async Task<(bool Exito, DTOs.ResultadoOperacionDTO Error)>
            ProcesarVerificacionCodigo(string token, ICambioContrasenaServicio servicio)
        {
            var adaptador = CrearAdaptadorVerificacion(servicio);

            var respuestaDialogo = await _verificarCodigoDialogoServicio.MostrarDialogoAsync(
                Lang.cambiarContrasenaTextoCodigoVerificacion,
                token,
                adaptador,
                _avisoServicio,
                _localizador,
                _sonidoManejador).ConfigureAwait(true);

            var validacion = ValidarRespuestaVerificacion(respuestaDialogo);
            if (!validacion.Exito)
            {
                _logger.Warn("Verificacion de codigo fallida.");
                return (false, validacion.Error);
            }

            _avisoServicio.Mostrar(Lang.avisoTextoCodigoVerificadoCambio);
            return (true, null);
        }

        private ICodigoVerificacionServicio CrearAdaptadorVerificacion(
            ICambioContrasenaServicio servicio)
        {
            return new CodigoRecuperacionServicioAdaptador(servicio);
        }

        private (bool Exito, DTOs.ResultadoOperacionDTO Error) ValidarRespuestaVerificacion(
            DTOs.ResultadoRegistroCuentaDTO respuesta)
        {
            if (respuesta == null)
            {
                return (false, null);
            }

            if (!respuesta.RegistroExitoso)
            {
                return (false, CrearError(respuesta.Mensaje, Lang.errorTextoCodigoIncorrecto));
            }

            return (true, null);
        }

        private Task<DTOs.ResultadoOperacionDTO> ProcesarCambioContrasena(
            string token,
            ICambioContrasenaServicio servicio)
        {
            return MostrarDialogoCambio(token, servicio);
        }

        private Task<DTOs.ResultadoOperacionDTO> MostrarDialogoCambio(
            string token,
            ICambioContrasenaServicio servicio)
        {
            var ventana = new CambioContrasena();
            var vistaModelo = new CambioContrasenaVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                token,
                servicio,
                _avisoServicio,
                _sonidoManejador);
            var finalizacion = new TaskCompletionSource<DTOs.ResultadoOperacionDTO>();

            ConfigurarCierreVentana(ventana, finalizacion);

            ventana.DataContext = vistaModelo;
            ventana.ShowDialog();

            return finalizacion.Task;
        }

        private void ConfigurarCierreVentana(
            CambioContrasena ventana,
            TaskCompletionSource<DTOs.ResultadoOperacionDTO> tcs)
        {
            ventana.Closed += (_, __) =>
            {
                if (!tcs.Task.IsCompleted)
                {
                    tcs.TrySetResult(null);
                }
            };
        }

        private static DTOs.ResultadoOperacionDTO CrearError(string mensajeServer, string fallback)
        {
            return new DTOs.ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = string.IsNullOrWhiteSpace(mensajeServer) ? fallback : mensajeServer
            };
        }

        private static DTOs.ResultadoOperacionDTO CrearExitoDefecto()
        {
            return new DTOs.ResultadoOperacionDTO
            {
                OperacionExitosa = true,
                Mensaje = Lang.avisoTextoContrasenaActualizada
            };
        }
    }
}