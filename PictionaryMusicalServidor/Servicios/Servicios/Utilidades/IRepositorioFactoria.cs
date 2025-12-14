using Datos.Modelo;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Interfaz para la factoría de creación de repositorios.
    /// Permite abstraer la creación de repositorios para facilitar pruebas unitarias
    /// y mantener una única responsabilidad en la gestión del contexto.
    /// </summary>
    public interface IRepositorioFactoria
    {
        /// <summary>
        /// Crea una instancia del repositorio de usuarios.
        /// </summary>
        /// <param name="contexto">Contexto de base de datos a utilizar.</param>
        /// <returns>Instancia del repositorio de usuarios.</returns>
        IUsuarioRepositorio CrearUsuarioRepositorio(BaseDatosPruebaEntities contexto);

        /// <summary>
        /// Crea una instancia del repositorio de amigos.
        /// </summary>
        /// <param name="contexto">Contexto de base de datos a utilizar.</param>
        /// <returns>Instancia del repositorio de amigos.</returns>
        IAmigoRepositorio CrearAmigoRepositorio(BaseDatosPruebaEntities contexto);

        /// <summary>
        /// Crea una instancia del repositorio de clasificaciones.
        /// </summary>
        /// <param name="contexto">Contexto de base de datos a utilizar.</param>
        /// <returns>Instancia del repositorio de clasificaciones.</returns>
        IClasificacionRepositorio CrearClasificacionRepositorio(BaseDatosPruebaEntities contexto);

        /// <summary>
        /// Crea una instancia del repositorio de reportes.
        /// </summary>
        /// <param name="contexto">Contexto de base de datos a utilizar.</param>
        /// <returns>Instancia del repositorio de reportes.</returns>
        IReporteRepositorio CrearReporteRepositorio(BaseDatosPruebaEntities contexto);

        /// <summary>
        /// Crea una instancia del repositorio de jugadores.
        /// </summary>
        /// <param name="contexto">Contexto de base de datos a utilizar.</param>
        /// <returns>Instancia del repositorio de jugadores.</returns>
        IJugadorRepositorio CrearJugadorRepositorio(BaseDatosPruebaEntities contexto);
    }
}
