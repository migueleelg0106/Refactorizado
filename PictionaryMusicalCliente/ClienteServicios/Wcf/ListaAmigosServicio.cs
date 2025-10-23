using Datos.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class ListaAmigosServicio : IListaAmigosServicio, PictionaryServidorServicioListaAmigos.IListaAmigosManejadorCallback
    {
        private const string Endpoint = "NetTcpBinding_IListaAmigosManejador";

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly object _amigosLock = new();
        private readonly List<DTOs.AmigoDTO> _amigos = new();

        private PictionaryServidorServicioListaAmigos.ListaAmigosManejadorClient _cliente;
        private string _usuarioSuscrito;

        public event EventHandler<IReadOnlyList<DTOs.AmigoDTO>> ListaActualizada;

        public IReadOnlyList<DTOs.AmigoDTO> ListaActual
        {
            get
            {
                lock (_amigosLock)
                {
                    return _amigos.Count == 0
                        ? Array.Empty<DTOs.AmigoDTO>()
                        : _amigos.ToArray();
                }
            }
        }

        public async Task SuscribirAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));

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
                    string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                    throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    cliente.Abort();
                    throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (TimeoutException ex)
                {
                    cliente.Abort();
                    throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
                }
                catch (CommunicationException ex)
                {
                    cliente.Abort();
                    throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (InvalidOperationException ex)
                {
                    cliente.Abort();
                    throw new ExcepcionServicio(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
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
                return;

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_cliente == null)
                    return;

                if (!string.Equals(_usuarioSuscrito, nombreUsuario, StringComparison.OrdinalIgnoreCase))
                    return;

                await CancelarSuscripcionInternaAsync().ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void ListaAmigosActualizada(DTOs.AmigoDTO[] amigos)
        {
            var lista = Convertir(amigos);

            lock (_amigosLock)
            {
                _amigos.Clear();
                _amigos.AddRange(lista);
            }

            ListaActualizada?.Invoke(this, lista);
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

        private PictionaryServidorServicioListaAmigos.ListaAmigosManejadorClient CrearCliente()
        {
            var contexto = new InstanceContext(this);
            return new PictionaryServidorServicioListaAmigos.ListaAmigosManejadorClient(contexto, Endpoint);
        }

        private async Task CancelarSuscripcionInternaAsync()
        {
            var cliente = _cliente;
            var usuario = _usuarioSuscrito;
            _cliente = null;
            _usuarioSuscrito = null;

            if (cliente == null)
                return;

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
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoErrorProcesarSolicitud);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                cliente.Abort();
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                cliente.Abort();
                throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                cliente.Abort();
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                cliente.Abort();
                throw new ExcepcionServicio(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }

        private static void CerrarCliente(PictionaryServidorServicioListaAmigos.ListaAmigosManejadorClient cliente)
        {
            if (cliente == null)
                return;

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

        private static IReadOnlyList<DTOs.AmigoDTO> Convertir(IEnumerable<DTOs.AmigoDTO> amigos)
        {
            if (amigos == null)
                return Array.Empty<DTOs.AmigoDTO>();

            var lista = amigos
                .Where(amigo => amigo != null && !string.IsNullOrWhiteSpace(amigo.NombreUsuario))
                .Select(amigo => new DTOs.AmigoDTO
                {
                    IdUsuario = amigo.IdUsuario,
                    NombreUsuario = amigo.NombreUsuario
                })
                .ToList();

            return lista.Count == 0 ? Array.Empty<DTOs.AmigoDTO>() : lista.AsReadOnly();
        }
    }
}
