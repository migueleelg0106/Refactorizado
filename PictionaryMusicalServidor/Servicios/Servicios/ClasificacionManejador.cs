using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using log4net;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion de clasificaciones de jugadores.
    /// Maneja la consulta de los mejores jugadores ordenados por puntuacion y rondas ganadas.
    /// </summary>
    public class ClasificacionManejador : IClasificacionManejador
    {
        private const int LimiteTopJugadores = 10;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClasificacionManejador));

        /// <summary>
        /// Obtiene la lista de los mejores jugadores ordenados por puntuacion.
        /// Retorna los 10 primeros jugadores ordenados por puntos ganados, rondas ganadas y nombre de usuario.
        /// </summary>
        /// <returns>Lista de clasificaciones de los mejores jugadores, o lista vacia si hay errores.</returns>
        public IList<ClasificacionUsuarioDTO> ObtenerTopJugadores()
        {
            try
            {
                using (var contexto = CrearContexto())
                {
                    List<ClasificacionUsuarioDTO> jugadores = contexto.Usuario
                        .Include(u => u.Jugador.Clasificacion)
                        .Where(u => u.Jugador != null && u.Jugador.Clasificacion != null)
                        .Select(u => new ClasificacionUsuarioDTO
                        {
                            Usuario = u.Nombre_Usuario,
                            Puntos = u.Jugador.Clasificacion.Puntos_Ganados ?? 0,
                            RondasGanadas = u.Jugador.Clasificacion.Rondas_Ganadas ?? 0
                        })
                        .OrderByDescending(c => c.Puntos)
                        .ThenByDescending(c => c.RondasGanadas)
                        .ThenBy(c => c.Usuario)
                        .Take(LimiteTopJugadores)
                        .ToList();

                    return jugadores;
                }
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al obtener la clasificación. Fallo en la consulta de jugadores.", ex);
                return new List<ClasificacionUsuarioDTO>();
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al obtener la clasificación. Los datos de clasificación no se pudieron procesar.", ex);
                return new List<ClasificacionUsuarioDTO>();
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida al obtener la clasificación. Secuencia de operaciones incorrecta.", ex);
                return new List<ClasificacionUsuarioDTO>();
            }
        }

        private static BaseDatosPruebaEntities CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities()
                : new BaseDatosPruebaEntities(conexion);
        }
    }
}