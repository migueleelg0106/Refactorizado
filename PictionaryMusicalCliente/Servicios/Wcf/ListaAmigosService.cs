using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using ListaAmigosSrv = PictionaryMusicalCliente.PictionaryServidorServicioListaAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class ListaAmigosService : IListaAmigosService
    {
        private static readonly string[] EndpointsPreferidos =
        {
            "NetTcpBinding_IListaAmigosManejador",
            "WSDualHttpBinding_IListaAmigosManejador"
        };

        private readonly SynchronizationContext _contexto;
        private ListaAmigosSrv.ListaAmigosManejadorClient _cliente;
        private ListaAmigosCallback _callback;

        public ListaAmigosService()
        {
            _contexto = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public event EventHandler<ListaAmigosActualizadaEventArgs> ListaActualizada;

        public async Task<IReadOnlyList<string>> SuscribirseAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException("El usuario es obligatorio", nameof(nombreUsuario));
            }

            await CancelarSuscripcionAsync().ConfigureAwait(false);

            Exception ultimoError = null;
            TipoErrorServicio? tipoUltimoError = null;

            foreach (string endpoint in EndpointsPreferidos)
            {
                try
                {
                    IReadOnlyList<string> amigos = await SuscribirseEnEndpointAsync(nombreUsuario, endpoint)
                        .ConfigureAwait(false);

                    return amigos;
                }
                catch (ServicioException)
                {
                    throw;
                }
                catch (FaultException ex)
                {
                    string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorNoDisponible);
                    throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    ultimoError = ex;
                    tipoUltimoError = TipoErrorServicio.Comunicacion;
                }
                catch (TimeoutException ex)
                {
                    ultimoError = ex;
                    tipoUltimoError = TipoErrorServicio.TiempoAgotado;
                }
                catch (CommunicationException ex)
                {
                    ultimoError = ex;
                    tipoUltimoError = TipoErrorServicio.Comunicacion;
                }
                catch (InvalidOperationException ex)
                {
                    ultimoError = ex;
                    tipoUltimoError = TipoErrorServicio.OperacionInvalida;
                }
            }

            string mensaje = ObtenerMensajePredeterminado(tipoUltimoError);
            throw new ServicioException(tipoUltimoError ?? TipoErrorServicio.Comunicacion, mensaje, ultimoError);
        }

        public Task CancelarSuscripcionAsync()
        {
            ListaAmigosSrv.ListaAmigosManejadorClient cliente = Interlocked.Exchange(ref _cliente, null);
            _callback = null;

            if (cliente == null)
            {
                return Task.CompletedTask;
            }

            try
            {
                if (cliente.State == CommunicationState.Opened)
                {
                    cliente.Close();
                }
                else
                {
                    cliente.Abort();
                }
            }
            catch (CommunicationException)
            {
                cliente.Abort();
            }
            catch (TimeoutException)
            {
                cliente.Abort();
            }
            catch (InvalidOperationException)
            {
                cliente.Abort();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            CancelarSuscripcionAsync().GetAwaiter().GetResult();
        }

        private void ProcesarActualizacion(ListaAmigosSrv.ListaAmigosDTO lista)
        {
            try
            {
                IReadOnlyList<string> amigos = ConvertirLista(lista);
                NotificarLista(amigos, null);
            }
            catch (ServicioException ex)
            {
                NotificarLista(Array.Empty<string>(), ex.Message);
            }
        }

        private void NotificarLista(IReadOnlyList<string> amigos, string mensajeError)
        {
            _contexto.Post(state =>
            {
                var args = (ListaAmigosActualizadaEventArgs)state;
                ListaActualizada?.Invoke(this, args);
            }, new ListaAmigosActualizadaEventArgs(
                amigos?.ToList()?.AsReadOnly() ?? new ReadOnlyCollection<string>(Array.Empty<string>()),
                mensajeError));
        }

        private static IReadOnlyList<string> ConvertirLista(ListaAmigosSrv.ListaAmigosDTO lista)
        {
            if (lista == null)
            {
                return new ReadOnlyCollection<string>(Array.Empty<string>());
            }

            if (!lista.OperacionExitosa)
            {
                string mensaje = string.IsNullOrWhiteSpace(lista.Mensaje)
                    ? Lang.errorTextoServidorNoDisponible
                    : lista.Mensaje;
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje);
            }

            IEnumerable<string> amigos = lista.Amigos ?? Array.Empty<string>();
            var valores = amigos
                .Where(amigo => !string.IsNullOrWhiteSpace(amigo))
                .Select(amigo => amigo.Trim())
                .Where(amigo => !string.IsNullOrWhiteSpace(amigo))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(amigo => amigo, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            return new ReadOnlyCollection<string>(valores);
        }

        private static void CerrarCliente(ICommunicationObject cliente)
        {
            if (cliente == null)
            {
                return;
            }

            try
            {
                if (cliente.State == CommunicationState.Opened)
                {
                    cliente.Close();
                }
                else
                {
                    cliente.Abort();
                }
            }
            catch (CommunicationException)
            {
                cliente.Abort();
            }
            catch (TimeoutException)
            {
                cliente.Abort();
            }
            catch (InvalidOperationException)
            {
                cliente.Abort();
            }
        }

        private async Task<IReadOnlyList<string>> SuscribirseEnEndpointAsync(string nombreUsuario, string endpoint)
        {
            var callback = new ListaAmigosCallback(ProcesarActualizacion);
            var contexto = new InstanceContext(callback);
            var cliente = new ListaAmigosSrv.ListaAmigosManejadorClient(contexto, endpoint);

            try
            {
                ListaAmigosSrv.ListaAmigosDTO listaDto = await cliente
                    .ObtenerListaAmigosAsync(nombreUsuario)
                    .ConfigureAwait(false);

                IReadOnlyList<string> amigos = ConvertirLista(listaDto);

                _callback = callback;
                _cliente = cliente;

                NotificarLista(amigos, null);

                return amigos;
            }
            catch
            {
                CerrarCliente(cliente);
                throw;
            }
        }

        private static string ObtenerMensajePredeterminado(TipoErrorServicio? tipoError)
        {
            switch (tipoError)
            {
                case TipoErrorServicio.TiempoAgotado:
                    return Lang.avisoTextoServidorTiempoSesion;
                case TipoErrorServicio.OperacionInvalida:
                    return Lang.errorTextoErrorProcesarSolicitud;
                default:
                    return Lang.errorTextoServidorNoDisponible;
            }
        }

        private class ListaAmigosCallback : ListaAmigosSrv.IListaAmigosManejadorCallback
        {
            private readonly Action<ListaAmigosSrv.ListaAmigosDTO> _notificar;

            public ListaAmigosCallback(Action<ListaAmigosSrv.ListaAmigosDTO> notificar)
            {
                _notificar = notificar ?? throw new ArgumentNullException(nameof(notificar));
            }

            public void ListaAmigosActualizada(ListaAmigosSrv.ListaAmigosDTO lista)
            {
                _notificar(lista);
            }
        }
    }
}
