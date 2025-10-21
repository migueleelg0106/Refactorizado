using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using ListaAmigosSrv = PictionaryMusicalCliente.PictionaryServidorServicioListaAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class ListaAmigosService : IListaAmigosService, ListaAmigosSrv.IListaAmigosManejadorCallback
    {
        private const string Endpoint = "NetTcpBinding_IListaAmigosManejador";

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly object _amigosLock = new();
        private readonly List<Amigo> _amigos = new();

        private ListaAmigosSrv.ListaAmigosManejadorClient _cliente;
        private string _usuarioSuscrito;

        public event EventHandler<IReadOnlyList<Amigo>> ListaActualizada;

        public IReadOnlyList<Amigo> ListaActual
        {
            get
            {
                lock (_amigosLock)
                {
                    return _amigos.Count == 0
                        ? Array.Empty<Amigo>()
                        : _amigos.ToArray();
                }
            }
        }

        public async Task SuscribirAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));
            }

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (string.Equals(_usuarioSuscrito, nombreUsuario, StringComparison.OrdinalIgnoreCase)
                    && _cliente != null)
                {
                    return;
                }

                await CancelarSuscripcionInternaAsync().ConfigureAwait(false);

                var cliente = CrearCliente();

                try
                {
                    await cliente.SuscribirAsync(nombreUsuario).ConfigureAwait(false);
                    _cliente = cliente;
                    _usuarioSuscrito = nombreUsuario;
                }
                catch (FaultException ex)
                {
                    cliente.Abort();
                    string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                    throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    cliente.Abort();
                    throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (TimeoutException ex)
                {
                    cliente.Abort();
                    throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
                }
                catch (CommunicationException ex)
                {
                    cliente.Abort();
                    throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (InvalidOperationException ex)
                {
                    cliente.Abort();
                    throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task CancelarSuscripcionAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_cliente == null)
                {
                    return;
                }

                if (!string.Equals(_usuarioSuscrito, nombreUsuario, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                await CancelarSuscripcionInternaAsync().ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void ListaAmigosActualizada(ListaAmigosSrv.AmigoDTO[] amigos)
        {
            var lista = Convertir(amigos);

            lock (_amigosLock)
            {
                _amigos.Clear();
                _amigos.AddRange(lista);
            }

            ListaActualizada?.Invoke(this, lista.AsReadOnly());
        }

        public void Dispose()
        {
            _semaphore.Wait();

            try
            {
                CerrarCliente(_cliente);
                _cliente = null;
                _usuarioSuscrito = null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private ListaAmigosSrv.ListaAmigosManejadorClient CrearCliente()
        {
            var contexto = new InstanceContext(this);
            return new ListaAmigosSrv.ListaAmigosManejadorClient(contexto, Endpoint);
        }

        private async Task CancelarSuscripcionInternaAsync()
        {
            ListaAmigosSrv.ListaAmigosManejadorClient cliente = _cliente;
            string usuario = _usuarioSuscrito;
            _cliente = null;
            _usuarioSuscrito = null;

            if (cliente == null)
            {
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(usuario))
                {
                    await cliente.CancelarSuscripcionAsync(usuario).ConfigureAwait(false);
                }

                CerrarCliente(cliente);
            }
            catch (FaultException ex)
            {
                cliente.Abort();
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                cliente.Abort();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                cliente.Abort();
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                cliente.Abort();
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                cliente.Abort();
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        private static void CerrarCliente(ListaAmigosSrv.ListaAmigosManejadorClient cliente)
        {
            if (cliente == null)
            {
                return;
            }

            try
            {
                if (cliente.State == CommunicationState.Faulted)
                {
                    cliente.Abort();
                }
                else
                {
                    cliente.Close();
                }
            }
            catch
            {
                cliente.Abort();
            }
        }

        private static List<Amigo> Convertir(IEnumerable<ListaAmigosSrv.AmigoDTO> amigos)
        {
            var lista = new List<Amigo>();

            if (amigos == null)
            {
                return lista;
            }

            foreach (var amigo in amigos)
            {
                if (amigo == null || string.IsNullOrWhiteSpace(amigo.NombreUsuario))
                {
                    continue;
                }

                lista.Add(new Amigo(amigo.IdUsuario, amigo.NombreUsuario));
            }

            return lista;
        }
    }
}
