using System.Collections.Generic;
using Datos.Modelo;

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

        /// <summary>
        /// Actualiza los puntos ganados y las rondas ganadas de la clasificacion asociada a un 
        /// jugador.
        /// </summary>
        /// <param name="jugadorId">Identificador del jugador propietario de la clasificacion.
        /// </param>
        /// <param name="puntosObtenidos">Puntos conseguidos en la partida.</param>
        /// <param name="ganoPartida">Indica si el jugador gano la partida.</param>
        /// <returns><c>true</c> si la actualizacion se aplico correctamente; de lo contrario, 
        /// <c>false</c>.</returns>
        bool ActualizarEstadisticas(int jugadorId, int puntosObtenidos, bool ganoPartida);

        /// <summary>
        /// Obtiene la lista de usuarios con sus clasificaciones ordenadas por puntuacion.
        /// </summary>
        /// <param name="cantidad">Numero maximo de registros a recuperar.</param>
        /// <returns>Lista de entidades de Usuario con datos cargados.</returns>
        IList<Usuario> ObtenerMejoresJugadores(int cantidad);
    }
}