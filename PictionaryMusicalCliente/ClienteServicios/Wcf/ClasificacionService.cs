using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class ClasificacionService : IClasificacionService
    {
        private const string ClasificacionEndpoint = "BasicHttpBinding_IClasificacionManejador";

        public async Task<IReadOnlyList<DTOs.ClasificacionUsuarioDTO>> ObtenerTopJugadoresAsync()
        {
            var cliente = new PictionaryServidorServicioClasificacion.ClasificacionManejadorClient(ClasificacionEndpoint);

            try
            {
                DTOs.ClasificacionUsuarioDTO[] clasificacion = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.ObtenerTopJugadoresAsync())
                    .ConfigureAwait(false);

                return clasificacion ?? Array.Empty<DTOs.ClasificacionUsuarioDTO>();
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
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
