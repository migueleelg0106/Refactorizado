using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    /// <summary>
    /// Repositorio encargado de gestionar la persistencia y actualizacion de las clasificaciones
    /// y estadisticas de juego de los usuarios.
    /// </summary>
    public class ClasificacionRepositorio : IClasificacionRepositorio
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(ClasificacionRepositorio));

        private readonly BaseDatosPruebaEntities _contexto;

        /// <summary>
        /// Inicializa una nueva instancia del repositorio de clasificaciones.
        /// </summary>
        /// <param name="contexto">Contexto de la base de datos.</param>
        /// <exception cref="ArgumentNullException">Se lanza si el contexto es nulo.</exception>
        public ClasificacionRepositorio(BaseDatosPruebaEntities contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        /// <summary>
        /// Crea un registro de clasificacion inicial con contadores en cero para un nuevo jugador.
        /// </summary>
        public Clasificacion CrearClasificacionInicial()
        {
            try
            {
                var clasificacion = new Clasificacion
                {
                    Puntos_Ganados = 0,
                    Rondas_Ganadas = 0
                };

                _contexto.Clasificacion.Add(clasificacion);
                _contexto.SaveChanges();

                return clasificacion;
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error al crear la clasificacion inicial.", ex);
                throw;
            }
            catch (EntityException ex)
            {
                _logger.Error("Error al crear la clasificacion inicial.", ex);
                throw;
            }
            catch (DataException ex)
            {
                _logger.Error("Error al crear la clasificacion inicial.", ex);
                throw;
            }
        }

        /// <summary>
        /// Actualiza las estadisticas de puntos y partidas ganadas de un jugador especifico.
        /// </summary>
        public bool ActualizarEstadisticas(
            int jugadorId,
            int puntosObtenidos,
            bool ganoPartida)
        {
            try
            {
                var jugador = _contexto.Jugador
                    .Include(j => j.Clasificacion)
                    .FirstOrDefault(j => j.idJugador == jugadorId);

                if (jugador?.Clasificacion == null)
                {
                    _logger.WarnFormat(
                        "No se encontro clasificacion para el jugador con ID {0}.",
                        jugadorId);
                    return false;
                }

                jugador.Clasificacion.Puntos_Ganados =
                    (jugador.Clasificacion.Puntos_Ganados ?? 0) + puntosObtenidos;

                if (ganoPartida)
                {
                    jugador.Clasificacion.Rondas_Ganadas =
                        (jugador.Clasificacion.Rondas_Ganadas ?? 0) + 1;
                }

                _contexto.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.ErrorFormat(
                    "Error al actualizar la clasificacion del jugador con ID {0}.",
                    jugadorId,
                    ex);
                throw;
            }
            catch (EntityException ex)
            {
                _logger.ErrorFormat(
                    "Error al actualizar la clasificacion del jugador con ID {0}.",
                    jugadorId,
                    ex);
                throw;
            }
            catch (DataException ex)
            {
                _logger.ErrorFormat(
                    "Error al actualizar la clasificacion del jugador con ID {0}.",
                    jugadorId,
                    ex);
                throw;
            }
        }

        /// <summary>
        /// Obtiene la lista de usuarios con sus clasificaciones ordenadas por puntuacion.
        /// </summary>
        public IList<Usuario> ObtenerMejoresJugadores(int cantidad)
        {
            try
            {
                return _contexto.Usuario
                    .Include(u => u.Jugador.Clasificacion)
                    .Where(u => u.Jugador != null && u.Jugador.Clasificacion != null)
                    .OrderByDescending(u => u.Jugador.Clasificacion.Puntos_Ganados)
                    .ThenByDescending(u => u.Jugador.Clasificacion.Rondas_Ganadas)
                    .ThenBy(u => u.Nombre_Usuario)
                    .Take(cantidad)
                    .ToList();
            }
            catch (DbUpdateException ex)
            {
                _logger.Error("Error al consultar los mejores jugadores.", ex);
                throw;
            }
            catch (EntityException ex)
            {
                _logger.Error("Error al consultar los mejores jugadores.", ex);
                throw;
            }
            catch (DataException ex)
            {
                _logger.Error("Error al consultar los mejores jugadores.", ex);
                throw;
            }
        }
    }
}