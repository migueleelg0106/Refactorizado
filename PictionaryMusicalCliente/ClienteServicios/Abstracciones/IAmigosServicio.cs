using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Gestiona las operaciones relacionadas con solicitudes de amistad y 
    /// eliminacion de contactos.
    /// </summary>
    public interface IAmigosServicio : IDisposable
    {
        /// <summary>
        /// Se dispara cuando llegan nuevas solicitudes de amistad o se aceptan/rechazan.
        /// </summary>
        event EventHandler<IReadOnlyCollection<DTOs.SolicitudAmistadDTO>> SolicitudesActualizadas;

        /// <summary>
        /// Coleccion de solicitudes pendientes de responder en cache local.
        /// </summary>
        IReadOnlyCollection<DTOs.SolicitudAmistadDTO> SolicitudesPendientes { get; }

        /// <summary>
        /// Inicia la escucha de eventos de amistad para el usuario conectado.
        /// </summary>
        /// <param name="nombreUsuario">Usuario local.</param>
        Task SuscribirAsync(string nombreUsuario);

        /// <summary>
        /// Detiene la escucha de eventos de amistad.
        /// </summary>
        /// <param name="nombreUsuario">Usuario local.</param>
        Task CancelarSuscripcionAsync(string nombreUsuario);

        /// <summary>
        /// Genera una peticion de amistad dirigida a otro usuario.
        /// </summary>
        /// <param name="nombreUsuarioEmisor">Quien envia la solicitud.</param>
        /// <param name="nombreUsuarioReceptor">Quien recibe la solicitud.</param>
        Task EnviarSolicitudAsync(
            string nombreUsuarioEmisor,
            string nombreUsuarioReceptor);

        /// <summary>
        /// Procesa la aceptacion o rechazo de una solicitud existente.
        /// </summary>
        /// <param name="nombreUsuarioEmisor">Quien envio originalmente la solicitud.</param>
        /// <param name="nombreUsuarioReceptor">Quien esta respondiendo la solicitud.</param>
        Task ResponderSolicitudAsync(
            string nombreUsuarioEmisor,
            string nombreUsuarioReceptor);

        /// <summary>
        /// Rompe el vinculo de amistad entre dos usuarios.
        /// </summary>
        /// <param name="nombreUsuarioA">Primer usuario implicado.</param>
        /// <param name="nombreUsuarioB">Segundo usuario implicado.</param>
        Task EliminarAmigoAsync(string nombreUsuarioA, string nombreUsuarioB);
    }
}