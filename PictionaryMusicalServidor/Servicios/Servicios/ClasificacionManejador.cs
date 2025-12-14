using Datos.Modelo;
using log4net;
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
        private readonly IContextoFactoria _contextoFactoria;
        private readonly IRepositorioFactoria _repositorioFactoria;

        /// <summary>
        /// Constructor por defecto para uso en WCF.
        /// </summary>
        public ClasificacionManejador() : this(
            new ContextoFactoria(), 
            new RepositorioFactoria()) 
        { 
        }

        /// <summary>
        /// Constructor con inyeccion de dependencias para pruebas unitarias.
        /// </summary>
        /// <param name="contextoFactoria">Factoria para crear contextos de base de datos.</param>
        /// <param name="repositorioFactoria">Factoria para crear repositorios.</param>
        public ClasificacionManejador(
            IContextoFactoria contextoFactoria,
            IRepositorioFactoria repositorioFactoria)
        {
            _contextoFactoria = contextoFactoria
                ?? throw new ArgumentNullException(nameof(contextoFactoria));
            _repositorioFactoria = repositorioFactoria
                ?? throw new ArgumentNullException(nameof(repositorioFactoria));
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
                using (var contexto = _contextoFactoria.CrearContexto())
                {
                    IClasificacionRepositorio repositorio =
                        _repositorioFactoria.CrearClasificacionRepositorio(contexto);

                    IList<Usuario> usuarios = repositorio.ObtenerMejoresJugadores(
                        LimiteTopJugadores);

                    return usuarios.Select(usuario => new ClasificacionUsuarioDTO
                    {
                        Usuario = usuario.Nombre_Usuario,
                        Puntos = usuario.Jugador.Clasificacion.Puntos_Ganados ?? 0,
                        RondasGanadas = usuario.Jugador.Clasificacion.Rondas_Ganadas ?? 0
                    }).ToList();
                }
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error de base de datos al obtener la clasificacion.", excepcion);
                return new List<ClasificacionUsuarioDTO>();
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error de datos al obtener la clasificacion.", excepcion);
                return new List<ClasificacionUsuarioDTO>();
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Error("Operacion invalida al obtener la clasificacion.", excepcion);
                return new List<ClasificacionUsuarioDTO>();
            }
            catch (Exception excepcion)
            {
                _logger.Error("Operacion invalida al obtener la clasificacion.", excepcion);
                return new List<ClasificacionUsuarioDTO>();
            }
        }
    }
}
