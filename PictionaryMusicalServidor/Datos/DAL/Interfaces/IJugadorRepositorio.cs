using PictionaryMusicalServidor.Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Interfaces
{
    /// <summary>
    /// Interfaz de repositorio para la gestion de jugadores en la capa de acceso a datos.
    /// Define operaciones de consulta y creacion de jugadores.
    /// </summary>
    public interface IJugadorRepositorio
    {
        /// <summary>
        /// Verifica si existe un jugador con el correo electronico especificado.
        /// </summary>
        /// <param name="correo">Correo electronico a verificar.</param>
        /// <returns>True si el correo ya esta registrado.</returns>
        bool ExisteCorreo(string correo);

        /// <summary>
        /// Crea un nuevo jugador en la base de datos.
        /// </summary>
        /// <param name="jugador">Entidad de jugador a crear.</param>
        /// <returns>Jugador creado con su identificador asignado.</returns>
        Jugador CrearJugador(Jugador jugador);
    }
}
