using log4net;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Factoría para la creación de contextos de base de datos.
    /// Centraliza la lógica de creación de instancias de contexto.
    /// </summary>
    internal static class ContextoFactory
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ContextoFactory));

        /// <summary>
        /// Crea una nueva instancia del contexto de base de datos.
        /// </summary>
        /// <returns>Instancia del contexto de base de datos configurada.</returns>
        public static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();

            if (string.IsNullOrWhiteSpace(conexion))
            {
                _logger.Warn("La cadena de conexión obtenida está vacía. Se intentará usar la configuración predeterminada (App.config).");
                return new BaseDatosPruebaEntities1();
            }

            return new BaseDatosPruebaEntities1(conexion);
        }
    }
}