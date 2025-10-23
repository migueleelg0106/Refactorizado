using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Helpers;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class CambioContrasenaServicio : ICambioContrasenaServicio
    {
        private const string Endpoint = "BasicHttpBinding_ICambiarContrasenaManejador";

        public async Task<DTOs.ResultadoSolicitudRecuperacionDTO> SolicitarCodigoRecuperacionAsync(string identificador)
        {
            if (string.IsNullOrWhiteSpace(identificador))
                throw new ArgumentException(Lang.errorTextoIdentificadorRecuperacionRequerido, nameof(identificador));

            try
            {
                DTOs.ResultadoSolicitudRecuperacionDTO resultado = await CodigoVerificacionServicioAyudante
                    .SolicitarCodigoRecuperacionAsync(identificador)
                    .ConfigureAwait(false);

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
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoServidorSolicitudCambioContrasena);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.OperacionInvalida, Lang.errorTextoPrepararSolicitudCambioContrasena, ex);
            }
        }

        public async Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRecuperacionAsync(string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));

            try
            {
                DTOs.ResultadoSolicitudCodigoDTO resultado = await CodigoVerificacionServicioAyudante
                    .ReenviarCodigoRecuperacionAsync(tokenCodigo)
                    .ConfigureAwait(false);

                if (resultado == null)
                    return null;

                return new DTOs.ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = resultado.CodigoEnviado,
                    Mensaje = resultado.Mensaje,
                    TokenCodigo = resultado.TokenCodigo
                };
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoServidorReenviarCodigo);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.OperacionInvalida, Lang.errorTextoPrepararSolicitudCambioContrasena, ex);
            }
        }

        public async Task<DTOs.ResultadoOperacionDTO> ConfirmarCodigoRecuperacionAsync(string tokenCodigo, string codigoIngresado)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));

            if (string.IsNullOrWhiteSpace(codigoIngresado))
                throw new ArgumentException(Lang.errorTextoCodigoVerificacionRequerido, nameof(codigoIngresado));

            try
            {
                DTOs.ResultadoOperacionDTO resultado = await CodigoVerificacionServicioAyudante
                    .ConfirmarCodigoRecuperacionAsync(tokenCodigo, codigoIngresado)
                    .ConfigureAwait(false);

                if (resultado == null)
                    return null;

                return new DTOs.ResultadoOperacionDTO
                {
                    OperacionExitosa = resultado.OperacionExitosa,
                    Mensaje = resultado.Mensaje
                };
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoServidorValidarCodigo);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.OperacionInvalida, Lang.errorTextoPrepararSolicitudCambioContrasena, ex);
            }
        }

        public async Task<DTOs.ResultadoOperacionDTO> ActualizarContrasenaAsync(string tokenCodigo, string nuevaContrasena)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));

            if (string.IsNullOrWhiteSpace(nuevaContrasena))
                throw new ArgumentNullException(nameof(nuevaContrasena));

            var cliente = new PictionaryServidorServicioCambioContrasena.CambiarContrasenaManejadorClient(Endpoint);

            try
            {
                var solicitud = new DTOs.ActualizarContrasenaDTO
                {
                    TokenCodigo = tokenCodigo,
                    NuevaContrasena = nuevaContrasena
                };

                DTOs.ResultadoOperacionDTO resultado = await WcfClienteAyudante.UsarAsync(
                    cliente,
                    c => c.ActualizarContrasenaAsync(solicitud))
                    .ConfigureAwait(false);

                if (resultado == null)
                    return null;

                return new DTOs.ResultadoOperacionDTO
                {
                    OperacionExitosa = resultado.OperacionExitosa,
                    Mensaje = resultado.Mensaje
                };
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoServidorActualizarContrasena);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.OperacionInvalida, Lang.errorTextoPrepararSolicitudCambioContrasena, ex);
            }
        }
    }
}
