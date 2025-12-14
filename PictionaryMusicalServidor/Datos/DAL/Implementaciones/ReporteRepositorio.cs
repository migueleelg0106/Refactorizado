using System;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    /// <summary>
    /// Repositorio encargado de gestionar la persistencia de reportes de jugadores.
    /// Permite verificar reportes duplicados y crear nuevos registros.
    /// </summary>
    public class ReporteRepositorio : IReporteRepositorio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ReporteRepositorio));
        private readonly BaseDatosPruebaEntities _contexto;

        /// <summary>
        /// Inicializa una nueva instancia del repositorio de reportes.
        /// </summary>
        /// <param name="contexto">Contexto de base de datos a utilizar.</param>
        public ReporteRepositorio(BaseDatosPruebaEntities contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        /// <summary>
        /// Verifica si ya existe un reporte del reportante hacia el usuario reportado.
        /// </summary>
        public bool ExisteReporte(int idReportante, int idReportado)
        {
            try
            {
                return _contexto.Reporte.Any(reporte => reporte.idReportante == idReportante
                    && reporte.idReportado == idReportado);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al verificar existencia del reporte entre {0} y {1}.",
                    idReportante,
                    idReportado,
                    excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al verificar existencia del reporte entre {0} y {1}.",
                    idReportante,
                    idReportado,
                    excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al verificar existencia del reporte entre {0} y {1}.",
                    idReportante,
                    idReportado,
                    excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat(
                    "Error al verificar existencia del reporte entre {0} y {1}.",
                    idReportante,
                    idReportado,
                    excepcion);
                throw;
            }
        }

        /// <summary>
        /// Almacena un nuevo reporte.
        /// </summary>
        /// <param name="reporte">Entidad con los datos del reporte a guardar.</param>
        /// <returns>Entidad almacenada con su identificador.</returns>
        public Reporte CrearReporte(Reporte reporte)
        {
            if (reporte == null)
            {
                var excepcion = new ArgumentNullException(nameof(reporte));
                _logger.Error("Se intento crear un reporte nulo.", excepcion);
                throw excepcion;
            }

            try
            {
                var entidad = _contexto.Reporte.Add(reporte);
                _contexto.SaveChanges();
                return entidad;
            }
            catch (DbUpdateException excepcion)
            {
                _logger.Error("Error al guardar el reporte en la base de datos.", excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error al guardar el reporte en la base de datos.", excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error al guardar el reporte en la base de datos.", excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error al guardar el reporte en la base de datos.", excepcion);
                throw;
            }
        }

        /// <summary>
        /// Cuenta la cantidad de reportes que ha recibido un usuario.
        /// </summary>
        /// <param name="idReportado">Identificador del usuario reportado.</param>
        /// <returns>Total de reportes registrados en su contra.</returns>
        public int ContarReportesRecibidos(int idReportado)
        {
            if (idReportado <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(idReportado),
                    "El identificador del usuario reportado debe ser mayor a cero.");
            }

            try
            {
                return _contexto.Reporte.Count(reporte => reporte.idReportado == idReportado);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat("Error al contar reportes del usuario {0}.", idReportado, excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat("Error al contar reportes del usuario {0}.", idReportado, excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat("Error al contar reportes del usuario {0}.", idReportado, excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat("Error al contar reportes del usuario {0}.", idReportado, excepcion);
                throw;
            }
        }
    }
}
