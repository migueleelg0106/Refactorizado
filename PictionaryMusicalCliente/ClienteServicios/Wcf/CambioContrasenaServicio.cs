using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    public class CambioContrasenaServicio : ICambioContrasenaServicio
    {
        private const string Endpoint = "BasicHttpBinding_ICambioContrasenaManejador";

        public async Task<DTOs.ResultadoSolicitudRecuperacionDTO> SolicitarCodigoRecuperacionAsync(string identificador)
        {
            if (string.IsNullOrWhiteSpace(identificador))
                throw new ArgumentException(Lang.errorTextoIdentificadorRecuperacionRequerido, nameof(identificador));

            DTOs.ResultadoSolicitudRecuperacionDTO resultado = await EjecutarConManejoDeErroresAsync(
                () => CodigoVerificacionServicioAyudante.SolicitarCodigoRecuperacionAsync(identificador),
                Lang.errorTextoServidorSolicitudCambioContrasena
            ).ConfigureAwait(false);

            if (resultado == null)
                return null;

            return new DTOs.ResultadoSolicitudRecuperacionDTO
            {
                CuentaEncontrada = resultado.CuentaEncontrada,
                CodigoEnviado = resultado.CodigoEnviado,
                CorreoDestino = resultado.CorreoDestino,
                Mensaje = resultado.Mensaje,
                TokenCodigo = resultado.TokenCodigo
            };
        }

        public async Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRecuperacionAsync(string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));

            DTOs.ResultadoSolicitudCodigoDTO resultado = await EjecutarConManejoDeErroresAsync(
                () => CodigoVerificacionServicioAyudante.ReenviarCodigoRecuperacionAsync(tokenCodigo),
                Lang.errorTextoServidorReenviarCodigo
            ).ConfigureAwait(false);

            if (resultado == null)
                return null;

            return new DTOs.ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = resultado.CodigoEnviado,
                Mensaje = resultado.Mensaje,
                TokenCodigo = resultado.TokenCodigo
            };
        }

        public async Task<DTOs.ResultadoOperacionDTO> ConfirmarCodigoRecuperacionAsync(string tokenCodigo, string codigoIngresado)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));

            if (string.IsNullOrWhiteSpace(codigoIngresado))
                throw new ArgumentException(Lang.errorTextoCodigoVerificacionRequerido, nameof(codigoIngresado));

            DTOs.ResultadoOperacionDTO resultado = await EjecutarConManejoDeErroresAsync(
                () => CodigoVerificacionServicioAyudante.ConfirmarCodigoRecuperacionAsync(tokenCodigo, codigoIngresado),
                Lang.errorTextoServidorValidarCodigo
            ).ConfigureAwait(false);

            if (resultado == null)
                return null;

            return new DTOs.ResultadoOperacionDTO
            {
                OperacionExitosa = resultado.OperacionExitosa,
                Mensaje = resultado.Mensaje
            };
        }

        public async Task<DTOs.ResultadoOperacionDTO> ActualizarContrasenaAsync(string tokenCodigo, string nuevaContrasena)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));

            if (string.IsNullOrWhiteSpace(nuevaContrasena))
                throw new ArgumentNullException(nameof(nuevaContrasena));

            var cliente = new PictionaryServidorServicioCambioContrasena.CambioContrasenaManejadorClient(Endpoint);
            var solicitud = new DTOs.ActualizacionContrasenaDTO
            {
                TokenCodigo = tokenCodigo,
                NuevaContrasena = nuevaContrasena
            };

            DTOs.ResultadoOperacionDTO resultado = await EjecutarConManejoDeErroresAsync(
                () => WcfClienteAyudante.UsarAsincronoAsync(cliente, c => c.ActualizarContrasenaAsync(solicitud)),
                Lang.errorTextoServidorActualizarContrasena
            ).ConfigureAwait(false);

            if (resultado == null)
                return null;

            return new DTOs.ResultadoOperacionDTO
            {
                OperacionExitosa = resultado.OperacionExitosa,
                Mensaje = resultado.Mensaje
            };
        }

        private static async Task<TResult> EjecutarConManejoDeErroresAsync<TResult>(
            Func<Task<TResult>> operacion,
            string mensajeFallaPredeterminado)
        {
            try
            {
                return await operacion().ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, mensajeFallaPredeterminado);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioExcepcion(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioExcepcion(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioExcepcion(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioExcepcion(TipoErrorServicio.OperacionInvalida, Lang.errorTextoPrepararSolicitudCambioContrasena, ex);
            }
        }
    }
}