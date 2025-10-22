using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using CodigoVerificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioCodigoVerificacion;
using CuentaSrv = PictionaryMusicalCliente.PictionaryServidorServicioCuenta;
using Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class VerificarCodigoService : IVerificarCodigoService
    {
        public async Task<ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));
            }

            CodigoVerificacionSrv.ResultadoRegistroCuentaDTO resultado = await EjecutarOperacionAsync(
                () => CodigoVerificacionServicioHelper.ConfirmarCodigoRegistroAsync(tokenCodigo, codigoIngresado),
                Lang.errorTextoServidorValidarCodigo).ConfigureAwait(false);

            if (resultado == null)
            {
                return null;
            }

            return new ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = resultado.RegistroExitoso,
                UsuarioYaRegistrado = resultado.UsuarioYaRegistrado,
                CorreoYaRegistrado = resultado.CorreoYaRegistrado,
                Mensaje = resultado.Mensaje
            };
        }

        public async Task<ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));
            }

            CuentaSrv.ResultadoSolicitudCodigoDTO resultado = await EjecutarOperacionAsync(
                () => CodigoVerificacionServicioHelper.ReenviarCodigoRegistroAsync(tokenCodigo),
                Lang.errorTextoServidorReenviarCodigo).ConfigureAwait(false);

            if (resultado == null)
            {
                return null;
            }

            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = resultado.CodigoEnviado,
                UsuarioYaRegistrado = resultado.UsuarioYaRegistrado,
                CorreoYaRegistrado = resultado.CorreoYaRegistrado,
                Mensaje = resultado.Mensaje,
                TokenCodigo = resultado.TokenCodigo
            };
        }

        private static async Task<T> EjecutarOperacionAsync<T>(Func<Task<T>> operacion, string mensajeErrorPredeterminado)
        {
            try
            {
                return await operacion().ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, mensajeErrorPredeterminado);
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
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }
    }
}
