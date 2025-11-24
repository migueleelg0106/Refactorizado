using PictionaryMusicalServidor.Datos.Modelo;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Interfaz para la creación de contextos de base de datos.
    /// Permite la inyección de dependencias para facilitar las pruebas unitarias.
    /// </summary>
    public interface IContextoFactory
    {
        /// <summary>
        /// Crea una nueva instancia del contexto de base de datos.
        /// </summary>
        /// <returns>Instancia del contexto de base de datos configurada.</returns>
        BaseDatosPruebaEntities1 CrearContexto();
    }
}
