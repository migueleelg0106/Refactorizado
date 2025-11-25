using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.LogicaNegocio;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementa una fachada para administrar el curso de las partidas activas.
    /// Gestiona los callbacks y delega la logica al <see cref="ControladorPartida"/> correspondiente por sala.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class CursoPartidaManejador : ICursoPartidaManejador
    {
        private const int TiempoRondaPorDefectoSegundos = 90;
        private const int NumeroRondasPorDefecto = 3;
        private const string DificultadPorDefecto = "Media";

        private static readonly ILog _logger = LogManager.GetLogger(typeof(CursoPartidaManejador));
        private static readonly Dictionary<string, ControladorPartida> _partidasActivas = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Dictionary<string, ICursoPartidaCallback>> _callbacksPorSala = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _sincronizacion = new();

        /// <summary>
        /// Registra a un jugador para recibir notificaciones de la partida de la sala especificada.
        /// Tambien agrega al jugador al controlador de partida correspondiente.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="idJugador">Identificador unico del jugador.</param>
        /// <param name="nombreUsuario">Nombre visible del jugador.</param>
        /// <param name="esHost">Indica si el jugador es el host de la sala.</param>
        public void SuscribirJugador(string idSala, string idJugador, string nombreUsuario, bool esHost)
        {
            if (string.IsNullOrWhiteSpace(idSala))
            {
                throw new FaultException("El identificador de sala es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(idJugador))
            {
                throw new FaultException("El identificador de jugador es obligatorio.");
            }

            var callback = ObtenerCallbackActual();
            var controlador = ObtenerOCrearControlador(idSala.Trim());

            controlador.AgregarJugador(idJugador.Trim(), nombreUsuario?.Trim() ?? string.Empty, esHost);
            RegistrarCallback(idSala.Trim(), idJugador.Trim(), callback);
        }

        /// <summary>
        /// Inicia la partida de la sala indicada.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="idJugadorSolicitante">Identificador del jugador que solicita el inicio.</param>
        public void IniciarPartida(string idSala, string idJugadorSolicitante)
        {
            if (string.IsNullOrWhiteSpace(idSala))
            {
                throw new FaultException("El identificador de sala es obligatorio.");
            }

            var controlador = ObtenerOCrearControlador(idSala.Trim());
            controlador.IniciarPartida(idJugadorSolicitante?.Trim());
        }

        /// <summary>
        /// Procesa un mensaje de chat enviado por un jugador durante la partida.
        /// </summary>
        /// <param name="mensaje">Mensaje a enviar.</param>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="idJugador">Identificador del jugador.</param>
        public void EnviarMensajeJuego(string mensaje, string idSala, string idJugador)
        {
            if (string.IsNullOrWhiteSpace(idSala))
            {
                throw new FaultException("El identificador de sala es obligatorio.");
            }

            var controlador = ObtenerOCrearControlador(idSala.Trim());
            controlador.ProcesarMensaje(idJugador?.Trim(), mensaje);
        }

        /// <summary>
        /// Procesa un trazo dibujado por el jugador actual de la sala.
        /// </summary>
        /// <param name="trazo">Trazo a procesar.</param>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="idJugador">Identificador del jugador.</param>
        public void EnviarTrazo(TrazoDTO trazo, string idSala, string idJugador)
        {
            if (string.IsNullOrWhiteSpace(idSala))
            {
                throw new FaultException("El identificador de sala es obligatorio.");
            }

            var controlador = ObtenerOCrearControlador(idSala.Trim());
            controlador.ProcesarTrazo(idJugador?.Trim(), trazo);
        }

        private ControladorPartida ObtenerOCrearControlador(string idSala)
        {
            lock (_sincronizacion)
            {
                if (_partidasActivas.TryGetValue(idSala, out var existente))
                {
                    return existente;
                }

                var controlador = new ControladorPartida(TiempoRondaPorDefectoSegundos, DificultadPorDefecto, NumeroRondasPorDefecto);
                SuscribirEventos(controlador, idSala);
                _partidasActivas[idSala] = controlador;
                _callbacksPorSala[idSala] = new Dictionary<string, ICursoPartidaCallback>(StringComparer.OrdinalIgnoreCase);

                _logger.InfoFormat("Controlador de partida creado para la sala {0}.", idSala);
                return controlador;
            }
        }

        private void SuscribirEventos(ControladorPartida controlador, string idSala)
        {
            controlador.PartidaIniciada += () => NotificarCallbacks(idSala, callback => callback.NotificarPartidaIniciada());
            controlador.InicioRonda += ronda => NotificarCallbacks(idSala, callback => callback.NotificarInicioRonda(ronda));
            controlador.JugadorAdivino += (jugador, puntos) => NotificarCallbacks(idSala, callback => callback.NotificarJugadorAdivino(jugador, puntos));
            controlador.MensajeChatRecibido += (jugador, mensaje) => NotificarCallbacks(idSala, callback => callback.NotificarMensajeChat(jugador, mensaje));
            controlador.TrazoRecibido += trazo => NotificarCallbacks(idSala, callback => callback.NotificarTrazoRecibido(trazo));
            controlador.FinRonda += () => NotificarCallbacks(idSala, callback => callback.NotificarFinRonda());
            controlador.FinPartida += resultado => NotificarCallbacks(idSala, callback => callback.NotificarFinPartida(resultado));
        }

        private void NotificarCallbacks(string idSala, Action<ICursoPartidaCallback> accion)
        {
            List<KeyValuePair<string, ICursoPartidaCallback>> callbacks;
            lock (_sincronizacion)
            {
                if (!_callbacksPorSala.TryGetValue(idSala, out var callbacksSala))
                {
                    return;
                }

                callbacks = callbacksSala.ToList();
            }

            foreach (var par in callbacks)
            {
                try
                {
                    accion(par.Value);
                }
                catch (CommunicationException ex)
                {
                    _logger.WarnFormat("Error de comunicacion con jugador {0} en sala {1}. Se quitará su callback.", par.Key, idSala);
                    _logger.Warn(ex);
                    RemoverCallback(idSala, par.Key);
                }
                catch (TimeoutException ex)
                {
                    _logger.WarnFormat("Timeout al notificar a jugador {0} en sala {1}. Se quitará su callback.", par.Key, idSala);
                    _logger.Warn(ex);
                    RemoverCallback(idSala, par.Key);
                }
            }
        }

        private void RegistrarCallback(string idSala, string idJugador, ICursoPartidaCallback callback)
        {
            lock (_sincronizacion)
            {
                if (!_callbacksPorSala.TryGetValue(idSala, out var callbacks))
                {
                    callbacks = new Dictionary<string, ICursoPartidaCallback>(StringComparer.OrdinalIgnoreCase);
                    _callbacksPorSala[idSala] = callbacks;
                }

                callbacks[idJugador] = callback;
            }

            var canal = OperationContext.Current?.Channel;
            if (canal != null)
            {
                canal.Closed += (_, __) => RemoverCallback(idSala, idJugador);
                canal.Faulted += (_, __) => RemoverCallback(idSala, idJugador);
            }
        }

        private void RemoverCallback(string idSala, string idJugador)
        {
            lock (_sincronizacion)
            {
                if (_callbacksPorSala.TryGetValue(idSala, out var callbacks))
                {
                    callbacks.Remove(idJugador);
                }

                if (_partidasActivas.TryGetValue(idSala, out var controlador))
                {
                    controlador.RemoverJugador(idJugador);
                }
            }
        }

        private static ICursoPartidaCallback ObtenerCallbackActual()
        {
            var contexto = OperationContext.Current;
            if (contexto == null)
            {
                throw new FaultException("No se encontro un contexto de operacion activo.");
            }

            var callback = contexto.GetCallbackChannel<ICursoPartidaCallback>();
            if (callback == null)
            {
                throw new FaultException("No se pudo obtener el callback del cliente.");
            }

            return callback;
        }
    }
}
