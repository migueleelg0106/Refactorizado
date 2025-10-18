using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using CambioContrasenaSrv = PictionaryMusicalCliente.PictionaryServidorServicioCambioContrasena;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class CambioContrasenaService : ICambioContrasenaService
    {
        private const string Endpoint = "BasicHttpBinding_ICambiarContrasenaManejador";

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

                return new ResultadoOperacion
                {
                    OperacionExitosa = resultado.OperacionExitosa,
                    Mensaje = resultado.Mensaje
                };
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
