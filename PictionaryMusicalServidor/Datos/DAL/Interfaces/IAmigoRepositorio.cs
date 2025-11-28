using System.Collections.Generic;
using Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Interfaces
{
    /// <summary>
    /// Interfaz de repositorio para la gestion de relaciones de amistad en la capa de acceso a datos.
    /// Define operaciones para crear, consultar, actualizar y eliminar amistades.
    /// </summary>
    public interface IAmigoRepositorio
    {
        /// <summary>
        /// Verifica si existe una relacion de amistad entre dos usuarios.
        /// </summary>
        /// <param name="usuarioAId">Identificador del primer usuario.</param>
        /// <param name="usuarioBId">Identificador del segundo usuario.</param>
        /// <returns>True si existe una relacion entre los usuarios.</returns>
        bool ExisteRelacion(int usuarioAId, int usuarioBId);

        /// <summary>
        /// Crea una solicitud de amistad entre dos usuarios.
        /// </summary>
        /// <param name="usuarioEmisorId">Identificador del usuario que envia la solicitud.</param>
        /// <param name="usuarioReceptorId">Identificador del usuario que recibe la solicitud.</param>
        /// <returns>Relacion de amistad creada.</returns>
        Amigo CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId);

        /// <summary>
        /// Obtiene la relacion de amistad entre dos usuarios.
        /// </summary>
        /// <param name="usuarioAId">Identificador del primer usuario.</param>
        /// <param name="usuarioBId">Identificador del segundo usuario.</param>
        /// <returns>Relacion de amistad encontrada o null si no existe.</returns>
        Amigo ObtenerRelacion(int usuarioAId, int usuarioBId);

        /// <summary>
        /// Obtiene todas las solicitudes de amistad pendientes de un usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <returns>Lista de solicitudes de amistad pendientes.</returns>
        IList<Amigo> ObtenerSolicitudesPendientes(int usuarioId);

        /// <summary>
        /// Actualiza el estado de una relacion de amistad (aceptada o pendiente).
        /// </summary>
        /// <param name="relacion">Relacion de amistad a actualizar.</param>
        /// <param name="estado">Nuevo estado de la relacion.</param>
        void ActualizarEstado(Amigo relacion, bool estado);

        /// <summary>
        /// Elimina una relacion de amistad entre dos usuarios.
        /// </summary>
        /// <param name="relacion">Relacion de amistad a eliminar.</param>
        void EliminarRelacion(Amigo relacion);

        /// <summary>
        /// Obtiene la lista de amigos confirmados de un usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <returns>Lista de usuarios amigos.</returns>
        IList<Usuario> ObtenerAmigos(int usuarioId);
    }
}
