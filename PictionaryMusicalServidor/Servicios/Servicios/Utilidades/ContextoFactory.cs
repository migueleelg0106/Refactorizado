using log4net;
using Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Factoría para la creación de contextos de base de datos.
    /// Centraliza la lógica de creación de instancias de contexto.
    /// </summary>
    public class ContextoFactory : IContextoFactory
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ContextoFactory));

        /// <summary>
        /// Crea una nueva instancia del contexto de base de datos.
        /// </summary>
        /// <returns>Instancia del contexto de base de datos configurada.</returns>
        public BaseDatosPruebaEntities CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();

            if (string.IsNullOrWhiteSpace(conexion))
            {
                _logger.Warn("La cadena de conexión obtenida está vacía. Se intentará usar la configuración predeterminada (App.config).");
                return new BaseDatosPruebaEntities();
            }

            return new BaseDatosPruebaEntities(conexion);
        }
    }
}