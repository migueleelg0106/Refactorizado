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
using PictionaryMusicalServidor.Servicios.Servicios.Modelos;
using PictionaryMusicalServidor.Servicios.Servicios.Notificadores;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion de salas de juego.
    /// Maneja creacion, union, abandono y expulsion de salas con notificaciones en tiempo real via callbacks.
    /// Utiliza un diccionario concurrente para almacenar salas activas en memoria.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SalasManejador : ISalasManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SalasManejador));
        private static readonly ConcurrentDictionary<string, SalaInterna> _salas = new(StringComparer.OrdinalIgnoreCase);
        private static readonly NotificadorSalas _notificador = new(() => _salas.Values);

        /// <summary>
        /// Crea una nueva sala de juego con la configuracion especificada.
        /// Genera un codigo unico, registra el callback del creador y notifica a todos los suscriptores.
        /// </summary>
        /// <param name="nombreCreador">Nombre del usuario que crea la sala.</param>
        /// <param name="configuracion">Configuracion de la partida para la sala.</param>
        /// <returns>Datos de la sala creada.</returns>
        /// <exception cref="FaultException">Se lanza si los datos son invalidos o hay errores al crear la sala.</exception>
        public SalaDTO CrearSala(string nombreCreador, ConfiguracionPartidaDTO configuracion)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreCreador, nameof(nombreCreador));
                ValidarConfiguracion(configuracion);

                string codigo = GenerarCodigoSala();
                var callback = OperationContext.Current.GetCallbackChannel<ISalasCallback>();

                var sala = new SalaInterna(codigo, nombreCreador.Trim(), configuracion);
                sala.AgregarJugador(nombreCreador.Trim(), callback, notificar: false);

                if (!_salas.TryAdd(codigo, sala))
                {
                    _logger.Warn($"Error de concurrencia al intentar agregar la sala {codigo}.");
                    throw new FaultException(MensajesError.Cliente.ErrorCrearSala);
                }

                _notificador.NotificarListaSalasATodos();

                _logger.Info($"Sala '{codigo}' creada exitosamente por '{nombreCreador.Trim()}'. Configuración: {configuracion.NumeroRondas} rondas, {configuracion.TiempoPorRondaSegundos}s.");
                return sala.ToDto();
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.SalaCrearOperacionInvalida, ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.SalaCrearOperacionInvalida, ex);
                throw new FaultException(ex.Message);
            }
            catch (CommunicationException ex)
            {
                _logger.Error(MensajesError.Log.SalaCrearComunicacion, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoCrearSala);
            }
            catch (TimeoutException ex)
            {
                _logger.Error(MensajesError.Log.SalaCrearTimeout, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoCrearSala);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaCrearErrorGeneral, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoCrearSala);
            }
        }

        /// <summary>
        /// Une un usuario a una sala de juego existente.
        /// Valida que la sala exista, agrega el jugador, registra su callback y notifica a todos los participantes.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <param name="nombreUsuario">Nombre del usuario que se une a la sala.</param>
        /// <returns>Datos actualizados de la sala a la que se unio.</returns>
        /// <exception cref="FaultException">Se lanza si los datos son invalidos, la sala no existe, o hay errores al unirse.</exception>
        public SalaDTO UnirseSala(string codigoSala, string nombreUsuario)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreUsuario, nameof(nombreUsuario));

                if (string.IsNullOrWhiteSpace(codigoSala))
                {
                    throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);
                }

                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                {
                    _logger.Warn($"Intento de unirse a sala inexistente: '{codigoSala}'. Usuario: '{nombreUsuario}'.");
                    throw new FaultException(MensajesError.Cliente.SalaNoEncontrada);
                }

                var callback = OperationContext.Current.GetCallbackChannel<ISalasCallback>();
                var resultado = sala.AgregarJugador(nombreUsuario.Trim(), callback, notificar: true);

                _notificador.NotificarListaSalasATodos();

                _logger.Info($"Jugador '{nombreUsuario.Trim()}' se unió correctamente a la sala '{codigoSala.Trim()}'.");
                return resultado;
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.SalaUnirseOperacionInvalida, ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.SalaUnirseOperacionInvalida, ex);
                throw new FaultException(ex.Message);
            }
            catch (CommunicationException ex)
            {
                _logger.Error(MensajesError.Log.SalaUnirseComunicacion, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoUnirse);
            }
            catch (TimeoutException ex)
            {
                _logger.Error(MensajesError.Log.SalaUnirseTimeout, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoUnirse);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaUnirseErrorGeneral, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoUnirse);
            }
        }

        /// <summary>
        /// Obtiene la lista de todas las salas de juego disponibles.
        /// Retorna todas las salas activas en el sistema.
        /// </summary>
        /// <returns>Lista de salas disponibles.</returns>
        public IList<SalaDTO> ObtenerSalas()
        {
            try
            {
                return _salas.Values.Select(s => s.ToDto()).ToList();
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.SalaObtenerListaOperacionInvalida, ex);
                return new List<SalaDTO>();
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaObtenerListaErrorGeneral, ex);
                return new List<SalaDTO>();
            }
        }

        /// <summary>
        /// Permite a un usuario abandonar una sala de juego.
        /// Remueve el jugador de la sala, elimina la sala si queda vacia y notifica a todos los suscriptores.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <param name="nombreUsuario">Nombre del usuario que abandona la sala.</param>
        /// <exception cref="FaultException">Se lanza si los datos son invalidos, la sala no existe, o hay errores al abandonar.</exception>
        public void AbandonarSala(string codigoSala, string nombreUsuario)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreUsuario, nameof(nombreUsuario));

                if (string.IsNullOrWhiteSpace(codigoSala))
                {
                    throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);
                }

                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                {
                    _logger.Warn($"Intento de abandonar sala inexistente: '{codigoSala}'.");
                    throw new FaultException(MensajesError.Cliente.SalaNoEncontrada);
                }

                sala.RemoverJugador(nombreUsuario.Trim());

                if (sala.DebeEliminarse)
                {
                    if (_salas.TryRemove(codigoSala.Trim(), out _))
                    {
                        _logger.Info($"Sala '{codigoSala.Trim()}' eliminada automáticamente (vacía o host salió).");
                    }
                }

                _notificador.NotificarListaSalasATodos();
                _logger.Info($"Jugador '{nombreUsuario.Trim()}' abandonó la sala '{codigoSala.Trim()}'.");
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.SalaAbandonarOperacionInvalida, ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.SalaAbandonarOperacionInvalida, ex);
                throw new FaultException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaAbandonarErrorGeneral, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoAbandonar);
            }
        }

        /// <summary>
        /// Suscribe al cliente para recibir notificaciones sobre cambios en la lista de salas.
        /// Registra el callback del cliente y configura eventos de cierre de canal para limpieza automatica.
        /// </summary>
        /// <exception cref="FaultException">Se lanza si hay errores de comunicacion o al suscribir.</exception>
        public void SuscribirListaSalas()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<ISalasCallback>();
                var sesionId = _notificador.Suscribir(callback);

                var canal = OperationContext.Current?.Channel;
                if (canal != null)
                {
                    canal.Closed += (_, __) => _notificador.Desuscribir(sesionId);
                    canal.Faulted += (_, __) => _notificador.Desuscribir(sesionId);
                }

                _notificador.NotificarListaSalas(callback);
                _logger.Info($"Nueva suscripción al lobby de salas. Sesión ID: {sesionId}");
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.SalaSuscripcionOperacionInvalida, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoSuscripcion);
            }
            catch (CommunicationException ex)
            {
                _logger.Error(MensajesError.Log.SalaSuscripcionComunicacion, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoSuscripcion);
            }
            catch (TimeoutException ex)
            {
                _logger.Error(MensajesError.Log.SalaSuscripcionTimeout, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoSuscripcion);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaSuscripcionErrorGeneral, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoSuscripcion);
            }
        }

        /// <summary>
        /// Cancela la suscripcion del cliente de notificaciones de la lista de salas.
        /// Elimina el callback del cliente del notificador.
        /// </summary>
        public void CancelarSuscripcionListaSalas()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<ISalasCallback>();
                _notificador.DesuscribirPorCallback(callback);
                _logger.Info("Cliente canceló suscripción al lobby de salas.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.SalaCancelarSuscripcionOperacionInvalida, ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error(MensajesError.Log.SalaCancelarSuscripcionComunicacion, ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error(MensajesError.Log.SalaCancelarSuscripcionTimeout, ex);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaCancelarSuscripcionErrorGeneral, ex);
            }
        }

        /// <summary>
        /// Expulsa un jugador de una sala de juego.
        /// Valida que el usuario sea el anfitrion, remueve al jugador expulsado y notifica a todos los participantes.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <param name="nombreHost">Nombre del usuario anfitrion que expulsa.</param>
        /// <param name="nombreJugadorAExpulsar">Nombre del jugador a expulsar.</param>
        /// <exception cref="FaultException">Se lanza si los datos son invalidos, la sala no existe, el usuario no es anfitrion, o hay errores al expulsar.</exception>
        public void ExpulsarJugador(string codigoSala, string nombreHost, string nombreJugadorAExpulsar)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreHost, nameof(nombreHost));
                ValidadorNombreUsuario.Validar(nombreJugadorAExpulsar, nameof(nombreJugadorAExpulsar));

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

                _logger.Info($"Jugador '{nombreJugadorAExpulsar.Trim()}' expulsado de sala '{codigoSala.Trim()}' por '{nombreHost.Trim()}'.");
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn(MensajesError.Log.SalaExpulsarOperacionInvalida, ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.SalaExpulsarOperacionInvalida, ex);
                throw new FaultException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaExpulsarErrorGeneral, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoExpulsar);
            }
        }

        /// <summary>
        /// Obtiene una sala por su codigo identificador.
        /// Busca la sala en el diccionario de salas activas y retorna su representacion DTO.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <returns>Datos de la sala como DTO, o null si el codigo es invalido o la sala no existe.</returns>
        internal static SalaDTO ObtenerSalaPorCodigo(string codigoSala)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                return null;
            }

            if (_salas.TryGetValue(codigoSala.Trim(), out var sala))
            {
                return sala.ToDto();
            }

            return null;
        }


        private static string GenerarCodigoSala()
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

            _logger.Error("No se pudo generar un código de sala único después de múltiples intentos.");
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