using Datos.Modelo;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Interfaz para la factoría de creación de contextos de base de datos.
    /// Permite inyectar y mockear la creación de contextos en pruebas unitarias.
    /// </summary>
    public interface IContextoFactory
    {
        /// <summary>
        /// Crea una nueva instancia del contexto de base de datos.
        /// </summary>
        /// <returns>Instancia del contexto de base de datos configurada.</returns>
        BaseDatosPruebaEntities CrearContexto();
    }
}
