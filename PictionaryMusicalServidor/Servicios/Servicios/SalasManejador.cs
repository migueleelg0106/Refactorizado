using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using PictionaryMusicalServidor.Servicios.Servicios.Notificadores;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion de salas de juego.
    /// Maneja creacion, union, abandono y expulsion de salas con notificaciones en tiempo real
    /// via callbacks.
    /// Utiliza un diccionario concurrente para almacenar salas activas en memoria.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SalasManejador : ISalasManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SalasManejador));

        private static readonly ConcurrentDictionary<string, SalaInternaManejador> _salas =
            new ConcurrentDictionary<string, SalaInternaManejador>(
                StringComparer.OrdinalIgnoreCase);

        private readonly INotificadorSalas _notificador;
        private readonly IValidadorNombreUsuario _validadorUsuario;

        /// <summary>
        /// Constructor por defecto que inicializa las dependencias.
        /// </summary>
        public SalasManejador()
        {
            _notificador = new NotificadorSalas(() => _salas.Values);
            _validadorUsuario = new ValidadorNombreUsuario();
        }

        /// <summary>
        /// Constructor con inyeccion de dependencias.
        /// </summary>
        public SalasManejador(INotificadorSalas notificador, 
            IValidadorNombreUsuario validadorUsuario)
        {
            _notificador = notificador ??
                throw new ArgumentNullException(nameof(notificador));
            _validadorUsuario = validadorUsuario ??
                throw new ArgumentNullException(nameof(validadorUsuario));
        }

        /// <summary>
        /// Crea una nueva sala de juego con la configuracion especificada.
        /// Genera un codigo unico, registra el callback del creador y notifica a todos.
        /// </summary>
        /// <param name="nombreCreador">Nombre del usuario que crea la sala.</param>
        /// <param name="configuracion">Configuracion de la partida para la sala.</param>
        /// <returns>Datos de la sala creada.</returns>
        public SalaDTO CrearSala(string nombreCreador, ConfiguracionPartidaDTO configuracion)
        {
            try
            {
                _validadorUsuario.Validar(nombreCreador, nameof(nombreCreador));
                ValidarConfiguracion(configuracion);

                string codigo = GenerarCodigoSala();
                var callback = OperationContext.Current.GetCallbackChannel
                    <ISalasManejadorCallback>();

                var gestorNotificacionesSala = new GestorNotificacionesSalaInterna();

                var sala = new SalaInternaManejador(
                    codigo,
                    nombreCreador.Trim(),
                    configuracion,
                    gestorNotificacionesSala);

                sala.AgregarJugador(nombreCreador.Trim(), callback, notificar: false);

                if (!_salas.TryAdd(codigo, sala))
                {
                    _logger.Warn("Error de concurrencia al intentar agregar la sala.");
                    throw new FaultException(MensajesError.Cliente.ErrorCrearSala);
                }

                _logger.InfoFormat(
                    "Sala creada exitosamente con codigo {0}.",
                    codigo);

                _notificador.NotificarListaSalasATodos();

                return sala.ToDto();
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Operacion invalida al crear sala.", ex);
                throw new FaultException(ex.Message);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicacion WCF al crear sala.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoCrearSala);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al crear sala.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoCrearSala);
            }
            catch (ObjectDisposedException ex)
            {
                _logger.Error("Error inesperado al crear sala.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoCrearSala);
            }
        }

        /// <summary>
        /// Une un usuario a una sala de juego existente.
        /// Valida sala, agrega jugador, registra callback y notifica.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <param name="nombreUsuario">Nombre del usuario que se une a la sala.</param>
        /// <returns>Datos actualizados de la sala a la que se unio.</returns>
        public SalaDTO UnirseSala(string codigoSala, string nombreUsuario)
        {
            try
            {
                _validadorUsuario.Validar(nombreUsuario, nameof(nombreUsuario));

                if (string.IsNullOrWhiteSpace(codigoSala))
                {
                    throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);
                }

                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                {
                    throw new FaultException(MensajesError.Cliente.SalaNoEncontrada);
                }

                if (sala.PartidaIniciada)
                {
                    throw new FaultException(MensajesError.Cliente.PartidaComenzo);
                }

                var callback = OperationContext.Current.GetCallbackChannel
                    <ISalasManejadorCallback>();

                var resultado = sala.AgregarJugador(
                    nombreUsuario.Trim(),
                    callback,
                    notificar: true);

                _logger.InfoFormat(
                    "Usuario unido exitosamente a sala con codigo {0}.",
                    codigoSala.Trim());

                _notificador.NotificarListaSalasATodos();

                return resultado;
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Operacion invalida al unirse a sala.", ex);
                throw new FaultException(ex.Message);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicacion WCF al unirse a sala.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoUnirse);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al unirse a la sala.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoUnirse);
            }
            catch (ObjectDisposedException ex)
            {
                _logger.Error("Error inesperado al unirse a la sala.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoUnirse);
            }
        }

        /// <summary>
        /// Obtiene la lista de todas las salas de juego disponibles.
        /// </summary>
        /// <returns> Lista de salas disponibles.</returns>
        public IList<SalaDTO> ObtenerSalas()
        {
            try
            {
                return _salas.Values.Select(s => s.ToDto()).ToList();
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operacion invalida al obtener lista de salas.", ex);
                return new List<SalaDTO>();
            }
        }

        /// <summary>
        /// Permite a un usuario abandonar una sala de juego.
        /// Remueve jugador, elimina sala si vacia y notifica.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <param name="nombreUsuario">Nombre del usuario que abandona la sala.</param>
        public void AbandonarSala(string codigoSala, string nombreUsuario)
        {
            try
            {
                _validadorUsuario.Validar(nombreUsuario, nameof(nombreUsuario));

                if (string.IsNullOrWhiteSpace(codigoSala))
                {
                    throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);
                }

                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                {
                    throw new FaultException(MensajesError.Cliente.SalaNoEncontrada);
                }

                sala.RemoverJugador(nombreUsuario.Trim());
                _notificador.NotificarListaSalasATodos();
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Operacion invalida al abandonar sala.", ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Operacion invalida al abandonar sala.", ex);
                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Suscribe al cliente para recibir notificaciones sobre cambios en la lista de salas.
        /// </summary>
        public void SuscribirListaSalas()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel
                    <ISalasManejadorCallback>();
                var sesionId = _notificador.Suscribir(callback);

                var canal = OperationContext.Current?.Channel;
                if (canal != null)
                {
                    canal.Closed += (_, __) => _notificador.Desuscribir(sesionId);
                    canal.Faulted += (_, __) => _notificador.Desuscribir(sesionId);
                }

                _notificador.NotificarListaSalas(callback);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operacion invalida al suscribirse a lista de salas.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoSuscripcion);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicacion WCF al suscribirse a lista de salas.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoSuscripcion);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al suscribirse a la lista de salas.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoSuscripcion);
            }
        }

        /// <summary>
        /// Cancela la suscripcion del cliente de notificaciones de la lista de salas.
        /// </summary>
        public void CancelarSuscripcionListaSalas()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel
                    <ISalasManejadorCallback>();
                _notificador.DesuscribirPorCallback(callback);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operacion invalida al cancelar suscripcion.", ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicacion WCF al cancelar suscripcion.", ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al cancelar la suscripcion.", ex);
            }
        }

        /// <summary>
        /// Expulsa un jugador de una sala de juego.
        /// Valida permisos de anfitrion, remueve jugador y notifica.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <param name="nombreHost">Nombre del usuario anfitrion que expulsa.</param>
        /// <param name="nombreJugadorAExpulsar">Nombre del jugador a expulsar.</param>
        public void ExpulsarJugador(
            string codigoSala,
            string nombreHost,
            string nombreJugadorAExpulsar)
        {
            try
            {
                _validadorUsuario.Validar(nombreHost, nameof(nombreHost));
                _validadorUsuario.Validar(
                    nombreJugadorAExpulsar,
                    nameof(nombreJugadorAExpulsar));

                if (string.IsNullOrWhiteSpace(codigoSala))
                {
                    throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);
                }

                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                {
                    throw new FaultException(MensajesError.Cliente.SalaNoEncontrada);
                }

                sala.ExpulsarJugador(nombreHost.Trim(), nombreJugadorAExpulsar.Trim());

                if (sala.DebeEliminarse)
                {
                    _salas.TryRemove(codigoSala.Trim(), out _);
                }

                _notificador.NotificarListaSalasATodos();
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Operacion invalida al expulsar jugador.", ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Operacion invalida al expulsar jugador.", ex);
                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene una sala por su codigo identificador.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <returns>Datos de la sala como DTO.</returns>
        public SalaDTO ObtenerSalaPorCodigo(string codigoSala)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                throw new InvalidOperationException(MensajesError.Cliente.CodigoSalaObligatorio);
            }

            if (_salas.TryGetValue(codigoSala.Trim(), out var sala))
            {
                return sala.ToDto();
            }

            throw new InvalidOperationException(MensajesError.Cliente.SalaNoEncontrada);
        }

        /// <summary>
        /// Marca una sala como iniciada para prevenir que nuevos jugadores se unan.
        /// </summary>
        public void MarcarPartidaComoIniciada(string codigoSala)
        {
            if (_salas.TryGetValue(codigoSala, out var sala))
            {
                sala.PartidaIniciada = true;
            }
        }

        public void MarcarPartidaComoFinalizada(string codigoSala)
        {
            if (_salas.TryGetValue(codigoSala, out var sala))
            {
                sala.PartidaFinalizada = true;
            }
        }


        private string GenerarCodigoSala()
        {
            var random = new Random();
            const int maxIntentos = 1000;

            for (int i = 0; i < maxIntentos; i++)
            {
                string codigo = random.Next(0, 1_000_000).ToString("D6");
                if (!_salas.ContainsKey(codigo))
                {
                    return codigo;
                }
            }

            _logger.Error("No se pudo generar codigo unico de sala.");
            throw new FaultException(MensajesError.Cliente.ErrorGenerarCodigo);
        }

        private static void ValidarConfiguracion(ConfiguracionPartidaDTO configuracion)
        {
            if (configuracion == null)
            {
                throw new FaultException(MensajesError.Cliente.ConfiguracionObligatoria);
            }

            if (configuracion.NumeroRondas <= 0)
            {
                throw new FaultException(MensajesError.Cliente.NumeroRondasInvalido);
            }

            if (configuracion.TiempoPorRondaSegundos <= 0)
            {
                throw new FaultException(MensajesError.Cliente.TiempoRondaInvalido);
            }

            if (string.IsNullOrWhiteSpace(configuracion.IdiomaCanciones))
            {
                throw new FaultException(MensajesError.Cliente.IdiomaObligatorio);
            }

            if (string.IsNullOrWhiteSpace(configuracion.Dificultad))
            {
                throw new FaultException(MensajesError.Cliente.DificultadObligatoria);
            }
        }
    }
}