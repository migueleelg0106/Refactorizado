using System.Collections.Generic;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Servicios.Notificadores
{
    /// <summary>
    /// Define las operaciones para notificar eventos relacionados con solicitudes de amistad.
    /// </summary>
    public interface INotificadorAmigos
    {
        /// <summary>
        /// Notifica que una solicitud de amistad ha sido actualizada para el usuario indicado.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario destino de la notificacion.</param>
        /// <param name="solicitud">Informacion de la solicitud de amistad actualizada.</param>
        void NotificarSolicitudActualizada(string nombreUsuario, SolicitudAmistadDTO solicitud);

        /// <summary>
        /// Notifica que una amistad ha sido eliminada para el usuario indicado.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario destino de la notificacion.</param>
        /// <param name="solicitud">Informacion de la solicitud asociada al evento de eliminacion.
        /// </param>
        void NotificarAmistadEliminada(string nombreUsuario, SolicitudAmistadDTO solicitud);

        /// <summary>
        /// Notifica las solicitudes de amistad pendientes al momento en que un usuario se 
        /// suscribe al servicio.
        /// </summary>
        /// <param name="nombreNormalizado">Nombre del usuario normalizado para consulta.</param>
        /// <param name="usuarioId">Identificador unico del usuario.</param>
        void NotificarSolicitudesPendientesAlSuscribir(string nombreNormalizado, int usuarioId);
    }

    /// <summary>
    /// Define las operaciones para notificar cambios en la lista de amigos de un usuario.
    /// </summary>
    public interface INotificadorListaAmigos
    {
        /// <summary>
        /// Notifica que se ha producido un cambio en la relacion de amistad del usuario indicado.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario afectado por el cambio.</param>
        void NotificarCambioAmistad(string nombreUsuario);

        /// <summary>
        /// Notifica la lista completa de amigos del usuario indicado.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario destino de la notificacion.</param>
        /// <param name="amigos">Lista de amigos que se notificara al usuario.</param>
        void NotificarLista(string nombreUsuario, List<AmigoDTO> amigos);
    }
}