using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Modelos;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion del curso de partida.
    /// Maneja el registro de jugadores, envio de mensajes y validacion de adivinanzas.
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class CursoPartidaManejador : ICursoPartidaManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CursoPartidaManejador));
        private static readonly ConcurrentDictionary<string, ControladorPartida> _controladores =
            new ConcurrentDictionary<string, ControladorPartida>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, string> _jugadorASala =
            new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registra un jugador en el curso de una partida para recibir notificaciones.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala de juego.</param>
        /// <param name="nombreJugador">Nombre del jugador a registrar.</param>
        public void RegistrarEnPartida(string codigoSala, string nombreJugador)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreJugador, nameof(nombreJugador));
                ValidarCodigoSala(codigoSala);

                var callback = OperationContext.Current.GetCallbackChannel<ICursoPartidaCallback>();
                var controlador = ObtenerOCrearControlador(codigoSala);

                controlador.RegistrarJugador(nombreJugador, callback);
                _jugadorASala[nombreJugador] = codigoSala;

                ConfigurarEventosCierreCanal(nombreJugador, codigoSala);

                _logger.InfoFormat(
                    "Jugador '{0}' registrado en partida de sala '{1}'.",
                    nombreJugador, codigoSala);
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos invalidos al registrar en partida.", ex);
                throw new FaultException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Operacion invalida al registrar en partida.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorContextoOperacion);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicacion al registrar en partida.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorObtenerCallback);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al registrar en partida.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
        }

        /// <summary>
        /// Envia un mensaje de juego que puede ser un intento de adivinanza o chat normal.
        /// </summary>
        /// <param name="mensaje">Contenido del mensaje.</param>
        /// <param name="codigoSala">Codigo de la sala donde se envia el mensaje.</param>
        public void EnviarMensajeJuego(string mensaje, string codigoSala)
        {
            try
            {
                ValidarCodigoSala(codigoSala);

                if (string.IsNullOrWhiteSpace(mensaje))
                {
                    return;
                }

                string nombreJugador = ObtenerNombreJugadorActual();
                if (string.IsNullOrWhiteSpace(nombreJugador))
                {
                    _logger.Warn("Intento de enviar mensaje sin identificacion de jugador.");
                    throw new FaultException(MensajesError.Cliente.NombreUsuarioObligatorio);
                }

                if (!_controladores.TryGetValue(codigoSala, out var controlador))
                {
                    _logger.WarnFormat(
                        "Intento de enviar mensaje a sala sin controlador: '{0}'.",
                        codigoSala);
                    throw new FaultException(MensajesError.Cliente.SalaNoEncontrada);
                }

                controlador.ProcesarMensaje(nombreJugador, mensaje.Trim());

                _logger.InfoFormat(
                    "Mensaje procesado de '{0}' en sala '{1}'.",
                    nombreJugador, codigoSala);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos invalidos al enviar mensaje de juego.", ex);
                throw new FaultException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al enviar mensaje de juego.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
        }

        /// <summary>
        /// Desregistra un jugador del curso de la partida.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala de juego.</param>
        /// <param name="nombreJugador">Nombre del jugador a desregistrar.</param>
        public void DesregistrarDePartida(string codigoSala, string nombreJugador)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigoSala) ||
                    string.IsNullOrWhiteSpace(nombreJugador))
                {
                    return;
                }

                DesregistrarJugadorInterno(nombreJugador, codigoSala);

                _logger.InfoFormat(
                    "Jugador '{0}' desregistrado de partida de sala '{1}'.",
                    nombreJugador, codigoSala);
            }
            catch (Exception ex)
            {
                _logger.Warn("Error al desregistrar jugador de partida.", ex);
            }
        }

        /// <summary>
        /// Obtiene el controlador de partida para una sala especifica.
        /// Metodo interno para uso desde otros servicios.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala.</param>
        /// <returns>Controlador de partida o null si no existe.</returns>
        internal static ControladorPartida ObtenerControlador(string codigoSala)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                return null;
            }

            _controladores.TryGetValue(codigoSala, out var controlador);
            return controlador;
        }

        /// <summary>
        /// Crea o actualiza el controlador de partida para una sala.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala.</param>
        /// <returns>Controlador de partida.</returns>
        internal static ControladorPartida ObtenerOCrearControlador(string codigoSala)
        {
            return _controladores.GetOrAdd(codigoSala, codigo => new ControladorPartida(codigo));
        }

        /// <summary>
        /// Elimina el controlador de partida de una sala.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala.</param>
        internal static void EliminarControlador(string codigoSala)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                return;
            }

            _controladores.TryRemove(codigoSala, out _);
            _logger.InfoFormat("Controlador de partida eliminado para sala '{0}'.", codigoSala);
        }

        private static void ValidarCodigoSala(string codigoSala)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);
            }
        }

        private static string ObtenerNombreJugadorActual()
        {
            foreach (var entrada in _jugadorASala)
            {
                var callback = OperationContext.Current?.GetCallbackChannel<ICursoPartidaCallback>();
                if (callback != null)
                {
                    return entrada.Key;
                }
            }

            return null;
        }

        private void ConfigurarEventosCierreCanal(string nombreJugador, string codigoSala)
        {
            var canal = OperationContext.Current?.Channel;
            if (canal != null)
            {
                canal.Closed += (_, __) => DesregistrarJugadorInterno(nombreJugador, codigoSala);
                canal.Faulted += (_, __) => DesregistrarJugadorInterno(nombreJugador, codigoSala);
            }
        }

        private static void DesregistrarJugadorInterno(string nombreJugador, string codigoSala)
        {
            _jugadorASala.TryRemove(nombreJugador, out _);

            if (_controladores.TryGetValue(codigoSala, out var controlador))
            {
                controlador.DesregistrarJugador(nombreJugador);
            }
        }
    }
}
