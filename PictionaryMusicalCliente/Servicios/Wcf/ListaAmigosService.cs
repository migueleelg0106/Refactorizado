using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using ListaAmigosSrv = PictionaryMusicalCliente.PictionaryServidorServicioListaAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class ListaAmigosService : IListaAmigosService, ListaAmigosSrv.IListaAmigosManejadorCallback
    {
        private const string ListaAmigosEndpoint = "NetTcpBinding_IListaAmigosManejador";

        public async Task<IReadOnlyList<string>> ObtenerListaAmigosAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));
            }

            var contexto = new InstanceContext(this);
            var cliente = new ListaAmigosSrv.ListaAmigosManejadorClient(contexto, ListaAmigosEndpoint);

            try
            {
                ListaAmigosSrv.ListaAmigosDTO resultado = await WcfClientHelper
                    .UsarAsync(cliente, c => c.ObtenerListaAmigosAsync(nombreUsuario))
                    .ConfigureAwait(false);

                if (resultado == null)
                {
                    return Array.Empty<string>();
                }

                if (!resultado.OperacionExitosa)
                {
                    string mensaje = MensajeServidorHelper.Localizar(
                        resultado.Mensaje,
                        Lang.errorTextoErrorProcesarSolicitud);

                    throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje);
                }

                return resultado.Amigos?
                    .Where(amigo => !string.IsNullOrWhiteSpace(amigo))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
                    ?? new List<string>();
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
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

        public void ListaAmigosActualizada(ListaAmigosSrv.ListaAmigosDTO lista)
        {
            // La ventana principal solo necesita la carga inicial por ahora.
        }
    }
}
