using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using log4net;
using PictionaryMusicalServidor.Datos;
using PictionaryMusicalServidor.Datos.Entidades;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.LogicaNegocio
{
    /// <summary>
    /// Controlador principal (Orquestador) que coordina el flujo de la partida.
    /// Delega la logica especifica a los gestores de jugadores y tiempos, manteniendo la 
    /// coherencia del estado global.
    /// </summary>
    public class ControladorPartida
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(ControladorPartida));

        private const string RolDibujante = "Dibujante";
        private const int LimitePalabrasMensaje = 150;
        private const int TiempoOverlayClienteSegundos = 5;

        private readonly object _sincronizacion = new object();

        private readonly ICatalogoCanciones _catalogoCanciones;
        private readonly GestorJugadoresPartida _gestorJugadores;
        private readonly GestorTiemposPartida _gestorTiempos;
        private readonly HashSet<int> _cancionesUsadas;

        private readonly int _duracionRondaSegundos;
        private readonly string _dificultad;
        private readonly int _totalRondas;
        private string _idiomaCanciones = "Espanol";

        private EstadoPartida _estadoActual;
        private int _rondaActual;
        private int _cancionActualId;
        private bool _rondaFinalizadaPorAciertos;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de partida.
        /// </summary>
        /// <param name="tiempoRonda">Tiempo limite en segundos por ronda.</param>
        /// <param name="dificultad">Nivel de dificultad de la partida (facil, medio, dificil).
        /// </param>
        /// <param name="totalRondas">Numero total de rondas a jugar.</param>
        /// <param name="catalogo">Servicio de catalogo de canciones inyectado.</param>
        /// <exception cref="ArgumentOutOfRangeException">Se lanza si el tiempo o las rondas son 
        /// invalidos.</exception>
        /// <exception cref="ArgumentNullException">Se lanza si el catalogo es nulo.</exception>
        public ControladorPartida(int tiempoRonda, string dificultad, int totalRondas, 
            ICatalogoCanciones catalogo, GestorJugadoresPartida gestorJugadores)
        {
            if (tiempoRonda <= 0 || totalRondas <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (string.IsNullOrWhiteSpace(dificultad))
            {
                throw new ArgumentException(nameof(dificultad));
            }

            _catalogoCanciones = catalogo ??
                throw new ArgumentNullException(nameof(catalogo));

            _gestorJugadores = gestorJugadores ??
                throw new ArgumentNullException(nameof(gestorJugadores));

            _duracionRondaSegundos = tiempoRonda;
            _dificultad = dificultad.Trim();
            _totalRondas = totalRondas;

            _gestorTiempos = new GestorTiemposPartida(tiempoRonda, TiempoOverlayClienteSegundos);
            _cancionesUsadas = new HashSet<int>();
            _estadoActual = EstadoPartida.EnSalaEspera;

            SuscribirEventosTiempo();
        }

        /// <summary>
        /// Evento que se dispara cuando la partida cambia al estado Jugando.
        /// </summary>
        public event Action PartidaIniciada;

        /// <summary>
        /// Evento que notifica el inicio de una nueva ronda con sus datos correspondientes.
        /// </summary>
        public event Action<RondaDTO> InicioRonda;

        /// <summary>
        /// Evento que notifica cuando un jugador ha acertado la cancion.
        /// </summary>
        public event Action<string, int> JugadorAdivino;

        /// <summary>
        /// Evento que retransmite un mensaje de chat publico a los clientes.
        /// </summary>
        public event Action<string, string> MensajeChatRecibido;

        /// <summary>
        /// Evento que retransmite los datos de un trazo de dibujo.
        /// </summary>
        public event Action<TrazoDTO> TrazoRecibido;

        /// <summary>
        /// Evento que notifica la finalizacion de una ronda.
        /// </summary>
        public event Action FinRonda;

        /// <summary>
        /// Evento que notifica el fin de la partida y envia los resultados finales.
        /// </summary>
        public event Action<ResultadoPartidaDTO> FinPartida;

        /// <summary>
        /// Indica si la partida ya llego a su estado finalizado.
        /// </summary>
        public bool EstaFinalizada
        {
            get
            {
                lock (_sincronizacion)
                {
                    return _estadoActual == EstadoPartida.Finalizada;
                }
            }
        }

        /// <summary>
        /// Configura el idioma que se utilizara para seleccionar las canciones.
        /// </summary>
        /// <param name="idioma">Nombre del idioma (Ej. Espanol, Ingles).</param>
        public void ConfigurarIdiomaCanciones(string idioma)
        {
            lock (_sincronizacion)
            {
                _idiomaCanciones = idioma;
            }
        }

        /// <summary>
        /// Intenta agregar un nuevo jugador a la partida.
        /// </summary>
        /// <param name="id">Identificador de conexion.</param>
        /// <param name="nombre">Nombre de usuario.</param>
        /// <param name="esHost">Indica si es el creador de la partida.</param>
        /// <exception cref="InvalidOperationException">Se lanza si la partida ya ha iniciado.
        /// </exception>
        public void AgregarJugador(string id, string nombre, bool esHost)
        {
            lock (_sincronizacion)
            {
                if (_estadoActual != EstadoPartida.EnSalaEspera)
                {
                    throw new InvalidOperationException(MensajesError.Cliente.PartidaYaIniciada);
                }

                _gestorJugadores.Agregar(id, nombre, esHost);
            }
        }

        /// <summary>
        /// Remueve a un jugador y maneja la logica de desconexion (cancelacion o avance de turno).
        /// </summary>
        /// <param name="id">Identificador de conexion del jugador.</param>
        public void RemoverJugador(string id)
        {
            bool eraDibujante;
            bool debeCancelar = false;
            bool debeAvanzar = false;
            bool eraAnfitrion;
            string mensajeCancelacion = null;

            lock (_sincronizacion)
            {
                eraAnfitrion = _gestorJugadores.EsHost(id);
                if (!_gestorJugadores.Remover(id, out eraDibujante))
                {
                    return;
                }
                if (_estadoActual == EstadoPartida.Jugando)
                {
                    if (!_gestorJugadores.HaySuficientesJugadores)
                    {
                        debeCancelar = true;
                        mensajeCancelacion = eraAnfitrion
                            ? MensajesError.Cliente.PartidaCanceladaHostSalio
                            : MensajesError.Cliente.PartidaCanceladaFaltaJugadores;
                    }
                    else if (eraDibujante)
                    {
                        debeAvanzar = true;
                    }
                }
            }

            if (debeCancelar)
            {
                CancelarPartida(mensajeCancelacion);
            }
            else if (debeAvanzar)
            {
                FinalizarRondaActual();
            }
        }

        /// <summary>
        /// Inicia el flujo de juego de la partida.
        /// </summary>
        /// <param name="idSolicitante">ID del usuario que solicita el inicio.</param>
        public void IniciarPartida(string idSolicitante)
        {
            lock (_sincronizacion)
            {
                ValidarInicioPartida(idSolicitante);
                _estadoActual = EstadoPartida.Jugando;
            }

            PartidaIniciada?.Invoke();
            PrepararSiguienteRonda();
        }

        /// <summary>
        /// Procesa un mensaje de chat, verificando si es un intento de adivinanza o charla normal.
        /// </summary>
        /// <param name="id">ID del emisor.</param>
        /// <param name="mensaje">Contenido del mensaje.</param>
        public void ProcesarMensaje(string id, string mensaje)
        {
            if (EsMensajeInvalido(id, mensaje))
            {
                return;
            }

            JugadorPartida jugador;
            lock (_sincronizacion)
            {
                jugador = _gestorJugadores.Obtener(id);
                if (jugador == null)
                {
                    return;
                }
            }

            if (_estadoActual == EstadoPartida.EnSalaEspera)
            {
                MensajeChatRecibido?.Invoke(jugador.NombreUsuario, mensaje);
                return;
            }

            ProcesarIntentoAdivinanza(jugador, mensaje);
        }

        /// <summary>
        /// Procesa y retransmite un trazo de dibujo si proviene del dibujante actual.
        /// </summary>
        /// <param name="id">ID del emisor.</param>
        /// <param name="trazo">Objeto de transferencia con datos del trazo.</param>
        public void ProcesarTrazo(string id, TrazoDTO trazo)
        {
            lock (_sincronizacion)
            {
                var jugador = _gestorJugadores.Obtener(id);
                if (_estadoActual == EstadoPartida.Jugando && jugador != null 
                    && jugador.EsDibujante)
                {
                    TrazoRecibido?.Invoke(trazo);
                }
            }
        }

        /// <summary>
        /// Obtiene una copia segura de la lista de jugadores actuales en la partida.
        /// </summary>
        public IEnumerable<JugadorPartida> ObtenerJugadores()
        {
            lock (_sincronizacion)
            {
                return _gestorJugadores.ObtenerCopiaLista();
            }
        }

        private void ValidarInicioPartida(string id)
        {
            if (_estadoActual != EstadoPartida.EnSalaEspera)
            {
                throw new InvalidOperationException(MensajesError.Cliente.PartidaYaIniciada);
            }
            if (!_gestorJugadores.HaySuficientesJugadores)
            {
                throw new InvalidOperationException(MensajesError.Cliente.FaltanJugadores);
            }
            if (!_gestorJugadores.EsHost(id))
            {
                throw new SecurityException(MensajesError.Cliente.SoloHost);
            }
        }

        private void ProcesarIntentoAdivinanza(JugadorPartida jugador, string mensaje)
        {
            bool acierto = false;
            int puntos = 0;
            bool finRonda = false;

            lock (_sincronizacion)
            {
                if (JugadorPuedeAdivinar(jugador))
                {
                    acierto = VerificarAcierto(mensaje, out puntos);
                    if (acierto)
                    {
                        RegistrarAcierto(jugador, puntos);
                        finRonda = _gestorJugadores.TodosAdivinaron();
                    }
                }
            }

            if (acierto)
            {
                JugadorAdivino?.Invoke(jugador.NombreUsuario, puntos);
                if (finRonda)
                {
                    FinalizarRondaAnticipada();
                }
            }
            else
            {
                MensajeChatRecibido?.Invoke(jugador.NombreUsuario, mensaje);
            }
        }

        private bool JugadorPuedeAdivinar(JugadorPartida jugador)
        {
            return _estadoActual == EstadoPartida.Jugando && !jugador.EsDibujante 
                && !jugador.YaAdivino;
        }

        private bool VerificarAcierto(string mensaje, out int puntos)
        {
            puntos = 0;
            bool esCorrecto = _catalogoCanciones.ValidarRespuesta(_cancionActualId, mensaje);

            if (!esCorrecto && EsMensajeAciertoProtocolo(mensaje, out int puntosProtocolo))
            {
                esCorrecto = true;
                puntos = puntosProtocolo;
            }

            if (esCorrecto && puntos == 0)
            {
                puntos = _gestorTiempos.CalcularPuntosPorTiempo();
            }

            return esCorrecto;
        }

        private void RegistrarAcierto(JugadorPartida jugador, int puntos)
        {
            jugador.YaAdivino = true;
            jugador.PuntajeTotal += puntos;
        }

        private void PrepararSiguienteRonda()
        {
            RondaDTO rondaDto = null;
            bool finJuego = false;

            lock (_sincronizacion)
            {
                if (_estadoActual != EstadoPartida.Jugando)
                {
                    return;
                }

                if (!_gestorJugadores.QuedanDibujantesPendientes())
                {
                    if (_rondaActual >= _totalRondas)
                    {
                        finJuego = true;
                    }
                    else
                    {
                        _rondaActual++;
                        _gestorJugadores.PrepararColaDibujantes();
                    }
                }

                if (!finJuego)
                {
                    _gestorJugadores.SeleccionarSiguienteDibujante();
                    var cancion = _catalogoCanciones.ObtenerCancionAleatoria(_idiomaCanciones, 
                        _cancionesUsadas);
                    _cancionActualId = cancion.Id;
                    _cancionesUsadas.Add(cancion.Id);

                    rondaDto = CrearRondaDto(cancion);
                    IniciarTimerRondaConRetardo();
                }
                else
                {
                    _estadoActual = EstadoPartida.Finalizada;
                }
            }

            if (finJuego)
            {
                NotificarFinPartida();
            }
            else
            {
                InicioRonda?.Invoke(rondaDto);
            }
        }

        private void IniciarTimerRondaConRetardo()
        {
            Task.Delay(TiempoOverlayClienteSegundos * 1000).ContinueWith(_ =>
            {
                lock (_sincronizacion)
                {
                    if (_estadoActual == EstadoPartida.Jugando)
                    {
                        _gestorTiempos.IniciarRonda();
                    }
                }
            });
        }

        private void FinalizarRondaActual(bool forzarPorAciertos = false)
        {
            lock (_sincronizacion)
            {
                if (_estadoActual != EstadoPartida.Jugando)
                {
                    return;
                }
                if (_rondaFinalizadaPorAciertos && !forzarPorAciertos)
                {
                    return;
                }
                _gestorTiempos.DetenerTodo();
            }

            FinRonda?.Invoke();
            EvaluarContinuidadPartida();
        }

        private void FinalizarRondaAnticipada()
        {
            bool yaFinalizada;

            lock (_sincronizacion)
            {
                yaFinalizada = _rondaFinalizadaPorAciertos;
                _rondaFinalizadaPorAciertos = true;
            }

            if (!yaFinalizada)
            {
                FinalizarRondaActual(true);
            }
        }

        private void EvaluarContinuidadPartida()
        {
            bool esFin = false;
            lock (_sincronizacion)
            {
                if (_estadoActual == EstadoPartida.Finalizada ||
                    (_rondaActual >= _totalRondas && 
                    !_gestorJugadores.QuedanDibujantesPendientes()))
                {
                    _estadoActual = EstadoPartida.Finalizada;
                    esFin = true;
                }
                else
                {
                    _rondaFinalizadaPorAciertos = false;
                    _gestorTiempos.IniciarTransicion();
                }
            }

            if (esFin)
            {
                NotificarFinPartida();
            }
        }

        private void CancelarPartida(string mensajeCancelacion)
        {
            bool debeNotificar = false;

            lock (_sincronizacion)
            {
                if (_estadoActual == EstadoPartida.Finalizada)
                {
                    return;
                }

                _estadoActual = EstadoPartida.Finalizada;
                _gestorTiempos.DetenerTodo();
                debeNotificar = true;
            }

            if (debeNotificar)
            {
                FinPartida?.Invoke(new ResultadoPartidaDTO
                {
                    Clasificacion = _gestorJugadores.GenerarClasificacion(),
                    Mensaje = mensajeCancelacion
                        ?? MensajesError.Cliente.PartidaCanceladaFaltaJugadores
                });
            }
        }

        private void NotificarFinPartida()
        {
            FinPartida?.Invoke(new ResultadoPartidaDTO
            {
                Clasificacion = _gestorJugadores.GenerarClasificacion()
            });
        }

        private void SuscribirEventosTiempo()
        {
            _gestorTiempos.TiempoRondaAgotado += () => FinalizarRondaActual();
            _gestorTiempos.TiempoTransicionAgotado += PrepararSiguienteRonda;
        }

        private bool EsMensajeInvalido(string id, string mensaje)
        {
            return string.IsNullOrWhiteSpace(id) ||
                   string.IsNullOrWhiteSpace(mensaje) ||
                   mensaje.Split(' ').Length > LimitePalabrasMensaje;
        }

        private static bool EsMensajeAciertoProtocolo(string mensaje, out int puntos)
        {
            puntos = 0;
            if (!mensaje.StartsWith("ACIERTO:", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var partes = mensaje.Split(':');
            return partes.Length >= 3 && int.TryParse(partes[2], out puntos) && puntos > 0;
        }

        private RondaDTO CrearRondaDto(Cancion cancion)
        {
            string genero = null, artista = null;
            if (_dificultad.Equals("facil", StringComparison.OrdinalIgnoreCase))
            {
                artista = cancion.Artista;
            }
            if (!_dificultad.Equals("dificil", StringComparison.OrdinalIgnoreCase))
            {
                genero = cancion.Genero;
            }

            return new RondaDTO
            {
                IdCancion = cancion.Id,
                Rol = RolDibujante,
                PistaArtista = artista,
                PistaGenero = genero,
                TiempoSegundos = _duracionRondaSegundos
            };
        }
    }
}