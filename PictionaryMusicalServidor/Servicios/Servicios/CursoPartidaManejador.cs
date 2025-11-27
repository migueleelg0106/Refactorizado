using log4net;
using PictionaryMusicalServidor.Datos;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.LogicaNegocio;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

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
        private const int LimitePalabrasMensaje = 150;

        private static readonly ILog _logger = LogManager.GetLogger(typeof(CursoPartidaManejador));
        private static readonly Dictionary<string, ControladorPartida> _partidasActivas = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Dictionary<string, ICursoPartidaManejadorCallback>> _callbacksPorSala = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _sincronizacion = new();

        private readonly IContextoFactory _contextoFactory;

        public CursoPartidaManejador()
            : this(new ContextoFactory())
        {
        }

        public CursoPartidaManejador(IContextoFactory contextoFactory)
        {
            _contextoFactory = contextoFactory ?? throw new ArgumentNullException(nameof(contextoFactory));
        }

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

            SalasManejador.MarcarPartidaComoIniciada(idSala.Trim());
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

            if (SuperaLimitePalabras(mensaje))
            {
                throw new FaultException($"El mensaje supera el límite de {LimitePalabrasMensaje} palabras.");
            }

            var controlador = ObtenerOCrearControlador(idSala.Trim());
            controlador.ProcesarMensaje(idJugador?.Trim(), mensaje);
        }

        private static bool SuperaLimitePalabras(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return false;
            }

            var palabras = mensaje.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            return palabras.Length > LimitePalabrasMensaje;
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

                var configuracion = ObtenerConfiguracionSala(idSala);
                var controlador = new ControladorPartida(
                    configuracion?.TiempoPorRondaSegundos ?? TiempoRondaPorDefectoSegundos,
                    configuracion?.Dificultad ?? DificultadPorDefecto,
                    configuracion?.NumeroRondas ?? NumeroRondasPorDefecto);

                if (!string.IsNullOrWhiteSpace(configuracion?.IdiomaCanciones))
                {
                    controlador.ConfigurarIdiomaCanciones(configuracion.IdiomaCanciones);
                }
                SuscribirEventos(controlador, idSala);
                _partidasActivas[idSala] = controlador;
                _callbacksPorSala[idSala] = new Dictionary<string, ICursoPartidaManejadorCallback>(StringComparer.OrdinalIgnoreCase);

                _logger.InfoFormat("Controlador de partida creado para la sala {0}.", idSala);
                return controlador;
            }
        }

        private void SuscribirEventos(ControladorPartida controlador, string idSala)
        {
            controlador.PartidaIniciada += () => NotificarCallbacks(idSala, callback => callback.NotificarPartidaIniciada());
            controlador.InicioRonda += rondaBase =>
            Task.Run(() =>
            {
                var jugadoresEstado = controlador.ObtenerJugadores();

                var dibujante = jugadoresEstado.FirstOrDefault(j => j.EsDibujante);
                string nombreDibujante = dibujante?.NombreUsuario ?? string.Empty;

                List<KeyValuePair<string, ICursoPartidaManejadorCallback>> callbacks;
                lock (_sincronizacion)
                {
                    if (!_callbacksPorSala.TryGetValue(idSala, out var callbacksSala)) return;
                    callbacks = callbacksSala.ToList();
                }

                var cancionActual = CatalogoCancionesLogico.ObtenerCancionPorId(rondaBase.IdCancion);

                foreach (var par in callbacks)
                {
                    var idJugador = par.Key;
                    var callback = par.Value;

                    var datosJugador = jugadoresEstado.FirstOrDefault(j =>
                        string.Equals(j.IdConexion, idJugador, StringComparison.OrdinalIgnoreCase));

                    bool esDibujante = datosJugador != null && datosJugador.EsDibujante;

                    string pistaArtista = rondaBase.PistaArtista;
                    string pistaGenero = rondaBase.PistaGenero;

                    if (esDibujante && cancionActual != null)
                    {
                        pistaArtista = cancionActual.Artista;
                        pistaGenero = cancionActual.Genero;
                    }

                    var rondaPersonalizada = new RondaDTO
                    {
                        IdCancion = rondaBase.IdCancion,
                        PistaArtista = pistaArtista,
                        PistaGenero = pistaGenero,
                        TiempoSegundos = rondaBase.TiempoSegundos,
                        Rol = esDibujante ? "Dibujante" : "Adivinador",
                        NombreDibujante = nombreDibujante
                    };

                    try
                    {
                        callback.NotificarInicioRonda(rondaPersonalizada);
                    }
                    catch (Exception ex)
                    {
                        _logger.WarnFormat("Error notificando inicio de ronda a {0}", idJugador, ex);
                        RemoverCallback(idSala, idJugador);
                    }
                }
            });
            controlador.JugadorAdivino += (jugador, puntos) => NotificarCallbacks(idSala, callback => callback.NotificarJugadorAdivino(jugador, puntos));
            controlador.MensajeChatRecibido += (jugador, mensaje) => NotificarCallbacks(idSala, callback => callback.NotificarMensajeChat(jugador, mensaje));
            controlador.TrazoRecibido += trazo => NotificarCallbacks(idSala, callback => callback.NotificarTrazoRecibido(trazo));
            controlador.FinRonda += () => NotificarCallbacks(idSala, callback => callback.NotificarFinRonda());
            controlador.FinPartida += resultado =>
            {
                Task.Run(() => ActualizarClasificacionPartida(controlador, resultado));
                NotificarCallbacks(idSala, callback => callback.NotificarFinPartida(resultado));
            };
        }

        private void ActualizarClasificacionPartida(ControladorPartida controlador, ResultadoPartidaDTO resultado)
        {
            if (controlador == null || resultado?.Clasificacion == null || !resultado.Clasificacion.Any())
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(resultado.Mensaje))
            {
                _logger.Info("La partida finalizó de forma anticipada o con mensaje informativo. No se actualizará la clasificación.");
                return;
            }

            List<JugadorPartida> jugadoresFinales;

            try
            {
                jugadoresFinales = controlador.ObtenerJugadores()?.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("Error al obtener los jugadores para actualizar la clasificación al finalizar la partida.", ex);
                return;
            }

            if (jugadoresFinales == null || jugadoresFinales.Count == 0)
            {
                return;
            }

            int puntajeMaximo = jugadoresFinales.Max(j => j.PuntajeTotal);
            var ganadores = jugadoresFinales
                .Where(j => j.PuntajeTotal == puntajeMaximo)
                .Select(j => j.IdConexion)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            try
            {
                using (var contexto = _contextoFactory.CrearContexto())
                {
                    var clasificacionRepositorio = new ClasificacionRepositorio(contexto);

                    foreach (var jugador in jugadoresFinales)
                    {
                        if (!int.TryParse(jugador.IdConexion, out int jugadorId) || jugadorId <= 0)
                        {
                            continue;
                        }

                        bool ganoPartida = ganadores.Contains(jugador.IdConexion);

                        try
                        {
                            clasificacionRepositorio.ActualizarEstadisticas(jugadorId, jugador.PuntajeTotal, ganoPartida);
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorFormat("No se pudo actualizar la clasificación del jugador {0}.", jugadorId, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al actualizar las clasificaciones de la partida.", ex);
            }
        }

        private void NotificarCallbacks(string idSala, Action<ICursoPartidaManejadorCallback> accion)
        {
            List<KeyValuePair<string, ICursoPartidaManejadorCallback>> callbacks;
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
                    if (!CanalActivo(par.Value))
                    {
                        _logger.WarnFormat("El canal de jugador {0} en sala {1} ya no está activo. Se quitará su callback.", par.Key, idSala);
                        RemoverCallback(idSala, par.Key);
                        continue;
                    }
                    accion(par.Value);
                }
                catch (ObjectDisposedException ex)
                {
                    _logger.WarnFormat("El canal de jugador {0} en sala {1} fue desechado. Se quitará su callback.", par.Key, idSala);
                    _logger.Warn(ex);
                    RemoverCallback(idSala, par.Key);
                }
                catch (CommunicationObjectFaultedException ex)
                {
                    _logger.WarnFormat("El canal de jugador {0} en sala {1} está en estado Faulted. Se quitará su callback.", par.Key, idSala);
                    _logger.Warn(ex);
                    RemoverCallback(idSala, par.Key);
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
        private static bool CanalActivo(ICursoPartidaManejadorCallback callback)
        {
            if (callback is ICommunicationObject canal)
            {
                return canal.State == CommunicationState.Opened;
            }

            return true;
        }

        private void RegistrarCallback(string idSala, string idJugador, ICursoPartidaManejadorCallback callback)
        {
            lock (_sincronizacion)
            {
                if (!_callbacksPorSala.TryGetValue(idSala, out var callbacks))
                {
                    callbacks = new Dictionary<string, ICursoPartidaManejadorCallback>(StringComparer.OrdinalIgnoreCase);
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

        private static ICursoPartidaManejadorCallback ObtenerCallbackActual()
        {
            var contexto = OperationContext.Current;
            if (contexto == null)
            {
                throw new FaultException("No se encontro un contexto de operacion activo.");
            }

            var callback = contexto.GetCallbackChannel<ICursoPartidaManejadorCallback>();
            if (callback == null)
            {
                throw new FaultException("No se pudo obtener el callback del cliente.");
            }

            return callback;
        }

        private ConfiguracionPartidaDTO ObtenerConfiguracionSala(string idSala)
        {
            try
            {
                return SalasManejador.ObtenerSalaPorCodigo(idSala)?.Configuracion;
            }
            catch (Exception ex)
            {
                _logger.WarnFormat("No se pudo obtener la configuracion de la sala {0}. Se usaran valores por defecto.", idSala);
                _logger.Warn(ex);
                return null;
            }
        }
    }
}
