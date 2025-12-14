using log4net;
using Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using System.Data;
using System;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Factoria para la creacion de contextos de base de datos.
    /// Centraliza la logica de creacion de instancias de contexto.
    /// </summary>
    public class ContextoFactoria : IContextoFactoria
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ContextoFactoria));

        /// <summary>
        /// Crea una nueva instancia del contexto de base de datos.
        /// </summary>
        /// <returns>Instancia del contexto de base de datos configurada.</returns>
        public BaseDatosPruebaEntities CrearContexto()
        {
            try
            {
                string conexion = Conexion.ObtenerConexion();

                if (string.IsNullOrWhiteSpace(conexion))
                {
                    _logger.Warn(
                        "La cadena de conexion obtenida esta vacia.");
                    return new BaseDatosPruebaEntities();
                }

                return new BaseDatosPruebaEntities(conexion);
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error al construir el contexto de base de datos.", excepcion);
                throw new DataException(
                    "No se pudo establecer la conexion con la base de datos.", excepcion);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado al crear el contexto de base de datos.", excepcion);
                throw new DataException(
                    "No se pudo establecer la conexion con la base de datos.", excepcion);
            }
        }
    }
}