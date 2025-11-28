using System;
using System.Data.Entity;
using System.Linq;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    public class ClasificacionRepositorio : IClasificacionRepositorio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClasificacionRepositorio));
        private readonly BaseDatosPruebaEntities _contexto;

        public ClasificacionRepositorio(BaseDatosPruebaEntities contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

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

                // No es estrictamente necesario un Info aquí si se llama siempre al crear usuario, 
                // pero ayuda a la trazabilidad si falla.

                return clasificacion;
            }
            catch (Exception ex)
            {
                _logger.Error("Error al crear la clasificación inicial.", ex);
                throw;
            }
        }

        public bool ActualizarEstadisticas(int jugadorId, int puntosObtenidos, bool ganoPartida)
        {
            try
            {
                var jugador = _contexto.Jugador
                    .Include(j => j.Clasificacion)
                    .FirstOrDefault(j => j.idJugador == jugadorId);

                if (jugador?.Clasificacion == null)
                {
                    _logger.WarnFormat("No se encontró clasificación para el jugador con ID {0}.", jugadorId);
                    return false;
                }

                jugador.Clasificacion.Puntos_Ganados = (jugador.Clasificacion.Puntos_Ganados ?? 0) + puntosObtenidos;

                if (ganoPartida)
                {
                    jugador.Clasificacion.Rondas_Ganadas = (jugador.Clasificacion.Rondas_Ganadas ?? 0) + 1;
                }

                _contexto.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error al actualizar la clasificación del jugador con ID {0}.", jugadorId), ex);
                throw;
            }
        }
    }
}