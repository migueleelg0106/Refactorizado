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
        /// <summary>
        /// Crea una nueva instancia del contexto de base de datos.
        /// </summary>
        /// <returns>Instancia del contexto de base de datos configurada.</returns>
        public static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }
    }
}
