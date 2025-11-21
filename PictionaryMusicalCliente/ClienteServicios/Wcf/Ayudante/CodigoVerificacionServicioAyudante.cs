using System;
using System.Globalization;
using System.Threading.Tasks;
using log4net;
using PictionaryMusicalCliente.ClienteServicios.Idiomas;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante
{
    /// <summary>
    /// Facilita la creacion y consumo de servicios WCF relacionados con la verificacion de 
    /// codigos.
    /// </summary>
    public static class CodigoVerificacionServicioAyudante
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(CodigoVerificacionServicioAyudante));

        private const string CodigoVerificacionEndpoint =
            "BasicHttpBinding_ICodigoVerificacionManejador";
        private const string CuentaEndpoint =
            "BasicHttpBinding_ICuentaManejador";
        private const string CambioContrasenaEndpoint =
            "BasicHttpBinding_ICambioContrasenaManejador";

        /// <summary>
        /// Consume el servicio para solicitar un codigo de registro.
        /// </summary>
        public static Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(
            DTOs.NuevaCuentaDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            solicitud.Idioma ??= ObtenerCodigoIdiomaActual();
            _logger.InfoFormat("Iniciando solicitud de código de registro para '{0}'.", 
                solicitud.Correo);

            var cliente = new PictionaryServidorServicioCodigoVerificacion
                .CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);

            return WcfClienteAyudante.UsarAsincronoAsync(
                cliente,
                c => c.SolicitarCodigoVerificacionAsync(solicitud));
        }

        /// <summary>
        /// Consume el servicio para solicitar un codigo de recuperacion de cuenta.
        /// </summary>
        public static Task<DTOs.ResultadoSolicitudRecuperacionDTO>
            SolicitarCodigoRecuperacionAsync(string identificador)
        {
            _logger.InfoFormat("Iniciando solicitud de código de recuperación para '{0}'.", 
                identificador);

            var cliente = new PictionaryServidorServicioCodigoVerificacion
                .CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);

            var solicitud = new DTOs.SolicitudRecuperarCuentaDTO
            {
                Identificador = identificador?.Trim(),
                Idioma = ObtenerCodigoIdiomaActual()
            };

            return WcfClienteAyudante.UsarAsincronoAsync(
                cliente,
                c => c.SolicitarCodigoRecuperacionAsync(solicitud));
        }

        /// <summary>
        /// Envia el codigo ingresado por el usuario para validar el registro.
        /// </summary>
        public static Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(
            string tokenCodigo,
            string codigoIngresado)
        {
            _logger.Info("Enviando confirmación de código para registro.");

            var cliente = new PictionaryServidorServicioCodigoVerificacion
                .CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);

            var solicitud = new DTOs.ConfirmacionCodigoDTO
            {
                TokenCodigo = tokenCodigo,
                CodigoIngresado = codigoIngresado?.Trim()
            };

            return WcfClienteAyudante.UsarAsincronoAsync(
                cliente,
                c => c.ConfirmarCodigoVerificacionAsync(solicitud));
        }

        /// <summary>
        /// Envia el codigo ingresado para validar la recuperacion de cuenta.
        /// </summary>
        public static Task<DTOs.ResultadoOperacionDTO> ConfirmarCodigoRecuperacionAsync(
            string tokenCodigo,
            string codigoIngresado)
        {
            _logger.Info("Enviando confirmación de código para recuperación.");

            var cliente = new PictionaryServidorServicioCodigoVerificacion
                .CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);

            var solicitud = new DTOs.ConfirmacionCodigoDTO
            {
                TokenCodigo = tokenCodigo,
                CodigoIngresado = codigoIngresado?.Trim()
            };

            return WcfClienteAyudante.UsarAsincronoAsync(
                cliente,
                c => c.ConfirmarCodigoRecuperacionAsync(solicitud));
        }

        /// <summary>
        /// Solicita el reenvio del codigo de registro.
        /// </summary>
        public static Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(
            string tokenCodigo)
        {
            _logger.Info("Solicitando reenvío de código de registro.");

            var cliente = new PictionaryServidorServicioCuenta.
                CuentaManejadorClient(CuentaEndpoint);
            var solicitud = new DTOs.ReenvioCodigoVerificacionDTO
            {
                TokenCodigo = tokenCodigo?.Trim()
            };

            return WcfClienteAyudante.UsarAsincronoAsync(
                cliente,
                c => c.ReenviarCodigoVerificacionAsync(solicitud));
        }

        /// <summary>
        /// Solicita el reenvio del codigo de recuperacion.
        /// </summary>
        public static Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRecuperacionAsync(
            string tokenCodigo)
        {
            _logger.Info("Solicitando reenvío de código de recuperación.");

            var cliente = new PictionaryServidorServicioCambioContrasena
                .CambioContrasenaManejadorClient(CambioContrasenaEndpoint);

            var solicitud = new DTOs.ReenvioCodigoDTO
            {
                TokenCodigo = tokenCodigo?.Trim()
            };

            return WcfClienteAyudante.UsarAsincronoAsync(
                cliente,
                c => c.ReenviarCodigoRecuperacionAsync(solicitud));
        }

        private static string ObtenerCodigoIdiomaActual()
        {
            return LocalizacionServicio.Instancia.CulturaActual?.Name
                ?? CultureInfo.CurrentUICulture?.Name;
        }
    }
}