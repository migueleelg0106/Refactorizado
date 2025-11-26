using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Gestiona la comunicacion y eventos en tiempo real para las salas de juego.
    /// </summary>
    public sealed class SalasServicio : ISalasServicio,
        PictionaryServidorServicioSalas.ISalasManejadorCallback
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SalasServicio));
        private const string Endpoint = "NetTcpBinding_ISalasManejador";

        private readonly SemaphoreSlim _semaforo = new(1, 1);
        private readonly object _salasBloqueo = new();
        private readonly List<DTOs.SalaDTO> _salas = new();

        private PictionaryServidorServicioSalas.SalasManejadorClient _cliente;
        private bool _suscrito;

        /// <summary>
        /// Evento disparado cuando un jugador entra a la sala actual.
        /// </summary>
        public event EventHandler<string> JugadorSeUnio;

        /// <summary>
        /// Evento disparado cuando un jugador sale de la sala actual.
        /// </summary>
        public event EventHandler<string> JugadorSalio;

        /// <summary>
        /// Evento disparado cuando un jugador es expulsado por el anfitrion.
        /// </summary>
        public event EventHandler<string> JugadorExpulsado;

        /// <summary>
        /// Evento disparado cuando la sala es cancelada por el anfitrion.
        /// </summary>
        public event EventHandler<string> SalaCancelada;

        /// <summary>
        /// Evento disparado cuando se actualiza la lista de salas disponibles.
        /// </summary>
        public event EventHandler<IReadOnlyList<DTOs.SalaDTO>> ListaSalasActualizada;

        /// <summary>
        /// Evento disparado cuando cambian las propiedades de la sala actual.
        /// </summary>
        public event EventHandler<DTOs.SalaDTO> SalaActualizada;

        /// <summary>
        /// Obtiene la lista de salas almacenada localmente.
        /// </summary>
        public IReadOnlyList<DTOs.SalaDTO> ListaSalasActual
        {
            get
            {
                lock (_salasBloqueo)
                {
                    return _salas.Count == 0
                        ? Array.Empty<DTOs.SalaDTO>()
                        : _salas.ToArray();
                }
            }
        }

        /// <summary>
        /// Solicita la creacion de una nueva sala.
        /// </summary>
        public async Task<DTOs.SalaDTO> CrearSalaAsync(
            string nombreCreador,
            DTOs.ConfiguracionPartidaDTO configuracion)
        {
            if (string.IsNullOrWhiteSpace(nombreCreador))
            {
                throw new ArgumentException(
                    "El nombre de creador es obligatorio.",
                    nameof(nombreCreador));
            }

            if (configuracion == null)
            {
                throw new ArgumentNullException(nameof(configuracion));
            }

            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                var cliente = ObtenerOCrearCliente();

                try
                {
                    var sala = await cliente.CrearSalaAsync(
                        nombreCreador,
                        configuracion).ConfigureAwait(false);

                    _logger.InfoFormat("Sala creada exitosamente por '{0}'. Código: {1}", 
                        nombreCreador, sala.Codigo);
                    return sala;
                }
                catch (FaultException ex)
                {
                    _logger.Warn("El servidor rechazó la creación de la sala.", ex);
                    string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                        ex,
                        Lang.errorTextoErrorProcesarSolicitud);
                    throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    _logger.Error("No se encontró el endpoint para crear sala.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Comunicacion,
                        Lang.errorTextoServidorNoDisponible,
                        ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.Error("Timeout al intentar crear sala.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.TiempoAgotado,
                        Lang.errorTextoServidorTiempoAgotado,
                        ex);
                }
                catch (CommunicationException ex)
                {
                    _logger.Error("Error de comunicación al crear sala.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Comunicacion,
                        Lang.errorTextoServidorNoDisponible,
                        ex);
                }
            }
            finally
            {
                _semaforo.Release();
            }
        }

        /// <summary>
        /// Solicita unirse a una sala existente mediante su codigo.
        /// </summary>
        public async Task<DTOs.SalaDTO> UnirseSalaAsync(string codigoSala, string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                throw new ArgumentException(
                    "El código de sala es obligatorio.",
                    nameof(codigoSala));
            }

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException(
                    "El nombre de usuario es obligatorio.",
                    nameof(nombreUsuario));
            }

            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                var cliente = ObtenerOCrearCliente();

                try
                {
                    var sala = await cliente.UnirseSalaAsync(
                        codigoSala,
                        nombreUsuario).ConfigureAwait(false);

                    _logger.InfoFormat("Usuario '{0}' se unió a la sala '{1}'.",
                        nombreUsuario, codigoSala);
                    return sala;
                }
                catch (FaultException ex)
                {
                    _logger.WarnFormat("Fallo al unirse a sala '{0}'. Razón: {1}",
                        codigoSala, ex.Message);
                    string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                        ex,
                        Lang.errorTextoErrorProcesarSolicitud);
                    throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    _logger.Error("No se encontró el endpoint para unirse a la sala.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Comunicacion,
                        Lang.errorTextoServidorNoDisponible,
                        ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.Error("Timeout al unirse a la sala.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.TiempoAgotado,
                        Lang.errorTextoServidorTiempoAgotado,
                        ex);
                }
                catch (CommunicationException ex)
                {
                    _logger.Error("Error de comunicación al unirse a la sala.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Comunicacion,
                        Lang.errorTextoServidorNoDisponible,
                        ex);
                }
            }
            finally
            {
                _semaforo.Release();
            }
        }

        /// <summary>
        /// Abandona la sala actual y notifica al servidor.
        /// </summary>
        public async Task AbandonarSalaAsync(string codigoSala, string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_cliente == null)
                {
                    return;
                }

                try
                {
                    await _cliente.AbandonarSalaAsync(
                        codigoSala,
                        nombreUsuario).ConfigureAwait(false);

                    _logger.InfoFormat("Usuario '{0}' abandonó la sala '{1}'.", 
                        nombreUsuario, codigoSala);
                }
                catch (FaultException ex)
                {
                    _logger.Warn("Error de lógica al abandonar sala.", ex);
                    string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                        ex,
                        Lang.errorTextoErrorProcesarSolicitud);
                    throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    _logger.Error("Endpoint no encontrado al abandonar sala.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Comunicacion,
                        Lang.errorTextoServidorNoDisponible,
                        ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.Error("Timeout al abandonar sala.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.TiempoAgotado,
                        Lang.errorTextoServidorTiempoAgotado,
                        ex);
                }
                catch (CommunicationException ex)
                {
                    _logger.Error("Error de comunicación al abandonar sala.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Comunicacion,
                        Lang.errorTextoServidorNoDisponible,
                        ex);
                }
            }
            finally
            {
                _semaforo.Release();
            }
        }

        /// <summary>
        /// Expulsa a un jugador especifico de la sala (solo anfitrion).
        /// </summary>
        public async Task ExpulsarJugadorAsync(
            string codigoSala,
            string nombreHost,
            string nombreJugadorAExpulsar)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                throw new ArgumentException(
                    "El código de sala es obligatorio.",
                    nameof(codigoSala));
            }

            if (string.IsNullOrWhiteSpace(nombreHost))
            {
                throw new ArgumentException(
                    "El nombre del host es obligatorio.",
                    nameof(nombreHost));
            }

            if (string.IsNullOrWhiteSpace(nombreJugadorAExpulsar))
            {
                throw new ArgumentException(
                    "El nombre del jugador a expulsar es obligatorio.",
                    nameof(nombreJugadorAExpulsar));
            }

            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                var cliente = ObtenerOCrearCliente();

                try
                {
                    await cliente.ExpulsarJugadorAsync(
                        codigoSala,
                        nombreHost,
                        nombreJugadorAExpulsar).ConfigureAwait(false);

                    _logger.InfoFormat("El host '{0}' expulsó a '{1}' de la sala '{2}'.",
                        nombreHost, nombreJugadorAExpulsar, codigoSala);
                }
                catch (FaultException ex)
                {
                    _logger.Warn("Error al intentar expulsar jugador (Permisos o jugador no " +
                        "existe).", ex);
                    string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                        ex,
                        Lang.errorTextoErrorProcesarSolicitud);
                    throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    _logger.Error("Endpoint no encontrado al expulsar jugador.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Comunicacion,
                        Lang.errorTextoServidorNoDisponible,
                        ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.Error("Timeout al expulsar jugador.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.TiempoAgotado,
                        Lang.errorTextoServidorTiempoAgotado,
                        ex);
                }
                catch (CommunicationException ex)
                {
                    _logger.Error("Error de comunicación al expulsar jugador.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Comunicacion,
                        Lang.errorTextoServidorNoDisponible,
                        ex);
                }
            }
            finally
            {
                _semaforo.Release();
            }
        }

        /// <summary>
        /// Callback: Notifica localmente que alguien entro a la sala.
        /// </summary>
        public void NotificarJugadorSeUnio(string codigoSala, string nombreJugador)
        {
            _logger.InfoFormat("Callback recibido: '{0}' se unió a la sala '{1}'.", 
                nombreJugador, codigoSala);
            JugadorSeUnio?.Invoke(this, nombreJugador);
        }

        /// <summary>
        /// Callback: Notifica localmente que alguien salio de la sala.
        /// </summary>
        public void NotificarJugadorSalio(string codigoSala, string nombreJugador)
        {
            _logger.InfoFormat("Callback recibido: '{0}' salió de la sala '{1}'.",
                nombreJugador, codigoSala);
            JugadorSalio?.Invoke(this, nombreJugador);
        }

        /// <summary>
        /// Callback: Notifica localmente que alguien fue expulsado.
        /// </summary>
        public void NotificarJugadorExpulsado(string codigoSala, string nombreJugador)
        {
            _logger.InfoFormat("Callback recibido: '{0}' fue expulsado de la sala '{1}'.",
                nombreJugador, codigoSala);
            JugadorExpulsado?.Invoke(this, nombreJugador);
        }

        /// <summary>
        /// Callback: Notifica localmente que la sala fue cancelada por el anfitrion.
        /// </summary>
        public void NotificarSalaCancelada(string codigoSala)
        {
            _logger.InfoFormat("Callback recibido: la sala '{0}' fue cancelada por el anfitrión.",
                codigoSala);
            SalaCancelada?.Invoke(this, codigoSala);
        }

        /// <summary>
        /// Callback: Actualiza la lista publica de salas.
        /// </summary>
        public void NotificarListaSalasActualizada(DTOs.SalaDTO[] salas)
        {
            var lista = Convertir(salas);

            lock (_salasBloqueo)
            {
                _salas.Clear();
                _salas.AddRange(lista);
            }

            ListaSalasActualizada?.Invoke(this, lista);
        }

        /// <summary>
        /// Callback: Actualiza la informacion de la sala actual.
        /// </summary>
        public void NotificarSalaActualizada(DTOs.SalaDTO sala)
        {
            SalaActualizada?.Invoke(this, sala);
        }

        /// <summary>
        /// Se suscribe para recibir actualizaciones sobre las salas publicas.
        /// </summary>
        public async Task SuscribirListaSalasAsync()
        {
            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_suscrito && _cliente != null)
                {
                    return;
                }

                var cliente = ObtenerOCrearCliente();

                try
                {
                    await cliente.SuscribirListaSalasAsync().ConfigureAwait(false);
                    _suscrito = true;
                    _logger.Info("Suscripción al lobby de salas exitosa.");
                }
                catch (FaultException ex)
                {
                    _logger.Warn("Error del servidor al suscribir a lista de salas.", ex);
                    string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                        ex,
                        Lang.errorTextoErrorProcesarSolicitud);
                    throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
                }
                catch (EndpointNotFoundException ex)
                {
                    _logger.Error("Endpoint no encontrado al suscribir a lista de salas.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Comunicacion,
                        Lang.errorTextoServidorNoDisponible,
                        ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.Error("Timeout al suscribir a lista de salas.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.TiempoAgotado,
                        Lang.errorTextoServidorTiempoAgotado,
                        ex);
                }
                catch (CommunicationException ex)
                {
                    _logger.Error("Error de comunicación al suscribir a lista de salas.", ex);
                    CerrarCliente();
                    throw new ServicioExcepcion(
                        TipoErrorServicio.Comunicacion,
                        Lang.errorTextoServidorNoDisponible,
                        ex);
                }
            }
            finally
            {
                _semaforo.Release();
            }
        }

        /// <summary>
        /// Cancela la suscripcion de actualizaciones de salas publicas.
        /// </summary>
        public async Task CancelarSuscripcionListaSalasAsync()
        {
            await _semaforo.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!_suscrito || _cliente == null)
                {
                    return;
                }

                try
                {
                    await _cliente.CancelarSuscripcionListaSalasAsync().ConfigureAwait(false);
                    _suscrito = false;
                    _logger.Info("Suscripción al lobby de salas cancelada.");
                }
                catch (CommunicationException ex)
                {
                    _logger.Warn("Advertencia: Error de comunicación al cancelar suscripción " +
                        "de salas (posible cierre forzado).", ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.Warn("Advertencia: Timeout al cancelar suscripción de salas.", ex);
                }
            }
            finally
            {
                _semaforo.Release();
            }
        }

        /// <summary>
        /// Libera los recursos y cierra la conexion.
        /// </summary>
        public void Dispose()
        {
            bool lockTomado = _semaforo.Wait(3000);

            try
            {
                if (lockTomado)
                {
                    if (_suscrito && _cliente != null)
                    {
                        try
                        {
                            Task.Run(async () =>
                                await _cliente.CancelarSuscripcionListaSalasAsync()).Wait(2000);
                        }
                        catch (CommunicationException ex)
                        {
                            _logger.Warn("Error de red al cerrar servicio de salas.", ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _logger.Warn("Timeout al cerrar servicio de salas.", ex);
                        }
                        catch (AggregateException ex)
                        {
                            _logger.Warn("Error agregado al cerrar servicio de salas.", ex);
                        }
                    }
                    CerrarCliente();
                }
                else
                {
                    _cliente?.Abort();
                }
            }
            finally
            {
                if (lockTomado)
                {
                    _semaforo.Release();
                }
                _semaforo.Dispose();
            }
        }

        private PictionaryServidorServicioSalas.SalasManejadorClient ObtenerOCrearCliente()
        {
            if (_cliente == null || _cliente.State == CommunicationState.Faulted)
            {
                CerrarCliente();
                var contexto = new InstanceContext(this);
                _cliente = new PictionaryServidorServicioSalas
                    .SalasManejadorClient(contexto, Endpoint);
            }

            return _cliente;
        }

        private void CerrarCliente()
        {
            if (_cliente == null)
            {
                return;
            }

            try
            {
                if (_cliente.State == CommunicationState.Faulted)
                {
                    _cliente.Abort();
                }
                else
                {
                    _cliente.Close();
                }
            }
            catch (CommunicationException)
            {
                _cliente.Abort();
            }
            catch (TimeoutException)
            {
                _cliente.Abort();
            }
            finally
            {
                _cliente = null;
                _suscrito = false;
            }
        }

        private static IReadOnlyList<DTOs.SalaDTO> Convertir(IEnumerable<DTOs.SalaDTO> salas)
        {
            if (salas == null)
            {
                return Array.Empty<DTOs.SalaDTO>();
            }

            var lista = salas
                .Where(sala => sala != null && !string.IsNullOrWhiteSpace(sala.Codigo))
                .Select(sala => new DTOs.SalaDTO
                {
                    Codigo = sala.Codigo,
                    Creador = sala.Creador,
                    Configuracion = sala.Configuracion,
                    Jugadores = sala.Jugadores != null
                        ? new List<string>(sala.Jugadores)
                        : new List<string>()
                })
                .ToList();

            return lista.Count == 0 ? Array.Empty<DTOs.SalaDTO>() : lista.AsReadOnly();
        }
    }
}