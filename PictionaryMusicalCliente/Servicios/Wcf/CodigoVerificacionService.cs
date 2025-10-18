using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using CodigoVerificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioCodigoVerificacion;
using CuentaSrv = PictionaryMusicalCliente.PictionaryServidorServicioCuenta;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class CodigoVerificacionService : ICodigoVerificacionService
    {
        private const string CodigoVerificacionEndpoint = "BasicHttpBinding_ICodigoVerificacionManejador";

        public async Task<ResultadoSolicitudCodigo> SolicitarCodigoRegistroAsync(SolicitudRegistroCuenta solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            var cliente = new CodigoVerificacionSrv.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);

            try
            {
                CodigoVerificacionSrv.NuevaCuentaDTO dto = new CodigoVerificacionSrv.NuevaCuentaDTO
                {
                    Usuario = solicitud.Usuario,
                    Correo = solicitud.Correo,
                    Nombre = solicitud.Nombre,
                    Apellido = solicitud.Apellido,
                    Contrasena = solicitud.Contrasena,
                    AvatarRutaRelativa = solicitud.AvatarRutaRelativa
                };

                CodigoVerificacionSrv.ResultadoSolicitudCodigoDTO resultado = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.SolicitarCodigoVerificacionAsync(dto)).ConfigureAwait(false);

                if (resultado == null)
                {
                    return null;
                }

                return new ResultadoSolicitudCodigo
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

        public async Task<ResultadoSolicitudCodigo> ReenviarCodigoRegistroAsync(string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException("Token requerido", nameof(tokenCodigo));
            }

            var cliente = new CuentaSrv.CuentaManejadorClient("BasicHttpBinding_ICuentaManejador");

            try
            {
                var dto = new CuentaSrv.ReenviarCodigoVerificacionDTO
                {
                    TokenCodigo = tokenCodigo
                };

                CuentaSrv.ResultadoSolicitudCodigoDTO resultado = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.ReenviarCodigoVerificacionAsync(dto)).ConfigureAwait(false);

                if (resultado == null)
                {
                    return null;
                }

                return new ResultadoSolicitudCodigo
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

        public async Task<ResultadoRegistroCuenta> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException("Token requerido", nameof(tokenCodigo));
            }

            if (string.IsNullOrWhiteSpace(codigoIngresado))
            {
                throw new ArgumentException("Código requerido", nameof(codigoIngresado));
            }

            var cliente = new CodigoVerificacionSrv.CodigoVerificacionManejadorClient(CodigoVerificacionEndpoint);

            try
            {
                var dto = new CodigoVerificacionSrv.ConfirmarCodigoDTO
                {
                    TokenCodigo = tokenCodigo,
                    CodigoIngresado = codigoIngresado
                };

                CodigoVerificacionSrv.ResultadoRegistroCuentaDTO resultado = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.ConfirmarCodigoVerificacionAsync(dto)).ConfigureAwait(false);

                if (resultado == null)
                {
                    return null;
                }

                return new ResultadoRegistroCuenta
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
