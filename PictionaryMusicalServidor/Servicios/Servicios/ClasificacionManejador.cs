using Datos.Modelo;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Implementaciones;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Linq;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de gestion de clasificaciones de jugadores.
    /// Maneja la consulta de los mejores jugadores ordenados por puntuacion y rondas ganadas.
    /// </summary>
    public class ClasificacionManejador : IClasificacionManejador
    {
        private const int LimiteTopJugadores = 10;
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(ClasificacionManejador));
        private readonly IContextoFactoria _contextoFactory;

        public ClasificacionManejador() : this(new ContextoFactoria()) 
        { 
        }

        /// <summary>
        /// Constructor con inyeccion de dependencias.
        /// </summary>
        public ClasificacionManejador(IContextoFactoria contextoFactory)
        {
            _contextoFactory = contextoFactory
                ?? throw new ArgumentNullException(nameof(contextoFactory));
        }

        /// <summary>
        /// Obtiene la lista de los mejores jugadores ordenados por puntuacion.
        /// Retorna los 10 primeros jugadores ordenados por puntos ganados, rondas ganadas y nombre
        /// de usuario.
        /// </summary>
        /// <returns>Lista de clasificaciones de los mejores jugadores, o lista vacia si hay 
        /// errores.</returns>
        public IList<ClasificacionUsuarioDTO> ObtenerTopJugadores()
        {
            try
            {
                using (var contexto = _contextoFactory.CrearContexto())
                {
                    IClasificacionRepositorio repositorio =
                        new ClasificacionRepositorio(contexto);

                    IList<Usuario> usuarios = repositorio.ObtenerMejoresJugadores(
                        LimiteTopJugadores);

                    return usuarios.Select(u => new ClasificacionUsuarioDTO
                    {
                        Usuario = u.Nombre_Usuario,
                        Puntos = u.Jugador.Clasificacion.Puntos_Ganados ?? 0,
                        RondasGanadas = u.Jugador.Clasificacion.Rondas_Ganadas ?? 0
                    }).ToList();
                }
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos al obtener la clasificacion.", ex);
                return new List<ClasificacionUsuarioDTO>();
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al obtener la clasificacion.", ex);
                return new List<ClasificacionUsuarioDTO>();
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operacion invalida al obtener la clasificacion.", ex);
                return new List<ClasificacionUsuarioDTO>();
            }
        }
    }
}