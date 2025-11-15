using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using log4net;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    public class ClasificacionManejador : IClasificacionManejador
    {
        private const int LimiteTopJugadores = 10;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClasificacionManejador));

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
                _logger.Error(MensajesError.Log.ClasificacionErrorBD, ex);
                return new List<ClasificacionUsuarioDTO>();
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.ClasificacionErrorDatos, ex);
                return new List<ClasificacionUsuarioDTO>();
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.ClasificacionOperacionInvalida, ex);
                return new List<ClasificacionUsuarioDTO>();
            }
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }
    }
}