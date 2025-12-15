using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
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
        private readonly IWcfClienteFabrica _fabricaClientes;
        private readonly IManejadorErrorServicio _manejadorError;

        private readonly SemaphoreSlim _semaforo = new(1, 1);
        private readonly object _salasBloqueo = new();
        private readonly List<DTOs.SalaDTO> _salas = new();

        private PictionaryServidorServicioSalas.SalasManejadorClient _cliente;
        private bool _suscrito;

        /// <summary>
        /// Inicializa el servicio de salas.
        /// </summary>
        public SalasServicio(
            IWcfClienteFabrica fabricaClientes,
            IManejadorErrorServicio manejadorError)
        {
            _fabricaClientes = fabricaClientes ??
                throw new ArgumentNullException(nameof(fabricaClientes));
            _manejadorError = manejadorError ??
                throw new ArgumentNullException(nameof(manejadorError));
        }

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
            ValidarCreacionSala(nombreCreador, configuracion);

            await _semaforo.WaitAsync().ConfigureAwait(false);
            try
            {
                var cliente = ObtenerOCrearCliente();
                var sala = await cliente.CrearSalaAsync(nombreCreador, configuracion)
                    .ConfigureAwait(false);

                _logger.InfoFormat("Sala creada por '{0}'. Codigo: {1}", nombreCreador, 
                    sala.Codigo);
                return sala;
            }
            catch (Exception excepcion)
            {
                ManejarExcepcionServicio(excepcion, Lang.errorTextoErrorProcesarSolicitud);
                throw;
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
            ValidarDatosSalaUsuario(codigoSala, nombreUsuario);

            await _semaforo.WaitAsync().ConfigureAwait(false);
            try
            {
                var cliente = ObtenerOCrearCliente();
                var sala = await cliente.UnirseSalaAsync(codigoSala, nombreUsuario)
                    .ConfigureAwait(false);
                return sala;
            }
            catch (Exception excepcion)
            {
                ManejarExcepcionServicio(excepcion, Lang.errorTextoErrorProcesarSolicitud);
                throw;
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
            if (string.IsNullOrWhiteSpace(codigoSala) || string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            await _semaforo.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_cliente == null) return;

                await _cliente.AbandonarSalaAsync(codigoSala, nombreUsuario).ConfigureAwait(false);
            }
            catch (Exception excepcion)
            {
                ManejarExcepcionServicio(excepcion, Lang.errorTextoErrorProcesarSolicitud);
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
            ValidarExpulsion(codigoSala, nombreHost, nombreJugadorAExpulsar);

            await _semaforo.WaitAsync().ConfigureAwait(false);
            try
            {
                var cliente = ObtenerOCrearCliente();
                await cliente.ExpulsarJugadorAsync(
                    codigoSala,
                    nombreHost,
                    nombreJugadorAExpulsar).ConfigureAwait(false);
            }
            catch (Exception excepcion)
            {
                ManejarExcepcionServicio(excepcion, Lang.errorTextoErrorProcesarSolicitud);
                throw;
            }
            finally
            {
                _semaforo.Release();
            }
        }

        /// <summary>
        /// Se suscribe para recibir actualizaciones sobre las salas publicas.
        /// </summary>
        public async Task SuscribirListaSalasAsync()
        {
            await _semaforo.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_suscrito && _cliente != null) return;

                var cliente = ObtenerOCrearCliente();
                await cliente.SuscribirListaSalasAsync().ConfigureAwait(false);

                _suscrito = true;
            }
            catch (Exception excepcion)
            {
                ManejarExcepcionServicio(excepcion, Lang.errorTextoErrorProcesarSolicitud);
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
                if (!_suscrito || _cliente == null) return;

                await _cliente.CancelarSuscripcionListaSalasAsync().ConfigureAwait(false);
                _suscrito = false;
            }
            catch (Exception excepcion)
            {
                _logger.Warn("Error al cancelar suscripcion de salas.", excepcion);
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
            _logger.InfoFormat("Callback recibido: '{0}' se unio a la sala '{1}'.", 
                nombreJugador, codigoSala);
            JugadorSeUnio?.Invoke(this, nombreJugador);
        }

        /// <summary>
        /// Callback: Notifica localmente que alguien salio de la sala.
        /// </summary>
        public void NotificarJugadorSalio(string codigoSala, string nombreJugador)
        {
            _logger.InfoFormat("Callback recibido: '{0}' salio de la sala '{1}'.",
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
            _logger.InfoFormat("Callback recibido: la sala '{0}' fue cancelada por el anfitrion.",
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
        /// Libera los recursos y cierra la conexion.
        /// </summary>
        public void Dispose()
        {
            bool lockTomado = _semaforo.Wait(3000);

            try
            {
                if (lockTomado)
                {
                    IntentarCancelarSuscripcionAlCerrar();
                    CerrarCliente();
                }
                else
                {
                    AbortarCliente();
                }
            }
            finally
            {
                if (lockTomado) _semaforo.Release();
                _semaforo.Dispose();
            }
        }

        private void IntentarCancelarSuscripcionAlCerrar()
        {
            if (_suscrito && _cliente != null)
            {
                try
                {
                    Task.Run(async () =>
                        await _cliente.CancelarSuscripcionListaSalasAsync()).Wait(2000);
                }
                catch (Exception excepcion)
                {
                    _logger.Warn("Error al cerrar suscripcion de salas en Dispose.", excepcion);
                }
            }
        }

        private PictionaryServidorServicioSalas.ISalasManejador ObtenerOCrearCliente()
        {
            var canal = _cliente as ICommunicationObject;
            if (_cliente == null || canal.State == CommunicationState.Faulted)
            {
                CerrarCliente();
                var contexto = new InstanceContext(this);
                _cliente = (PictionaryServidorServicioSalas.SalasManejadorClient)
                    _fabricaClientes.CrearClienteSalas(contexto);
            }
            return _cliente;
        }

        private void CerrarCliente()
        {
            if (_cliente is ICommunicationObject canal)
            {
                try
                {
                    if (canal.State == CommunicationState.Faulted) canal.Abort();
                    else canal.Close();
                }
                catch (Exception)
                {
                    canal.Abort();
                }
            }
            _cliente = null;
            _suscrito = false;
        }

        private void AbortarCliente()
        {
            if (_cliente is ICommunicationObject canal)
            {
                canal.Abort();
            }
        }

        private void ManejarExcepcionServicio(Exception excepcion, string mensajePredeterminado)
        {
            if (excepcion is CommunicationException || excepcion is TimeoutException)
            {
                CerrarCliente();
            }

            if (excepcion is FaultException faultExcepcion)
            {
                string mensaje = _manejadorError.ObtenerMensaje(faultExcepcion,
                    mensajePredeterminado);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje,
                    faultExcepcion);
            }

            if (excepcion is CommunicationException || excepcion is EndpointNotFoundException)
            {
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    excepcion);
            }

            if (excepcion is TimeoutException)
            {
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    excepcion);
            }

            throw new ServicioExcepcion(
                TipoErrorServicio.OperacionInvalida,
                mensajePredeterminado,
                excepcion);
        }

        private static void ValidarCreacionSala(
            string creador,
            DTOs.ConfiguracionPartidaDTO config)
        {
            if (string.IsNullOrWhiteSpace(creador))
                throw new ArgumentException("Creador obligatorio.", nameof(creador));
            if (config == null)
                throw new ArgumentNullException(nameof(config));
        }

        private static void ValidarDatosSalaUsuario(string codigo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("Codigo obligatorio.", nameof(codigo));
            if (string.IsNullOrWhiteSpace(usuario))
                throw new ArgumentException("Usuario obligatorio.", nameof(usuario));
        }

        private static void ValidarExpulsion(string codigo, string host, string jugador)
        {
            ValidarDatosSalaUsuario(codigo, host);
            if (string.IsNullOrWhiteSpace(jugador))
                throw new ArgumentException("Jugador a expulsar obligatorio.", nameof(jugador));
        }

        private static IReadOnlyList<DTOs.SalaDTO> Convertir(IEnumerable<DTOs.SalaDTO> salas)
        {
            if (salas == null) return Array.Empty<DTOs.SalaDTO>();

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

            return lista.AsReadOnly();
        }
    }
}