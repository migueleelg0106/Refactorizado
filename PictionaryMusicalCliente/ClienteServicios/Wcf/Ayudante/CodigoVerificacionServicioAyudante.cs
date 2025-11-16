using System;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante
{
    public static class CodigoVerificacionServicioAyudante
    {
        private const string CodigoVerificacionEndpoint = "BasicHttpBinding_ICodigoVerificacionManejador";
        private const string CuentaEndpoint = "BasicHttpBinding_ICuentaManejador";
        private const string CambioContrasenaEndpoint = "BasicHttpBinding_ICambioContrasenaManejador";

        public static Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(DTOs.NuevaCuentaDTO solicitud)
        {
            if (solicitud == null)
                throw new ArgumentNullException(nameof(solicitud));

            var cliente = new PictionaryServidorServicioCodigoVerificacion.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);
            return WcfClienteAyudante.UsarAsincronoAsync(cliente, c => c.SolicitarCodigoVerificacionAsync(solicitud));
        }

        public static Task<DTOs.ResultadoSolicitudRecuperacionDTO> SolicitarCodigoRecuperacionAsync(string identificador)
        {
            var cliente = new PictionaryServidorServicioCodigoVerificacion.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);
            var solicitud = new DTOs.SolicitudRecuperarCuentaDTO
            {
                Identificador = identificador?.Trim()
            };

            return WcfClienteAyudante.UsarAsincronoAsync(cliente, c => c.SolicitarCodigoRecuperacionAsync(solicitud));
        }

        public static Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado)
        {
            var cliente = new PictionaryServidorServicioCodigoVerificacion.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);
            var solicitud = new DTOs.ConfirmacionCodigoDTO
            {
                TokenCodigo = tokenCodigo,
                CodigoIngresado = codigoIngresado?.Trim()
            };

            return WcfClienteAyudante.UsarAsincronoAsync(cliente, c => c.ConfirmarCodigoVerificacionAsync(solicitud));
        }

        public static Task<DTOs.ResultadoOperacionDTO> ConfirmarCodigoRecuperacionAsync(string tokenCodigo, string codigoIngresado)
        {
            var cliente = new PictionaryServidorServicioCodigoVerificacion.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);
            var solicitud = new DTOs.ConfirmacionCodigoDTO
            {
                TokenCodigo = tokenCodigo,
                CodigoIngresado = codigoIngresado?.Trim()
            };

            return WcfClienteAyudante.UsarAsincronoAsync(cliente, c => c.ConfirmarCodigoRecuperacionAsync(solicitud));
        }

        public static Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo)
        {
            var cliente = new PictionaryServidorServicioCuenta.CuentaManejadorClient(CuentaEndpoint);
            var solicitud = new DTOs.ReenvioCodigoVerificacionDTO
            {
                TokenCodigo = tokenCodigo?.Trim()
            };

            return WcfClienteAyudante.UsarAsincronoAsync(cliente, c => c.ReenviarCodigoVerificacionAsync(solicitud));
        }

        public static Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRecuperacionAsync(string tokenCodigo)
        {
            var cliente = new PictionaryServidorServicioCambioContrasena.CambioContrasenaManejadorClient(CambioContrasenaEndpoint);
            var solicitud = new DTOs.ReenvioCodigoDTO
            {
                TokenCodigo = tokenCodigo?.Trim()
            };

            return WcfClienteAyudante.UsarAsincronoAsync(cliente, c => c.ReenviarCodigoRecuperacionAsync(solicitud));
        }
    }
}
