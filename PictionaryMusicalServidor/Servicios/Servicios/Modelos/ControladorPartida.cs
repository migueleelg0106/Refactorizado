using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Servicios.Servicios.Modelos
{
    /// <summary>
    /// Controlador de la logica de una partida en curso.
    /// Gestiona el flujo del juego, validacion de adivinanzas y puntuaciones.
    /// </summary>
    internal sealed class ControladorPartida
    {
        private const int PuntosBaseAcierto = 50;
        private const int MultiplicadorTiempo = 2;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ControladorPartida));

        private readonly object _sincronizacion = new object();
        private readonly Dictionary<string, int> _puntuaciones;
        private readonly Dictionary<string, ICursoPartidaCallback> _callbacksJugadores;
        private readonly HashSet<string> _jugadoresQueAdivinaron;
        private readonly string _codigoSala;

        private DatosCancionDTO _cancionActual;
        private string _nombreDibujante;
        private int _tiempoRestante;
        private bool _rondaEnCurso;

        /// <summary>
        /// Evento que se dispara cuando un jugador adivina correctamente.
        /// </summary>
        public event Action<string, int> JugadorAdivino;

        /// <summary>
        /// Evento que se dispara cuando se envia un mensaje de chat normal.
        /// </summary>
        public event Action<string, string> MensajeChatEnviado;

        /// <summary>
        /// Evento que se dispara cuando termina la ronda.
        /// </summary>
        public event Action RondaTerminada;

        /// <summary>
        /// Inicializa un nuevo controlador de partida.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala de juego.</param>
        public ControladorPartida(string codigoSala)
        {
            _codigoSala = codigoSala;
            _puntuaciones = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _callbacksJugadores = new Dictionary<string, ICursoPartidaCallback>(
                StringComparer.OrdinalIgnoreCase);
            _jugadoresQueAdivinaron = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _rondaEnCurso = false;
        }

        /// <summary>
        /// Obtiene el codigo de la sala.
        /// </summary>
        public string CodigoSala => _codigoSala;

        /// <summary>
        /// Obtiene la cancion actual de la ronda.
        /// </summary>
        public DatosCancionDTO CancionActual => _cancionActual;

        /// <summary>
        /// Obtiene el nombre del jugador que dibuja actualmente.
        /// </summary>
        public string NombreDibujante => _nombreDibujante;

        /// <summary>
        /// Indica si hay una ronda en curso.
        /// </summary>
        public bool RondaEnCurso => _rondaEnCurso;

        /// <summary>
        /// Registra un jugador en la partida con su callback.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador.</param>
        /// <param name="callback">Callback del jugador para notificaciones.</param>
        public void RegistrarJugador(string nombreJugador, ICursoPartidaCallback callback)
        {
            if (string.IsNullOrWhiteSpace(nombreJugador) || callback == null)
            {
                return;
            }

            lock (_sincronizacion)
            {
                _callbacksJugadores[nombreJugador] = callback;

                if (!_puntuaciones.ContainsKey(nombreJugador))
                {
                    _puntuaciones[nombreJugador] = 0;
                }

                _logger.InfoFormat("Jugador '{0}' registrado en partida de sala '{1}'.",
                    nombreJugador, _codigoSala);
            }
        }

        /// <summary>
        /// Desregistra un jugador de la partida.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador.</param>
        public void DesregistrarJugador(string nombreJugador)
        {
            if (string.IsNullOrWhiteSpace(nombreJugador))
            {
                return;
            }

            lock (_sincronizacion)
            {
                _callbacksJugadores.Remove(nombreJugador);
                _logger.InfoFormat("Jugador '{0}' desregistrado de partida de sala '{1}'.",
                    nombreJugador, _codigoSala);
            }
        }

        /// <summary>
        /// Inicia una nueva ronda con la cancion y dibujante especificados.
        /// </summary>
        /// <param name="idCancion">Identificador de la cancion a adivinar.</param>
        /// <param name="nombreDibujante">Nombre del jugador que dibuja.</param>
        /// <param name="tiempoSegundos">Tiempo en segundos para la ronda.</param>
        /// <returns>True si se inicio correctamente, false en caso contrario.</returns>
        public bool IniciarRonda(int idCancion, string nombreDibujante, int tiempoSegundos)
        {
            lock (_sincronizacion)
            {
                var cancion = CatalogoCancionesLogico.ObtenerCancion(idCancion);
                if (cancion == null)
                {
                    _logger.WarnFormat("Intento de iniciar ronda con cancion inexistente: {0}",
                        idCancion);
                    return false;
                }

                _cancionActual = cancion;
                _nombreDibujante = nombreDibujante;
                _tiempoRestante = tiempoSegundos;
                _jugadoresQueAdivinaron.Clear();
                _rondaEnCurso = true;

                _logger.InfoFormat(
                    "Ronda iniciada en sala '{0}'. Cancion: '{1}', Dibujante: '{2}'.",
                    _codigoSala, cancion.Nombre, nombreDibujante);

                return true;
            }
        }

        /// <summary>
        /// Procesa un mensaje recibido de un jugador.
        /// Determina si es un intento de adivinanza valido o un mensaje de chat normal.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador que envia el mensaje.</param>
        /// <param name="mensaje">Contenido del mensaje.</param>
        public void ProcesarMensaje(string nombreJugador, string mensaje)
        {
            if (string.IsNullOrWhiteSpace(nombreJugador) || string.IsNullOrWhiteSpace(mensaje))
            {
                return;
            }

            lock (_sincronizacion)
            {
                if (!_rondaEnCurso)
                {
                    EnviarMensajeChatATodos(nombreJugador, mensaje);
                    return;
                }

                if (EsDibujante(nombreJugador))
                {
                    EnviarMensajeChatATodos(nombreJugador, mensaje);
                    return;
                }

                if (YaAdivino(nombreJugador))
                {
                    return;
                }

                if (EsAdivinanzaCorrecta(mensaje))
                {
                    ProcesarAcierto(nombreJugador);
                }
                else
                {
                    EnviarMensajeChatATodos(nombreJugador, mensaje);
                }
            }
        }

        /// <summary>
        /// Actualiza el tiempo restante de la ronda.
        /// </summary>
        /// <param name="tiempoRestante">Nuevo tiempo restante en segundos.</param>
        public void ActualizarTiempoRestante(int tiempoRestante)
        {
            lock (_sincronizacion)
            {
                _tiempoRestante = tiempoRestante;
            }
        }

        /// <summary>
        /// Termina la ronda actual.
        /// </summary>
        public void TerminarRonda()
        {
            lock (_sincronizacion)
            {
                if (!_rondaEnCurso)
                {
                    return;
                }

                _rondaEnCurso = false;
                string nombreCancion = _cancionActual?.Nombre ?? string.Empty;

                _logger.InfoFormat("Ronda terminada en sala '{0}'. Cancion: '{1}'.",
                    _codigoSala, nombreCancion);

                NotificarFinRondaATodos(nombreCancion);
                RondaTerminada?.Invoke();
            }
        }

        /// <summary>
        /// Obtiene las puntuaciones actuales de todos los jugadores.
        /// </summary>
        /// <returns>Diccionario con las puntuaciones por jugador.</returns>
        public Dictionary<string, int> ObtenerPuntuaciones()
        {
            lock (_sincronizacion)
            {
                return new Dictionary<string, int>(_puntuaciones);
            }
        }

        private bool EsDibujante(string nombreJugador)
        {
            return string.Equals(
                nombreJugador,
                _nombreDibujante,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool YaAdivino(string nombreJugador)
        {
            return _jugadoresQueAdivinaron.Contains(nombreJugador);
        }

        private bool EsAdivinanzaCorrecta(string mensaje)
        {
            if (_cancionActual == null)
            {
                return false;
            }

            return CatalogoCancionesLogico.ValidarTitulo(_cancionActual.IdCancion, mensaje);
        }

        private void ProcesarAcierto(string nombreJugador)
        {
            int puntos = CalcularPuntos();
            SumarPuntos(nombreJugador, puntos);
            _jugadoresQueAdivinaron.Add(nombreJugador);

            _logger.InfoFormat(
                "Jugador '{0}' adivino correctamente en sala '{1}'. Puntos: {2}.",
                nombreJugador, _codigoSala, puntos);

            NotificarAciertoATodos(nombreJugador, puntos);
            JugadorAdivino?.Invoke(nombreJugador, puntos);

            if (TodosAdivinaron())
            {
                TerminarRonda();
            }
        }

        private int CalcularPuntos()
        {
            return (_tiempoRestante * MultiplicadorTiempo) + PuntosBaseAcierto;
        }

        private void SumarPuntos(string nombreJugador, int puntos)
        {
            if (_puntuaciones.ContainsKey(nombreJugador))
            {
                _puntuaciones[nombreJugador] += puntos;
            }
            else
            {
                _puntuaciones[nombreJugador] = puntos;
            }
        }

        private bool TodosAdivinaron()
        {
            int jugadoresQueDebenAdivinar = _callbacksJugadores.Count - 1;
            return _jugadoresQueAdivinaron.Count >= jugadoresQueDebenAdivinar;
        }

        private void EnviarMensajeChatATodos(string nombreJugador, string mensaje)
        {
            foreach (var callback in _callbacksJugadores.Values.ToList())
            {
                EjecutarCallback(() => callback.RecibirMensajeChat(nombreJugador, mensaje));
            }

            MensajeChatEnviado?.Invoke(nombreJugador, mensaje);
        }

        private void NotificarAciertoATodos(string nombreJugador, int puntaje)
        {
            foreach (var callback in _callbacksJugadores.Values.ToList())
            {
                EjecutarCallback(() => callback.NotificarAcierto(nombreJugador, puntaje));
            }
        }

        private void NotificarFinRondaATodos(string nombreCancion)
        {
            foreach (var callback in _callbacksJugadores.Values.ToList())
            {
                EjecutarCallback(() => callback.NotificarFinRonda(nombreCancion));
            }
        }

        private static void EjecutarCallback(Action accion)
        {
            try
            {
                accion();
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                _logger.Warn("Error de comunicacion en callback de partida.", ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Warn("Timeout en callback de partida.", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Operacion invalida en callback de partida.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado en callback de partida.", ex);
            }
        }
    }
}
