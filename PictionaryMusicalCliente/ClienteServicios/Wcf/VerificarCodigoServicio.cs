using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Helpers;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class VerificarCodigoServicio : IVerificarCodigoServicio
    {
        public async Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));
            }

            DTOs.ResultadoRegistroCuentaDTO resultado = await EjecutarOperacionAsync(
                () => CodigoVerificacionServicioAyudante.ConfirmarCodigoRegistroAsync(tokenCodigo, codigoIngresado),
                Lang.errorTextoServidorValidarCodigo).ConfigureAwait(false);

            if (resultado == null)
            {
                return null;
            }

            return resultado;
        }

        public async Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));
            }

            DTOs.ResultadoSolicitudCodigoDTO resultado = await EjecutarOperacionAsync(
                () => CodigoVerificacionServicioAyudante.ReenviarCodigoRegistroAsync(tokenCodigo),
                Lang.errorTextoServidorReenviarCodigo).ConfigureAwait(false);

            if (resultado == null)
            {
                return null;
            }

            return resultado;
        }

        private static async Task<T> EjecutarOperacionAsync<T>(Func<Task<T>> operacion, string mensajeErrorPredeterminado)
        {
            try
            {
                return await operacion().ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, mensajeErrorPredeterminado);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }
    }
}
