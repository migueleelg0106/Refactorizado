using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class CodigoVerificacionService : ICodigoVerificacionService
    {
        private const string CodigoVerificacionEndpoint = "BasicHttpBinding_ICodigoVerificacionManejador";
        private const string CuentaEndpoint = "BasicHttpBinding_ICuentaManejador";

        public async Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(DTOs.SolicitudRegistroCuentaDTO solicitud)
        {
            if (solicitud == null)
                throw new ArgumentNullException(nameof(solicitud));

            var cliente = new PictionaryServidorServicioCodigoVerificacion.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);

            try
            {
                var dto = new DTOs.NuevaCuentaDTO
                {
                    Usuario = solicitud.Usuario,
                    Correo = solicitud.Correo,
                    Nombre = solicitud.Nombre,
                    Apellido = solicitud.Apellido,
                    Contrasena = solicitud.Contrasena,
                    AvatarRutaRelativa = solicitud.AvatarRutaRelativa
                };

                DTOs.ResultadoSolicitudCodigoDTO resultado = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.SolicitarCodigoVerificacionAsync(dto))
                    .ConfigureAwait(false);

                if (resultado == null)
                    return null;

                return new DTOs.ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = resultado.CodigoEnviado,
                    UsuarioYaRegistrado = resultado.UsuarioYaRegistrado,
                    CorreoYaRegistrado = resultado.CorreoYaRegistrado,
                    Mensaje = resultado.Mensaje,
                    TokenCodigo = resultado.TokenCodigo
                };
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorCodigoVerificacion);
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
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public async Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
                throw new ArgumentException("Token requerido", nameof(tokenCodigo));

            var cliente = new PictionaryServidorServicioCuenta.CuentaManejadorClient(CuentaEndpoint);

            try
            {
                var dto = new DTOs.ReenviarCodigoVerificacionDTO
                {
                    TokenCodigo = tokenCodigo
                };

                DTOs.ResultadoSolicitudCodigoDTO resultado = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.ReenviarCodigoVerificacionAsync(dto))
                    .ConfigureAwait(false);

                if (resultado == null)
                    return null;

                return new DTOs.ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = resultado.CodigoEnviado,
                    UsuarioYaRegistrado = resultado.UsuarioYaRegistrado,
                    CorreoYaRegistrado = resultado.CorreoYaRegistrado,
                    Mensaje = resultado.Mensaje,
                    TokenCodigo = resultado.TokenCodigo
                };
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorCodigoVerificacion);
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
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        public async Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
                throw new ArgumentException("Token requerido", nameof(tokenCodigo));

            if (string.IsNullOrWhiteSpace(codigoIngresado))
                throw new ArgumentException("Código requerido", nameof(codigoIngresado));

            var cliente = new PictionaryServidorServicioCodigoVerificacion.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);

            try
            {
                var dto = new DTOs.ConfirmarCodigoDTO
                {
                    TokenCodigo = tokenCodigo,
                    CodigoIngresado = codigoIngresado
                };

                DTOs.ResultadoRegistroCuentaDTO resultado = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.ConfirmarCodigoVerificacionAsync(dto))
                    .ConfigureAwait(false);

                if (resultado == null)
                    return null;

                return new DTOs.ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = resultado.RegistroExitoso,
                    UsuarioYaRegistrado = resultado.UsuarioYaRegistrado,
                    CorreoYaRegistrado = resultado.CorreoYaRegistrado,
                    Mensaje = resultado.Mensaje
                };
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorCodigoVerificacion);
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
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }
    }
}
