using System;
using System.Threading.Tasks;
using CambioContrasenaSrv = PictionaryMusicalCliente.PictionaryServidorServicioCambioContrasena;
using CodigoVerificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioCodigoVerificacion;
using CuentaSrv = PictionaryMusicalCliente.PictionaryServidorServicioCuenta;

namespace PictionaryMusicalCliente.Servicios.Wcf.Helpers
{
    internal static class CodigoVerificacionServicioHelper
    {
        private const string CodigoVerificacionEndpoint = "BasicHttpBinding_ICodigoVerificacionManejador";
        private const string CuentaEndpoint = "BasicHttpBinding_ICuentaManejador";
        private const string CambioContrasenaEndpoint = "BasicHttpBinding_ICambiarContrasenaManejador";

        public static Task<CodigoVerificacionSrv.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(
            CodigoVerificacionSrv.NuevaCuentaDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            var cliente = new CodigoVerificacionSrv.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);
            return WcfClientHelper.UsarAsync(cliente, c => c.SolicitarCodigoVerificacionAsync(solicitud));
        }

        public static Task<CodigoVerificacionSrv.ResultadoSolicitudRecuperacionDTO> SolicitarCodigoRecuperacionAsync(
            string identificador)
        {
            var cliente = new CodigoVerificacionSrv.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);
            var solicitud = new CodigoVerificacionSrv.SolicitudRecuperarCuentaDTO
            {
                Identificador = identificador?.Trim()
            };

            return WcfClientHelper.UsarAsync(cliente, c => c.SolicitarCodigoRecuperacionAsync(solicitud));
        }

        public static Task<CodigoVerificacionSrv.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(
            string tokenCodigo,
            string codigoIngresado)
        {
            var cliente = new CodigoVerificacionSrv.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);
            var solicitud = new CodigoVerificacionSrv.ConfirmarCodigoDTO
            {
                TokenCodigo = tokenCodigo,
                CodigoIngresado = codigoIngresado?.Trim()
            };

            return WcfClientHelper.UsarAsync(cliente, c => c.ConfirmarCodigoVerificacionAsync(solicitud));
        }

        public static Task<CodigoVerificacionSrv.ResultadoOperacionDTO> ConfirmarCodigoRecuperacionAsync(
            string tokenCodigo,
            string codigoIngresado)
        {
            var cliente = new CodigoVerificacionSrv.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);
            var solicitud = new CodigoVerificacionSrv.ConfirmarCodigoDTO
            {
                TokenCodigo = tokenCodigo,
                CodigoIngresado = codigoIngresado?.Trim()
            };

            return WcfClientHelper.UsarAsync(cliente, c => c.ConfirmarCodigoRecuperacionAsync(solicitud));
        }

        public static Task<CuentaSrv.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo)
        {
            var cliente = new CuentaSrv.CuentaManejadorClient(CuentaEndpoint);
            var solicitud = new CuentaSrv.ReenviarCodigoVerificacionDTO
            {
                TokenCodigo = tokenCodigo?.Trim()
            };

            return WcfClientHelper.UsarAsync(cliente, c => c.ReenviarCodigoVerificacionAsync(solicitud));
        }

        public static Task<CambioContrasenaSrv.ResultadoSolicitudCodigoDTO> ReenviarCodigoRecuperacionAsync(string tokenCodigo)
        {
            var cliente = new CambioContrasenaSrv.CambiarContrasenaManejadorClient(CambioContrasenaEndpoint);
            var solicitud = new CambioContrasenaSrv.ReenviarCodigoDTO
            {
                TokenCodigo = tokenCodigo?.Trim()
            };

            return WcfClientHelper.UsarAsync(cliente, c => c.ReenviarCodigoRecuperacionAsync(solicitud));
        }
    }
}
