using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using CambioContrasenaSrv = PictionaryMusicalCliente.PictionaryServidorServicioCambioContrasena;
using CodigoVerificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioCodigoVerificacion;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class CambioContrasenaService : ICambioContrasenaService
    {
        private const string Endpoint = "BasicHttpBinding_ICambiarContrasenaManejador";

        public async Task<ResultadoSolicitudRecuperacion> SolicitarCodigoRecuperacionAsync(string identificador)
        {
            if (string.IsNullOrWhiteSpace(identificador))
            {
                throw new ArgumentException(Lang.errorTextoIdentificadorRecuperacionRequerido, nameof(identificador));
            }

            try
            {
                CodigoVerificacionSrv.ResultadoSolicitudRecuperacionDTO resultado = await CodigoVerificacionServicioHelper
                    .SolicitarCodigoRecuperacionAsync(identificador).ConfigureAwait(false);

                if (resultado == null)
                {
                    return null;
                }

                return new ResultadoSolicitudRecuperacion
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
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorSolicitudCambioContrasena);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoPrepararSolicitudCambioContrasena, ex);
            }
        }

        public async Task<ResultadoSolicitudCodigo> ReenviarCodigoRecuperacionAsync(string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));
            }

            try
            {
                CambioContrasenaSrv.ResultadoSolicitudCodigoDTO resultado = await CodigoVerificacionServicioHelper
                    .ReenviarCodigoRecuperacionAsync(tokenCodigo).ConfigureAwait(false);

                if (resultado == null)
                {
                    return null;
                }

                return new ResultadoSolicitudCodigo
                {
                    CodigoEnviado = resultado.CodigoEnviado,
                    Mensaje = resultado.Mensaje,
                    TokenCodigo = resultado.TokenCodigo
                };
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorReenviarCodigo);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoPrepararSolicitudCambioContrasena, ex);
            }
        }

        public async Task<ResultadoOperacion> ConfirmarCodigoRecuperacionAsync(string tokenCodigo, string codigoIngresado)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));
            }

            if (string.IsNullOrWhiteSpace(codigoIngresado))
            {
                throw new ArgumentException(Lang.errorTextoCodigoVerificacionRequerido, nameof(codigoIngresado));
            }

            try
            {
                CodigoVerificacionSrv.ResultadoOperacionDTO resultado = await CodigoVerificacionServicioHelper
                    .ConfirmarCodigoRecuperacionAsync(tokenCodigo, codigoIngresado).ConfigureAwait(false);

                if (resultado == null)
                {
                    return null;
                }

                return resultado.OperacionExitosa
                    ? ResultadoOperacion.Exitoso(resultado.Mensaje)
                    : ResultadoOperacion.Fallo(resultado.Mensaje);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorValidarCodigo);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoPrepararSolicitudCambioContrasena, ex);
            }
        }

        public async Task<ResultadoOperacion> ActualizarContrasenaAsync(string tokenCodigo, string nuevaContrasena)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));
            }

            if (nuevaContrasena == null)
            {
                throw new ArgumentNullException(nameof(nuevaContrasena));
            }

            var cliente = new CambioContrasenaSrv.CambiarContrasenaManejadorClient(Endpoint);

            try
            {
                var solicitud = new CambioContrasenaSrv.ActualizarContrasenaDTO
                {
                    TokenCodigo = tokenCodigo,
                    NuevaContrasena = nuevaContrasena
                };

                CambioContrasenaSrv.ResultadoOperacionDTO resultado = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.ActualizarContrasenaAsync(solicitud)).ConfigureAwait(false);

                if (resultado == null)
                {
                    return null;
                }

                return resultado.OperacionExitosa
                    ? ResultadoOperacion.Exitoso(resultado.Mensaje)
                    : ResultadoOperacion.Fallo(resultado.Mensaje);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorActualizarContrasena);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoPrepararSolicitudCambioContrasena,
                    ex);
            }
        }
    }
}
