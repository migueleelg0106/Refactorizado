using PictionaryMusicalServidor.Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Interfaces
{
    /// <summary>
    /// Interfaz de repositorio para la gestion de clasificaciones en la capa de acceso a datos.
    /// Define operaciones para crear clasificaciones de jugadores.
    /// </summary>
    public interface IClasificacionRepositorio
    {
        /// <summary>
        /// Crea una clasificacion inicial con valores predeterminados para un nuevo jugador.
        /// </summary>
        /// <returns>Clasificacion creada con su identificador asignado.</returns>
        Clasificacion CrearClasificacionInicial();
    }
}
