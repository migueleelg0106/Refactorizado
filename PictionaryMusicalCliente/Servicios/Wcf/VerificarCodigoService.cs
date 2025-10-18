using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using CodigoVerificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioCodigoVerificacion;
using ReenvioSrv = PictionaryMusicalCliente.PictionaryServidorServicioReenvioCodigoVerificacion;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class VerificarCodigoService : IVerificarCodigoService
    {
        public async Task<ConfirmacionCodigoResultado> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado)
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

            return new ConfirmacionCodigoResultado(resultado.RegistroExitoso, resultado.Mensaje);
        }

        public async Task<ReenvioCodigoResultado> ReenviarCodigoRegistroAsync(string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));
            }

            ReenvioSrv.ResultadoSolicitudCodigoDTO resultado = await EjecutarOperacionAsync(
                () => CodigoVerificacionServicioHelper.ReenviarCodigoRegistroAsync(tokenCodigo),
                Lang.errorTextoServidorReenviarCodigo).ConfigureAwait(false);

            if (resultado == null)
            {
                return null;
            }

            return new ReenvioCodigoResultado(resultado.CodigoEnviado, resultado.Mensaje, resultado.TokenCodigo);
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
