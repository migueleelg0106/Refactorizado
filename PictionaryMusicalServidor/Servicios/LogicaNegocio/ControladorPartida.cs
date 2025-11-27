using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Timers;
using log4net;
using PictionaryMusicalServidor.Datos;
using PictionaryMusicalServidor.Datos.Entidades;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.LogicaNegocio
{
    /// <summary>
    /// Gestiona el flujo central de la partida de Pictionary Musical.
    /// </summary>
    public class ControladorPartida
    {
        private const string RolDibujante = "Dibujante";
        private const string MensajeCancelacionFaltaJugadores = "No hay suficientes jugadores para seguir jugando, se canceló la partida.";
        private const int TiempoOverlayClienteSegundos = 5;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ControladorPartida));

        private readonly Dictionary<string, JugadorPartida> _jugadores = new Dictionary<string, JugadorPartida>(StringComparer.Ordinal);
        private readonly Queue<string> _colaDibujantes = new Queue<string>();
        private readonly HashSet<int> _cancionesUsadas = new HashSet<int>();
        private readonly object _sincronizacion = new object();
        private readonly Random _random = new Random();

        private readonly int _tiempoRondaSegundos;
        private readonly string _dificultad;
        private readonly int _cantidadRondas;
        private string _idiomaCanciones = "Español";

        private Timer _timerRonda;
        private Timer _timerTransicionRonda;
        private EstadoPartida _estadoActual = EstadoPartida.EnSalaEspera;
        private int _rondaActual;
        private int _cancionActualId;
        private DateTime _inicioRonda;
        private bool _rondaTerminadaPorTodosAdivinaron;

        public ControladorPartida(int tiempoRondaSegundos, string dificultad, int cantidadRondas)
        {
            if (tiempoRondaSegundos <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tiempoRondaSegundos));
            }

            if (cantidadRondas <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cantidadRondas));
            }

            if (string.IsNullOrWhiteSpace(dificultad))
            {
                throw new ArgumentException("La dificultad es obligatoria.", nameof(dificultad));
            }

            _tiempoRondaSegundos = tiempoRondaSegundos;
            _dificultad = dificultad.Trim();
            _cantidadRondas = cantidadRondas;

            _timerRonda = new Timer
            {
                AutoReset = false
            };
            _timerRonda.Elapsed += OnTiempoRondaCumplido;

            _timerTransicionRonda = new Timer
            {
                AutoReset = false,
                Interval = TiempoOverlayClienteSegundos * 1000
            };
            _timerTransicionRonda.Elapsed += OnTransicionRondaCompletada;
        }

        public event Action PartidaIniciada;
        public event Action<RondaDTO> InicioRonda;
        public event Action<string, int> JugadorAdivino;
        public event Action<string, string> MensajeChatRecibido;
        public event Action<TrazoDTO> TrazoRecibido;
        public event Action FinRonda;
        public event Action<ResultadoPartidaDTO> FinPartida;

        public EstadoPartida EstadoActual
        {
            get
            {
                lock (_sincronizacion)
                {
                    return _estadoActual;
                }
            }
        }

        public IReadOnlyCollection<JugadorPartida> ObtenerJugadores()
        {
            lock (_sincronizacion)
            {
                return _jugadores.Values.Select(j => j.CopiarDatosBasicos()).ToList();
            }
        }

        public void ConfigurarIdiomaCanciones(string idioma)
        {
            if (string.IsNullOrWhiteSpace(idioma))
            {
                throw new ArgumentException("El idioma de las canciones es obligatorio.", nameof(idioma));
            }

            lock (_sincronizacion)
            {
                _idiomaCanciones = idioma.Trim();
            }
        }

        public void AgregarJugador(string idConexion, string nombreUsuario, bool esHost)
        {
            if (string.IsNullOrWhiteSpace(idConexion))
            {
                throw new ArgumentException("El identificador de conexión es obligatorio.", nameof(idConexion));
            }

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));
            }

            lock (_sincronizacion)
            {
                if (_estadoActual != EstadoPartida.EnSalaEspera)
                {
                    throw new InvalidOperationException("No se pueden agregar jugadores cuando la partida ya inició.");
                }

                if (_jugadores.ContainsKey(idConexion))
                {
                    _jugadores[idConexion].NombreUsuario = nombreUsuario;
                    _jugadores[idConexion].EsHost = esHost;
                    _logger.InfoFormat("Jugador con conexión {0} actualizado como {1} (Host: {2}).", idConexion, nombreUsuario, esHost);
                    return;
                }

                var jugador = new JugadorPartida
                {
                    IdConexion = idConexion,
                    NombreUsuario = nombreUsuario,
                    EsHost = esHost,
                    PuntajeTotal = 0,
                    EsDibujante = false,
                    YaAdivino = false
                };

                _jugadores.Add(idConexion, jugador);

                _logger.InfoFormat("Jugador {0} agregado a la partida. Total jugadores: {1}.", nombreUsuario, _jugadores.Count);
            }
        }

        public void RemoverJugador(string idConexion)
        {
            if (string.IsNullOrWhiteSpace(idConexion))
            {
                return;
            }

            ResultadoPartidaDTO resultadoCancelacion = null;

            lock (_sincronizacion)
            {
                if (_jugadores.Remove(idConexion))
                {
                    _logger.InfoFormat("Jugador con conexión {0} removido de la partida.", idConexion);

                    if (_estadoActual == EstadoPartida.Jugando && QuedaSoloHost())
                    {
                        _estadoActual = EstadoPartida.Finalizada;
                        _timerRonda.Stop();
                        _timerTransicionRonda.Stop();

                        resultadoCancelacion = new ResultadoPartidaDTO
                        {
                            Clasificacion = ObtenerClasificacion(),
                            Mensaje = MensajeCancelacionFaltaJugadores
                        };
                    }
                }
            }

            if (resultadoCancelacion != null)
            {
                FinPartida?.Invoke(resultadoCancelacion);
            }
        }

        public void IniciarPartida(string idSolicitante)
        {
            lock (_sincronizacion)
            {
                if (_estadoActual != EstadoPartida.EnSalaEspera)
                {
                    throw new InvalidOperationException("La partida ya fue iniciada.");
                }

                if (_jugadores.Count < 2)
                {
                    throw new InvalidOperationException("Se requieren al menos dos jugadores para iniciar la partida.");
                }

                if (!_jugadores.TryGetValue(idSolicitante, out var solicitante))
                {
                    throw new KeyNotFoundException("El solicitante no está registrado en la partida.");
                }

                if (!solicitante.EsHost)
                {
                    var exception = new SecurityException("Solo el host puede iniciar la partida.");
                    _logger.Warn("Intento de inicio de partida por un usuario no autorizado.", exception);
                    throw exception;
                }

                _estadoActual = EstadoPartida.Jugando;
                _logger.Info("Partida iniciada correctamente.");
            }

            PartidaIniciada?.Invoke();
            IniciarNuevaRonda();
        }

        public void ProcesarMensaje(string idConexion, string mensaje)
        {
            if (string.IsNullOrWhiteSpace(idConexion))
            {
                throw new ArgumentException("El identificador de conexión es obligatorio.", nameof(idConexion));
            }

            JugadorPartida jugador;
            bool acierto;
            bool debeFinalizarRonda = false;
            int puntosObtenidos = 0;

            lock (_sincronizacion)
            {
                if (!_jugadores.TryGetValue(idConexion, out jugador))
                {
                    throw new KeyNotFoundException("El jugador no existe en la partida.");
                }
            }

            if (_estadoActual == EstadoPartida.EnSalaEspera)
            {
                MensajeChatRecibido?.Invoke(jugador.NombreUsuario, mensaje);
                return;
            }

            if (_estadoActual != EstadoPartida.Jugando)
            {
                return;
            }

            if (jugador.EsDibujante)
            {
                return;
            }

            lock (_sincronizacion)
            {
                if (jugador.YaAdivino)
                {
                    return;
                }

                acierto = CatalogoCancionesLogico.ValidarRespuesta(_cancionActualId, mensaje);

                if (acierto)
                {
                    jugador.YaAdivino = true;
                    puntosObtenidos = CalcularSegundosRestantes();
                    jugador.PuntajeTotal += puntosObtenidos;
                    debeFinalizarRonda = TodosAdivinaron();
                }
            }

            if (acierto)
            {
                _logger.InfoFormat("Jugador {0} adivinó la canción y obtuvo {1} puntos.", jugador.NombreUsuario, puntosObtenidos);
                JugadorAdivino?.Invoke(jugador.NombreUsuario, puntosObtenidos);

                if (debeFinalizarRonda)
                {
                    FinalizarRondaPorTodosAdivinaron();
                }
            }
            else
            {
                MensajeChatRecibido?.Invoke(jugador.NombreUsuario, mensaje);
            }
        }

        public void ProcesarTrazo(string idConexion, TrazoDTO trazo)
        {
            if (trazo == null)
            {
                throw new ArgumentNullException(nameof(trazo));
            }

            lock (_sincronizacion)
            {
                if (_estadoActual != EstadoPartida.Jugando || !_jugadores.TryGetValue(idConexion, out var jugador))
                {
                    return;
                }

                if (!jugador.EsDibujante)
                {
                    return;
                }
            }

            TrazoRecibido?.Invoke(trazo);
        }

        private void IniciarNuevaRonda()
        {
            RondaDTO ronda = null;
            bool debeFinalizar = false;

            lock (_sincronizacion)
            {
                if (_estadoActual != EstadoPartida.Jugando)
                {
                    return;
                }

                if (_colaDibujantes.Count == 0)
                {
                    if (_rondaActual >= _cantidadRondas)
                    {
                        _estadoActual = EstadoPartida.Finalizada;
                        debeFinalizar = true;
                    }
                    else
                    {
                        PrepararColaDibujantes();
                        _rondaActual++;
                    }
                }

                if (!debeFinalizar)
                {
                    SeleccionarDibujante();
                    var cancion = ObtenerCancionParaRonda();

                    _timerRonda.Stop();
                    _timerRonda.Interval = (_tiempoRondaSegundos + TiempoOverlayClienteSegundos) * 1000;
                    _inicioRonda = DateTime.UtcNow;
                    _timerRonda.Start();

                    ronda = CrearRondaDto(cancion);
                }
            }

            if (debeFinalizar)
            {
                NotificarFinPartida();
                return;
            }

            InicioRonda?.Invoke(ronda);
        }

        private void PrepararColaDibujantes()
        {
            _colaDibujantes.Clear();

            foreach (var jugador in _jugadores.Keys.OrderBy(_ => _random.Next()))
            {
                _colaDibujantes.Enqueue(jugador);
            }
        }

        private void SeleccionarDibujante()
        {
            foreach (var jugador in _jugadores.Values)
            {
                jugador.EsDibujante = false;
                jugador.YaAdivino = false;
            }

            while (_colaDibujantes.Count > 0)
            {
                var idDibujante = _colaDibujantes.Dequeue();

                if (_jugadores.TryGetValue(idDibujante, out var dibujante))
                {
                    dibujante.EsDibujante = true;
                    dibujante.YaAdivino = true;
                    return;
                }
            }

            throw new InvalidOperationException("No hay dibujantes disponibles para la ronda.");
        }

        private Cancion ObtenerCancionParaRonda()
        {
            var cancion = CatalogoCancionesLogico.ObtenerCancionAleatoria(_idiomaCanciones, _cancionesUsadas);
            _cancionActualId = cancion.Id;
            _cancionesUsadas.Add(cancion.Id);
            _logger.InfoFormat("Canción {0} seleccionada para la ronda {1}.", cancion.Nombre, _rondaActual);
            return cancion;
        }

        private RondaDTO CrearRondaDto(Cancion cancion)
        {
            string pistaArtista = null;
            string pistaGenero = null;
            var dificultadNormalizada = _dificultad.ToLowerInvariant();

            if (dificultadNormalizada.Equals("facil", StringComparison.OrdinalIgnoreCase))
            {
                pistaArtista = cancion.Artista;
                pistaGenero = cancion.Genero;
            }
            else if (dificultadNormalizada.Equals("media", StringComparison.OrdinalIgnoreCase))
            {
                pistaGenero = cancion.Genero;
            }

            return new RondaDTO
            {
                IdCancion = cancion.Id,
                Rol = RolDibujante,
                PistaArtista = pistaArtista,
                PistaGenero = pistaGenero,
                TiempoSegundos = _tiempoRondaSegundos
            };
        }

        private bool TodosAdivinaron()
        {
            return _jugadores.Values.Where(jugador => !jugador.EsDibujante).All(jugador => jugador.YaAdivino);
        }

        private int CalcularSegundosRestantes()
        {
            var transcurrido = (int)(DateTime.UtcNow - _inicioRonda).TotalSeconds;
            var transcurridoSinOverlay = Math.Max(0, transcurrido - TiempoOverlayClienteSegundos);
            var restante = _tiempoRondaSegundos - transcurridoSinOverlay;
            return Math.Max(0, restante);
        }

        private void OnTiempoRondaCumplido(object sender, ElapsedEventArgs e)
        {
            FinalizarRonda();
        }

        private void OnTransicionRondaCompletada(object sender, ElapsedEventArgs e)
        {
            ContinuarDespuesDeTransicion();
        }

        private void FinalizarRondaPorTodosAdivinaron()
        {
            bool partidaFinalizada = false;
            List<ClasificacionUsuarioDTO> clasificacion = null;

            lock (_sincronizacion)
            {
                if (_estadoActual != EstadoPartida.Jugando)
                {
                    return;
                }

                _timerRonda.Stop();
                _timerTransicionRonda.Stop();
                _rondaTerminadaPorTodosAdivinaron = true;

                partidaFinalizada = _colaDibujantes.Count == 0 && _rondaActual >= _cantidadRondas;
                clasificacion = ObtenerClasificacion();

                if (partidaFinalizada)
                {
                    _estadoActual = EstadoPartida.Finalizada;
                }
            }

            FinRonda?.Invoke();

            if (partidaFinalizada)
            {
                FinPartida?.Invoke(new ResultadoPartidaDTO
                {
                    Clasificacion = clasificacion
                });
            }
            else
            {
                _timerTransicionRonda.Start();
            }
        }

        private void ContinuarDespuesDeTransicion()
        {
            lock (_sincronizacion)
            {
                _timerTransicionRonda.Stop();
                _rondaTerminadaPorTodosAdivinaron = false;

                if (_estadoActual != EstadoPartida.Jugando)
                {
                    return;
                }
            }

            IniciarNuevaRonda();
        }

        private void FinalizarRonda()
        {
            bool partidaFinalizada = false;
            List<ClasificacionUsuarioDTO> clasificacion = null;

            lock (_sincronizacion)
            {
                if (_estadoActual != EstadoPartida.Jugando)
                {
                    return;
                }

                _timerRonda.Stop();
                _timerTransicionRonda.Stop();

                partidaFinalizada = _colaDibujantes.Count == 0 && _rondaActual >= _cantidadRondas;
                clasificacion = ObtenerClasificacion();

                if (partidaFinalizada)
                {
                    _estadoActual = EstadoPartida.Finalizada;
                }
            }

            FinRonda?.Invoke();

            if (partidaFinalizada)
            {
                FinPartida?.Invoke(new ResultadoPartidaDTO
                {
                    Clasificacion = clasificacion
                });
            }
            else
            {
                IniciarNuevaRonda();
            }
        }

        private List<ClasificacionUsuarioDTO> ObtenerClasificacion()
        {
            return _jugadores.Values
                .Select(jugador => new ClasificacionUsuarioDTO
                {
                    Usuario = jugador.NombreUsuario,
                    Puntos = jugador.PuntajeTotal,
                    RondasGanadas = 0
                })
                .OrderByDescending(j => j.Puntos)
                .ToList();
        }

        private void NotificarFinPartida()
        {
            List<ClasificacionUsuarioDTO> clasificacion;

            lock (_sincronizacion)
            {
                clasificacion = ObtenerClasificacion();
            }

            FinPartida?.Invoke(new ResultadoPartidaDTO
            {
                Clasificacion = clasificacion
            });
        }

        private bool QuedaSoloHost()
        {
            return _jugadores.Count == 1 && _jugadores.Values.All(jugador => jugador.EsHost);
        }
    }
}
