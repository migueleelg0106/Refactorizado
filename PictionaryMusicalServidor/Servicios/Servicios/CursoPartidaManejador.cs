using log4net;
using PictionaryMusicalServidor.Datos;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.LogicaNegocio;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementa una fachada para administrar el curso de las partidas activas.
    /// Gestiona los callbacks y delega la logica al ControladorPartida por sala.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class CursoPartidaManejador : ICursoPartidaManejador
    {
        private const int TiempoRondaPorDefectoSegundos = 90;
        private const int NumeroRondasPorDefecto = 3;
        private const string DificultadPorDefecto = "Media";
        private const int LimiteCaracteresMensaje = 150;

        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(CursoPartidaManejador));

        private static readonly Dictionary<string, ControladorPartida> _partidasActivas =
            new Dictionary<string, ControladorPartida>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string,
            Dictionary<string, ICursoPartidaManejadorCallback>> _callbacksPorSala =
            new Dictionary<string, Dictionary<string, ICursoPartidaManejadorCallback>>(
                StringComparer.OrdinalIgnoreCase);

        private static readonly object _sincronizacion = new object();

        private readonly IContextoFactoria _contextoFactory;
        private readonly ISalasManejador _salasManejador;
        private readonly ICatalogoCanciones _catalogoCanciones;

        public CursoPartidaManejador() : this(
            new ContextoFactoria(),
            new SalasManejador(),
            new CatalogoCanciones())
        {
        }

        public CursoPartidaManejador(
            IContextoFactoria contextoFactory,
            ISalasManejador salasManejador,
            ICatalogoCanciones catalogoCanciones)
        {
            _contextoFactory = contextoFactory
                ?? throw new ArgumentNullException(nameof(contextoFactory));

            _salasManejador = salasManejador
                ?? throw new ArgumentNullException(nameof(salasManejador));

            _catalogoCanciones = catalogoCanciones
                ?? throw new ArgumentNullException(nameof(catalogoCanciones));
        }

        /// <summary>
        /// Registra a un jugador para recibir notificaciones de la partida.
        /// </summary>
        public void SuscribirJugador(string idSala, string idJugador, string nombreUsuario,
            bool esHost)
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

            controlador.AgregarJugador(
                idJugador.Trim(),
                nombreUsuario?.Trim() ?? string.Empty,
                esHost);

            RegistrarCallback(idSala.Trim(), idJugador.Trim(), callback);

            _logger.InfoFormat(
                "Jugador id {0} suscrito para partida en sala {1}.",
                idJugador.Trim(),
                idSala.Trim());
        }

        /// <summary>
        /// Inicia la partida de la sala indicada.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="idJugadorSolicitante">Identificador del jugador que solicita el inicio.
        /// </param>
        public void IniciarPartida(string idSala, string idJugadorSolicitante)
        {
            if (string.IsNullOrWhiteSpace(idSala))
            {
                throw new FaultException("El identificador de sala es obligatorio.");
            }

            _logger.InfoFormat(
                "Inicio de partida solicitado para sala {0} por jugador id {1}.",
                idSala.Trim(),
                idJugadorSolicitante?.Trim());

            _salasManejador.MarcarPartidaComoIniciada(idSala.Trim());
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

            if (SuperaLimiteCaracteres(mensaje))
            {
                throw new FaultException("El mensaje supera el limite de caracteres.");
            }

            var controlador = ObtenerOCrearControlador(idSala.Trim());
            controlador.ProcesarMensaje(idJugador?.Trim(), mensaje);
        }

        private static bool SuperaLimiteCaracteres(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return false;
            }

            return mensaje.Length > LimiteCaracteresMensaje;
        }

        /// <summary>
        /// Procesa un trazo dibujado por el jugador actual de la sala.
        /// </summary>
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
                var gestorJugadores = new GestorJugadoresPartida();

                var controlador = new ControladorPartida(
                    configuracion?.TiempoPorRondaSegundos ?? TiempoRondaPorDefectoSegundos,
                    configuracion?.Dificultad ?? DificultadPorDefecto,
                    configuracion?.NumeroRondas ?? NumeroRondasPorDefecto,
                    _catalogoCanciones,
                    gestorJugadores);

                if (!string.IsNullOrWhiteSpace(configuracion?.IdiomaCanciones))
                {
                    controlador.ConfigurarIdiomaCanciones(configuracion.IdiomaCanciones);
                }

                SuscribirEventos(controlador, idSala);
                _partidasActivas[idSala] = controlador;
                _callbacksPorSala[idSala] =
                    new Dictionary<string, ICursoPartidaManejadorCallback>(
                        StringComparer.OrdinalIgnoreCase);

                return controlador;
            }
        }

        private void SuscribirEventos(ControladorPartida controlador, string idSala)
        {
            controlador.PartidaIniciada += () => ManejarPartidaIniciada(idSala);

            controlador.InicioRonda += (ronda) =>
                Task.Run(() => ManejarInicioRonda(idSala, ronda, controlador));

            controlador.JugadorAdivino += (jugador, puntos) =>
                ManejarJugadorAdivino(idSala, jugador, puntos);

            controlador.MensajeChatRecibido += (jugador, mensaje) =>
                ManejarMensajeChat(idSala, jugador, mensaje);

            controlador.TrazoRecibido += (trazo) =>
                ManejarTrazoRecibido(idSala, trazo);

            controlador.FinRonda += () => ManejarFinRonda(idSala);

            controlador.FinPartida += (resultado) =>
                ManejarFinPartida(idSala, resultado, controlador);
        }

        private void ManejarPartidaIniciada(string idSala)
        {
            NotificarCallbacks(idSala, cb => cb.NotificarPartidaIniciada());
        }

        private void ManejarInicioRonda(
            string idSala,
            RondaDTO rondaBase,
            ControladorPartida controlador)
        {
            var jugadoresEstado = controlador.ObtenerJugadores();
            var dibujante = jugadoresEstado.FirstOrDefault(j => j.EsDibujante);
            string nombreDibujante = dibujante?.NombreUsuario ?? string.Empty;

            List<KeyValuePair<string, ICursoPartidaManejadorCallback>> callbacks;

            lock (_sincronizacion)
            {
                if (!_callbacksPorSala.TryGetValue(idSala, out var callbacksSala))
                {
                    return;
                }
                callbacks = callbacksSala.ToList();
            }

            var cancionActual = _catalogoCanciones.ObtenerCancionPorId(rondaBase.IdCancion);

            foreach (var par in callbacks)
            {
                NotificarInicioRondaIndividual(
                    idSala,
                    par.Key,
                    par.Value,
                    rondaBase,
                    jugadoresEstado,
                    nombreDibujante,
                    cancionActual);
            }
        }

        private void NotificarInicioRondaIndividual(
            string idSala,
            string idJugador,
            ICursoPartidaManejadorCallback callback,
            RondaDTO rondaBase,
            IEnumerable<JugadorPartida> jugadoresEstado,
            string nombreDibujante,
            Datos.Entidades.Cancion cancionActual)
        {
            var datosJugador = jugadoresEstado.FirstOrDefault(j =>
                string.Equals(
                    j.IdConexion,
                    idJugador,
                    StringComparison.OrdinalIgnoreCase));

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
            catch (CommunicationException ex)
            {
                _logger.WarnFormat(
                    "Error notificando inicio de ronda a {0}",
                    idJugador,
                    ex);

                RemoverCallback(idSala, idJugador);
            }
            catch (TimeoutException ex)
            {
                _logger.WarnFormat(
                    "Error notificando inicio de ronda a {0}",
                    idJugador,
                    ex);

                RemoverCallback(idSala, idJugador);
            }
            catch (ObjectDisposedException ex)
            {
                _logger.WarnFormat(
                    "Error notificando inicio de ronda a {0}",
                    idJugador,
                    ex);

                RemoverCallback(idSala, idJugador);
            }
        }

        private void ManejarJugadorAdivino(string idSala, string jugador, int puntos)
        {
            NotificarCallbacks(
                idSala,
                cb => cb.NotificarJugadorAdivino(jugador, puntos));
        }

        private void ManejarMensajeChat(string idSala, string jugador, string mensaje)
        {
            NotificarCallbacks(
                idSala,
                cb => cb.NotificarMensajeChat(jugador, mensaje));
        }

        private void ManejarTrazoRecibido(string idSala, TrazoDTO trazo)
        {
            NotificarCallbacks(idSala, cb => cb.NotificarTrazoRecibido(trazo));
        }

        private void ManejarFinRonda(string idSala)
        {
            NotificarCallbacks(idSala, cb => cb.NotificarFinRonda());
        }

        private void ManejarFinPartida(
            string idSala,
            ResultadoPartidaDTO resultado,
            ControladorPartida controlador)
        {
            _salasManejador.MarcarPartidaComoFinalizada(idSala);
            Task.Run(() => ActualizarClasificacionPartida(controlador, resultado));
            NotificarCallbacks(idSala, cb => cb.NotificarFinPartida(resultado));
        }

        private void ActualizarClasificacionPartida(
            ControladorPartida controlador,
            ResultadoPartidaDTO resultado)
        {
            if (controlador == null ||
                resultado?.Clasificacion == null ||
                !resultado.Clasificacion.Any())
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(resultado.Mensaje))
            {
                _logger.Info("Partida finalizada sin clasificacion por mensaje de error.");
                return;
            }

            var jugadoresFinales = ObtenerJugadoresFinales(controlador);

            if (jugadoresFinales == null || jugadoresFinales.Count == 0)
            {
                return;
            }

            var ganadores = CalcularGanadores(jugadoresFinales);

            try
            {
                using (var contexto = _contextoFactory.CrearContexto())
                {
                    var clasificacionRepositorio = new ClasificacionRepositorio(contexto);

                    foreach (var jugador in jugadoresFinales)
                    {
                        PersistirEstadisticasJugador(
                            clasificacionRepositorio,
                            jugador,
                            ganadores);
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error inesperado al actualizar clasificaciones.", ex);
            }
            catch (EntityException ex)
            {
                _logger.Error("Error inesperado al actualizar clasificaciones.", ex);
            }
            catch (DataException ex)
            {
                _logger.Error("Error inesperado al actualizar clasificaciones.", ex);
            }
        }

        private List<JugadorPartida> ObtenerJugadoresFinales(ControladorPartida controlador)
        {
            try
            {
                return controlador.ObtenerJugadores()?.ToList();
            }
            catch (DbUpdateException ex)
            {
                _logger.Error(
                    "Error al obtener jugadores para actualizar clasificacion.",
                    ex);
                return new List<JugadorPartida>();
            }
            catch (EntityException ex)
            {
                _logger.Error(
                    "Error al obtener jugadores para actualizar clasificacion.",
                    ex);
                return new List<JugadorPartida>();
            }
            catch (DataException ex)
            {
                _logger.Error(
                    "Error al obtener jugadores para actualizar clasificacion.",
                    ex);
                return new List<JugadorPartida>();
            }
        }

        private HashSet<string> CalcularGanadores(List<JugadorPartida> jugadores)
        {
            int puntajeMaximo = jugadores.Max(j => j.PuntajeTotal);

            return jugadores
                .Where(j => j.PuntajeTotal == puntajeMaximo)
                .Select(j => j.IdConexion)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private void PersistirEstadisticasJugador(
            ClasificacionRepositorio repositorio,
            JugadorPartida jugador,
            HashSet<string> ganadores)
        {
            if (!int.TryParse(jugador.IdConexion, out int jugadorId) || jugadorId <= 0)
            {
                return;
            }

            bool ganoPartida = ganadores.Contains(jugador.IdConexion);

            try
            {
                repositorio.ActualizarEstadisticas(
                    jugadorId,
                    jugador.PuntajeTotal,
                    ganoPartida);
            }
            catch (DbUpdateException ex)
            {
                _logger.ErrorFormat(
                    "No se pudo actualizar clasificacion del jugador {0}.",
                    jugadorId,
                    ex);
            }
            catch (EntityException ex)
            {
                _logger.ErrorFormat(
                    "No se pudo actualizar clasificacion del jugador {0}.",
                    jugadorId,
                    ex);
            }
            catch (DataException ex)
            {
                _logger.ErrorFormat(
                    "No se pudo actualizar clasificacion del jugador {0}.",
                    jugadorId,
                    ex);
            }
        }

        private void NotificarCallbacks(
            string idSala,
            Action<ICursoPartidaManejadorCallback> accion)
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
                        _logger.WarnFormat(
                            "Canal inactivo para jugador {0} en sala {1}. Removiendo.",
                            par.Key,
                            idSala);

                        RemoverCallback(idSala, par.Key);
                        continue;
                    }
                    accion(par.Value);
                }
                catch (ObjectDisposedException ex)
                {
                    _logger.WarnFormat(
                        "Canal desechado para jugador {0} en sala {1}. Removiendo.",
                        par.Key,
                        idSala);
                    _logger.Warn(ex);
                    RemoverCallback(idSala, par.Key);
                }
                catch (CommunicationObjectFaultedException ex)
                {
                    _logger.WarnFormat(
                        "Canal en falta para jugador {0} en sala {1}. Removiendo.",
                        par.Key,
                        idSala);
                    _logger.Warn(ex);
                    RemoverCallback(idSala, par.Key);
                }
                catch (CommunicationException ex)
                {
                    _logger.WarnFormat(
                        "Error comunicacion con jugador {0} en sala {1}. Removiendo.",
                        par.Key,
                        idSala);
                    _logger.Warn(ex);
                    RemoverCallback(idSala, par.Key);
                }
                catch (TimeoutException ex)
                {
                    _logger.WarnFormat(
                        "Timeout con jugador {0} en sala {1}. Removiendo.",
                        par.Key,
                        idSala);
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

        private void RegistrarCallback(
            string idSala,
            string idJugador,
            ICursoPartidaManejadorCallback callback)
        {
            lock (_sincronizacion)
            {
                if (!_callbacksPorSala.TryGetValue(idSala, out var callbacks))
                {
                    callbacks = new Dictionary<string, ICursoPartidaManejadorCallback>(
                        StringComparer.OrdinalIgnoreCase);

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

                if (_partidasActivas.TryGetValue(idSala, out var controlador)
                    && !controlador.EstaFinalizada)
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
                return _salasManejador.ObtenerSalaPorCodigo(idSala)?.Configuracion;
            }
            catch (CommunicationException ex)
            {
                _logger.WarnFormat(
                    "No se pudo obtener configuracion de sala. Usará la sala por defecto.",
                    idSala);
                _logger.Warn(ex);
                return CrearConfiguracionPorDefecto();
            }
            catch (TimeoutException ex)
            {
                _logger.WarnFormat(
                    "No se pudo obtener configuracion de sala. Usará la sala por defecto.",
                    idSala);
                _logger.Warn(ex);
                return CrearConfiguracionPorDefecto();
            }
            catch (ObjectDisposedException ex)
            {
                _logger.WarnFormat(
                    "No se pudo obtener configuracion de sala. Usará la sala por defecto.",
                    idSala);
                _logger.Warn(ex);
                return CrearConfiguracionPorDefecto();
            }
        }

        private ConfiguracionPartidaDTO CrearConfiguracionPorDefecto()
        {
            return new ConfiguracionPartidaDTO
            {
                TiempoPorRondaSegundos = TiempoRondaPorDefectoSegundos,
                NumeroRondas = NumeroRondasPorDefecto,
                Dificultad = DificultadPorDefecto,
                IdiomaCanciones = "Espanol"
            };
        }
    }
}