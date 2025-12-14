using System;
using System.Collections.Generic;
using System.Linq;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.LogicaNegocio
{
    /// <summary>
    /// Gestiona la coleccion de jugadores, los turnos de dibujo y el calculo de puntuaciones 
    /// dentro de una partida.
    /// </summary>
    public class GestorJugadoresPartida
    {
        private readonly Dictionary<string, JugadorPartida> _jugadores;
        private readonly Queue<string> _colaDibujantes;
        private readonly Random _random;

        /// <summary>
        /// Inicializa una nueva instancia del gestor de jugadores.
        /// </summary>
        public GestorJugadoresPartida()
        {
            _jugadores = new Dictionary<string, JugadorPartida>(StringComparer.Ordinal);
            _colaDibujantes = new Queue<string>();
            _random = new Random();
        }

        /// <summary>
        /// Indica si existe la cantidad minima necesaria de jugadores para jugar (al menos 2).
        /// </summary>
        public bool HaySuficientesJugadores
        {
            get { return _jugadores.Count >= 2; }
        }

        /// <summary>
        /// Agrega un jugador a la coleccion o actualiza sus datos si ya existe.
        /// </summary>
        /// <param name="idConexion">Identificador de conexion del jugador.</param>
        /// <param name="nombre">Nombre de usuario visible.</param>
        /// <param name="esHost">Indica si el jugador es el anfitrion de la partida.</param>
        public void Agregar(string idConexion, string nombre, bool esHost)
        {
            if (_jugadores.ContainsKey(idConexion))
            {
                var existente = _jugadores[idConexion];
                existente.NombreUsuario = nombre;
                existente.EsHost = esHost;
                return;
            }

            _jugadores.Add(idConexion, new JugadorPartida
            {
                IdConexion = idConexion,
                NombreUsuario = nombre,
                EsHost = esHost,
                PuntajeTotal = 0
            });
        }

        /// <summary>
        /// Obtiene la informacion de un jugador especifico por su ID de conexion.
        /// </summary>
        /// <param name="idConexion">Identificador de conexion a buscar.</param>
        /// <returns>La instancia del jugador o null si no se encuentra.</returns>
        public JugadorPartida Obtener(string idConexion)
        {
            _jugadores.TryGetValue(idConexion, out var jugador);
            return jugador;
        }

        /// <summary>
        /// Elimina a un jugador de la partida y reorganiza la cola de turnos si es necesario.
        /// </summary>
        /// <param name="idConexion">Identificador del jugador a eliminar.</param>
        /// <param name="eraDibujante">Parametro de salida que indica si el jugador eliminado 
        /// tenia el turno de dibujo activo.</param>
        /// <returns>True si el jugador fue eliminado correctamente, False si no existia.</returns>
        public bool Remover(string idConexion, out bool eraDibujante)
        {
            eraDibujante = false;
            if (_jugadores.TryGetValue(idConexion, out var jugador))
            {
                eraDibujante = jugador.EsDibujante;
                _jugadores.Remove(idConexion);
                ReconstruirColaDibujantes(idConexion);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Verifica si un jugador especifico es el anfitrion de la partida.
        /// </summary>
        /// <param name="idConexion">Identificador de conexion a verificar.</param>
        /// <returns>True si el jugador es Host, False en caso contrario.</returns>
        public bool EsHost(string idConexion)
        {
            return _jugadores.TryGetValue(idConexion, out var jugador) && jugador.EsHost;
        }

        /// <summary>
        /// Inicializa y aleatoriza la cola de turnos para los dibujantes al inicio de 
        /// una ronda general.
        /// </summary>
        public void PrepararColaDibujantes()
        {
            _colaDibujantes.Clear();
            var idsAleatorios = _jugadores.Keys.OrderBy(idJugador => _random.Next()).ToList();

            foreach (var id in idsAleatorios)
            {
                _colaDibujantes.Enqueue(id);
            }
        }

        /// <summary>
        /// Selecciona al siguiente dibujante de la cola y actualiza los estados de los jugadores 
        /// para el nuevo turno.
        /// </summary>
        /// <returns>True si se pudo seleccionar un dibujante, False si la cola esta vacia.
        /// </returns>
        public bool SeleccionarSiguienteDibujante()
        {
            ReiniciarEstadoRondaJugadores();

            while (_colaDibujantes.Count > 0)
            {
                var id = _colaDibujantes.Dequeue();
                if (_jugadores.TryGetValue(id, out var jugador))
                {
                    jugador.EsDibujante = true;
                    jugador.YaAdivino = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Verifica si todos los jugadores (excepto el dibujante) han acertado la palabra.
        /// </summary>
        /// <returns>True si todos han adivinado, False si falta alguno.</returns>
        public bool TodosAdivinaron()
        {
            var adivinadores = _jugadores.Values.Where(jugador => !jugador.EsDibujante).ToList();
            return adivinadores.Count > 0 && adivinadores.All(jugador => jugador.YaAdivino);
        }

        /// <summary>
        /// Genera la lista de clasificacion final ordenada por puntaje descendente.
        /// </summary>
        /// <returns>Lista de objetos DTO con la clasificacion.</returns>
        public List<ClasificacionUsuarioDTO> GenerarClasificacion()
        {
            return _jugadores.Values
                .Select(jugador => new ClasificacionUsuarioDTO
                {
                    Usuario = jugador.NombreUsuario,
                    Puntos = jugador.PuntajeTotal
                })
                .OrderByDescending(jugador => jugador.Puntos)
                .ToList();
        }

        /// <summary>
        /// Obtiene una copia de seguridad de la lista actual de jugadores para evitar 
        /// modificaciones externas no controladas.
        /// </summary>
        /// <returns>Coleccion de jugadores.</returns>
        public IReadOnlyCollection<JugadorPartida> ObtenerCopiaLista()
        {
            return _jugadores.Values.Select(jugador => jugador.CopiarDatosBasicos()).ToList();
        }

        /// <summary>
        /// Verifica si aun quedan jugadores pendientes por dibujar en la ronda actual.
        /// </summary>
        /// <returns>True si hay dibujantes en cola, False si esta vacia.</returns>
        public bool QuedanDibujantesPendientes()
        {
            return _colaDibujantes.Count > 0;
        }

        private void ReiniciarEstadoRondaJugadores()
        {
            foreach (var jugador in _jugadores.Values)
            {
                jugador.EsDibujante = false;
                jugador.YaAdivino = false;
            }
        }

        private void ReconstruirColaDibujantes(string idExcluido)
        {
            if (_colaDibujantes.Count == 0)
            {
                return;
            }

            var lista = _colaDibujantes
                .Where(id => id != idExcluido && _jugadores.ContainsKey(id))
                .ToList();

            _colaDibujantes.Clear();

            foreach (var id in lista)
            {
                _colaDibujantes.Enqueue(id);
            }
        }
    }
}