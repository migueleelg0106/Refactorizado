namespace PictionaryMusicalServidor.Datos.DAL.Interfaces
{
    using PictionaryMusicalServidor.Datos.Modelo;

    /// <summary>
    /// Interfaz de repositorio para la gestion de usuarios en la capa de acceso a datos.
    /// Define operaciones de consulta y creacion de usuarios.
    /// </summary>
    public interface IUsuarioRepositorio
    {
        /// <summary>
        /// Verifica si existe un usuario con el nombre de usuario especificado.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a verificar.</param>
        /// <returns>True si el nombre de usuario ya existe.</returns>
        bool ExisteNombreUsuario(string nombreUsuario);

        /// <summary>
        /// Crea un nuevo usuario en la base de datos.
        /// </summary>
        /// <param name="usuario">Entidad de usuario a crear.</param>
        /// <returns>Usuario creado con su identificador asignado.</returns>
        Usuario CrearUsuario(Usuario usuario);

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a buscar.</param>
        /// <returns>Usuario encontrado o null si no existe.</returns>
        Usuario ObtenerPorNombreUsuario(string nombreUsuario);
    }
}
