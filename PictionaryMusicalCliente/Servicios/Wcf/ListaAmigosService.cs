using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using ListaSrv = PictionaryMusicalCliente.PictionaryServidorServicioListaAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class ListaAmigosService : IListaAmigosService
    {
        private const string ListaAmigosEndpoint = "NetTcpBinding_IListaAmigosManejador";

        public async Task<IReadOnlyList<Amigo>> ObtenerAmigosAsync(int jugadorId)
        {
            if (jugadorId <= 0)
            {
                throw new ArgumentException("Identificador de jugador inválido.", nameof(jugadorId));
            }

            var cliente = CrearCliente();

            try
            {
                ListaSrv.AmigoDTO[] amigos = await WcfClientHelper
                    .UsarAsync(cliente, c => c.ObtenerListaAmigosAsync(jugadorId))
                    .ConfigureAwait(false);

                if (amigos == null || amigos.Length == 0)
                {
                    return Array.Empty<Amigo>();
                }

                return amigos
                    .Select(AmigosMapper.Convertir)
                    .Where(amigo => amigo != null)
                    .ToList();
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

        private static ListaSrv.ListaAmigosManejadorClient CrearCliente()
        {
            var callback = new ListaAmigosCallback();
            var contexto = new InstanceContext(callback);
            return new ListaSrv.ListaAmigosManejadorClient(contexto, ListaAmigosEndpoint);
        }

        private sealed class ListaAmigosCallback : ListaSrv.IListaAmigosManejadorCallback
        {
            public void SolicitudRecibida(ListaSrv.SolicitudAmistadNotificacionDTO solicitud)
            {
            }

            public void SolicitudActualizada(ListaSrv.SolicitudAmistadEstadoDTO solicitud)
            {
            }

            public void AmigoAgregado(ListaSrv.AmigoDTO amigo)
            {
            }

            public void AmistadEliminada(ListaSrv.AmistadEliminadaDTO amistadEliminada)
            {
            }
        }
    }
}
