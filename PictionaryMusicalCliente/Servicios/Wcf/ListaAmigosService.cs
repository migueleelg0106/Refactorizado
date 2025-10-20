using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.Sesiones;
using ListaSrv = PictionaryMusicalCliente.PictionaryServidorServicioListaAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public sealed class ListaAmigosService : IListaAmigosService, ListaSrv.IListaAmigosManejadorCallback
    {
        private static readonly Lazy<ListaAmigosService> InstanciaInterna = new Lazy<ListaAmigosService>(() => new ListaAmigosService());

        private readonly object _sincronizacion = new object();
        private ListaSrv.ListaAmigosManejadorClient _cliente;

        private ListaAmigosService()
        {
        }

        public static ListaAmigosService Instancia => InstanciaInterna.Value;

        public event EventHandler<ListaAmigosActualizadaEventArgs> ListaActualizada;

        public async Task<ListaAmigosResultado> ObtenerListaAmigosAsync()
        {
            string jugador = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(jugador))
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud);
            }

            ListaSrv.ListaAmigosManejadorClient cliente = ObtenerCliente();

            try
            {
                ListaSrv.ListaAmigosDTO lista = await cliente
                    .ObtenerListaAmigosAsync(jugador)
                    .ConfigureAwait(false);

                return ConvertirResultado(lista);
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

        void ListaSrv.IListaAmigosManejadorCallback.ListaAmigosActualizada(ListaSrv.ListaAmigosDTO lista)
        {
            if (lista == null)
            {
                return;
            }

            ListaAmigosResultado resultado = ConvertirResultado(lista);
            ListaActualizada?.Invoke(this, new ListaAmigosActualizadaEventArgs(
                resultado.Jugador,
                resultado.Amigos));
        }

        private ListaSrv.ListaAmigosManejadorClient ObtenerCliente()
        {
            lock (_sincronizacion)
            {
                if (_cliente != null && _cliente.State != CommunicationState.Closed &&
                    _cliente.State != CommunicationState.Closing && _cliente.State != CommunicationState.Faulted)
                {
                    return _cliente;
                }

                LiberarCliente();

                var contexto = new InstanceContext(this);
                var cliente = new ListaSrv.ListaAmigosManejadorClient(contexto, "NetTcpBinding_IListaAmigosManejador");
                cliente.InnerChannel.Closed += CanalCerrado;
                cliente.InnerChannel.Faulted += CanalCerrado;

                _cliente = cliente;
                return _cliente;
            }
        }

        private void CanalCerrado(object sender, EventArgs e)
        {
            lock (_sincronizacion)
            {
                LiberarCliente();
            }
        }

        private void LiberarCliente()
        {
            if (_cliente == null)
            {
                return;
            }

            try
            {
                _cliente.InnerChannel.Closed -= CanalCerrado;
                _cliente.InnerChannel.Faulted -= CanalCerrado;
                if (_cliente.State == CommunicationState.Opened)
                {
                    _cliente.Close();
                }
                else
                {
                    _cliente.Abort();
                }
            }
            catch
            {
                _cliente.Abort();
            }
            finally
            {
                _cliente = null;
            }
        }

        private static ListaAmigosResultado ConvertirResultado(ListaSrv.ListaAmigosDTO lista)
        {
            if (lista == null)
            {
                return new ListaAmigosResultado(ResultadoOperacion.Fallo(Lang.errorTextoErrorProcesarSolicitud), null, null);
            }

            string mensaje = MensajeServidorHelper.Localizar(lista.Mensaje, Lang.errorTextoErrorProcesarSolicitud);
            ResultadoOperacion resultado = lista.OperacionExitosa
                ? ResultadoOperacion.Exitoso(mensaje)
                : ResultadoOperacion.Fallo(mensaje);

            IReadOnlyList<string> amigos = lista.Amigos != null
                ? new ReadOnlyCollection<string>(lista.Amigos.ToList())
                : new ReadOnlyCollection<string>(Array.Empty<string>());

            return new ListaAmigosResultado(resultado, amigos, lista.Jugador);
        }
    }
}
