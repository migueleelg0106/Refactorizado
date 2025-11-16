using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    public sealed class ListaAmigosServicio : IListaAmigosServicio, PictionaryServidorServicioListaAmigos.IListaAmigosManejadorCallback
    {
        private const string Endpoint = "NetTcpBinding_IListaAmigosManejador";

        private readonly SemaphoreSlim _semaforo = new(1, 1);
        private readonly object _amigosBloqueo = new();
        private readonly List<DTOs.AmigoDTO> _amigos = new();

        private PictionaryServidorServicioListaAmigos.ListaAmigosManejadorClient _cliente;
        private string _usuarioSuscrito;

        public event EventHandler<IReadOnlyList<DTOs.AmigoDTO>> ListaActualizada;

        public IReadOnlyList<DTOs.AmigoDTO> ListaActual
        {
            get
            {
                lock (_amigosBloqueo)
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

            await _semaforo.WaitAsync().ConfigureAwait(false);

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
                    throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    cliente.Abort();
                    throw new ServicioExcepcion(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (TimeoutException ex)
                {
                    cliente.Abort();
                    throw new ServicioExcepcion(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
                }
                catch (CommunicationException ex)
                {
                    cliente.Abort();
                    throw new ServicioExcepcion(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
                }
                catch (InvalidOperationException ex)
                {
                    cliente.Abort();
                    throw new ServicioExcepcion(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
                }
            }
            finally
            {
                _semaforo.Release();
            }
        }

        public async Task CancelarSuscripcionAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return;

            await _semaforo.WaitAsync().ConfigureAwait(false);

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
                _semaforo.Release();
            }
        }

        public async Task<IReadOnlyList<DTOs.AmigoDTO>> ObtenerAmigosAsync(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));

            await _semaforo.WaitAsync().ConfigureAwait(false);

            PictionaryServidorServicioListaAmigos.ListaAmigosManejadorClient cliente = null;
            bool esClienteTemporal = false;

            try
            {
                cliente = _cliente ?? CrearCliente();
                esClienteTemporal = _cliente == null;

                var amigos = await cliente.ObtenerAmigosAsync(nombreUsuario).ConfigureAwait(false);
                var lista = Convertir(amigos);

                if (!esClienteTemporal)
                {
                    ActualizarListaInterna(lista);
                }
                else
                {
                    CerrarCliente(cliente);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw ManejarExcepcionWcf(ex, cliente, esClienteTemporal);
            }
            finally
            {
                _semaforo.Release();
            }
        }

        private static Exception ManejarExcepcionWcf(Exception ex, PictionaryServidorServicioListaAmigos.ListaAmigosManejadorClient cliente, bool esTemporal)
        {
            if (esTemporal && cliente != null)
            {
                cliente.Abort();
            }

            return ex switch
            {
                FaultException fe => new ServicioExcepcion(
                    TipoErrorServicio.FallaServicio,
                    ErrorServicioAyudante.ObtenerMensaje(fe, Lang.errorTextoErrorProcesarSolicitud),
                    fe),

                EndpointNotFoundException or CommunicationException => new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex),

                TimeoutException => new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex),

                InvalidOperationException => new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex),

                _ => ex
            };
        }

        public void NotificarListaAmigosActualizada(DTOs.AmigoDTO[] amigos)
        {
            var lista = Convertir(amigos);

            lock (_amigosBloqueo)
            {
                _amigos.Clear();
                _amigos.AddRange(lista);
            }

            ListaActualizada?.Invoke(this, lista);
        }

        public void Dispose()
        {
            CerrarCliente(_cliente);
            _cliente = null;
            _usuarioSuscrito = null;

            _semaforo?.Dispose();
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
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                cliente.Abort();
                throw new ServicioExcepcion(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                cliente.Abort();
                throw new ServicioExcepcion(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                cliente.Abort();
                throw new ServicioExcepcion(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                cliente.Abort();
                throw new ServicioExcepcion(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
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
            catch (CommunicationException)
            {
                cliente.Abort();
            }
            catch (TimeoutException)
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
                    UsuarioId = amigo.UsuarioId,
                    NombreUsuario = amigo.NombreUsuario
                })
                .ToList();

            return lista.Count == 0 ? Array.Empty<DTOs.AmigoDTO>() : lista.AsReadOnly();
        }

        private void ActualizarListaInterna(IReadOnlyList<DTOs.AmigoDTO> lista)
        {
            if (lista == null)
            {
                return;
            }

            lock (_amigosBloqueo)
            {
                _amigos.Clear();
                _amigos.AddRange(lista);
            }
        }
    }
}
