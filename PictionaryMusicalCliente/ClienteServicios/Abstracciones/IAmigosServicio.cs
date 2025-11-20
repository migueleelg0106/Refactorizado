using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de gestion de amistades.
    /// </summary>
    public interface IAmigosServicio : IDisposable
    {
        /// <summary>
        /// Evento que se dispara cuando las solicitudes de amistad se actualizan.
        /// </summary>
        event EventHandler<IReadOnlyCollection<DTOs.SolicitudAmistadDTO>> SolicitudesActualizadas;

        /// <summary>
        /// Obtiene la coleccion de solicitudes de amistad pendientes.
        /// </summary>
        IReadOnlyCollection<DTOs.SolicitudAmistadDTO> SolicitudesPendientes { get; }

        /// <summary>
        /// Suscribe al usuario para recibir notificaciones de solicitudes de amistad.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario a suscribir.</param>
        Task SuscribirAsync(string nombreUsuario);

        /// <summary>
        /// Cancela la suscripcion del usuario a notificaciones de amistad.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        Task CancelarSuscripcionAsync(string nombreUsuario);

        /// <summary>
        /// Envia una solicitud de amistad a otro usuario.
        /// </summary>
        /// <param name="nombreUsuarioEmisor">Nombre del usuario que envia la solicitud.</param>
        /// <param name="nombreUsuarioReceptor">Nombre del usuario que recibe la solicitud.</param>
        Task EnviarSolicitudAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor);

        /// <summary>
        /// Responde a una solicitud de amistad.
        /// </summary>
        /// <param name="nombreUsuarioEmisor">Nombre del usuario que envio la solicitud.</param>
        /// <param name="nombreUsuarioReceptor">Nombre del usuario que responde.</param>
        Task ResponderSolicitudAsync(string nombreUsuarioEmisor, string nombreUsuarioReceptor);

        /// <summary>
        /// Elimina la relacion de amistad entre dos usuarios.
        /// </summary>
        /// <param name="nombreUsuarioA">Nombre del primer usuario.</param>
        /// <param name="nombreUsuarioB">Nombre del segundo usuario.</param>
        Task EliminarAmigoAsync(string nombreUsuarioA, string nombreUsuarioB);
    }
}
