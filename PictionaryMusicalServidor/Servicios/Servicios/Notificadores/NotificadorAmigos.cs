using System.Collections.Generic;
using System.Data;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Servicios.Servicios.Notificadores
{
    /// <summary>
    /// Servicio especializado en notificaciones de eventos de amistad.
    /// Gestiona las notificaciones de solicitudes y eliminaciones de amistad.
    /// </summary>
    internal class NotificadorAmigos
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificadorAmigos));
        private readonly ManejadorCallback<IAmigosManejadorCallback> _manejadorCallback;
        private readonly IAmistadServicio _amistadServicio;

        public NotificadorAmigos(ManejadorCallback<IAmigosManejadorCallback> manejadorCallback)
            : this(manejadorCallback, new AmistadServicio(new ContextoFactory()))
        {
        }

        public NotificadorAmigos(ManejadorCallback<IAmigosManejadorCallback> manejadorCallback, IAmistadServicio amistadServicio)
        {
            _manejadorCallback = manejadorCallback;
            _amistadServicio = amistadServicio;
        }

        /// <summary>
        /// Notifica una solicitud de amistad actualizada a un usuario.
        /// </summary>
        /// <param name="nombreUsuario">Usuario a notificar.</param>
        /// <param name="solicitud">Detalles de la solicitud.</param>
        public void NotificarSolicitudActualizada(string nombreUsuario, SolicitudAmistadDTO solicitud)
        {
            _logger.InfoFormat("Notificando solicitud de amistad (Emisor: {0}) a '{1}'. Estado aceptada: {2}", solicitud.UsuarioEmisor, nombreUsuario, solicitud.SolicitudAceptada);
            _manejadorCallback.Notificar(nombreUsuario, callback =>
            {
                callback.NotificarSolicitudActualizada(solicitud);
            });
        }

        /// <summary>
        /// Notifica la eliminación de una amistad a un usuario.
        /// </summary>
        /// <param name="nombreUsuario">Usuario a notificar.</param>
        /// <param name="solicitud">Detalles de la relación eliminada.</param>
        public void NotificarAmistadEliminada(string nombreUsuario, SolicitudAmistadDTO solicitud)
        {
            _logger.InfoFormat("Notificando eliminación de amistad a '{0}' con {1}/{2}.", nombreUsuario, solicitud.UsuarioEmisor, solicitud.UsuarioReceptor);
            _manejadorCallback.Notificar(nombreUsuario, callback =>
            {
                callback.NotificarAmistadEliminada(solicitud);
            });
        }

        /// <summary>
        /// Notifica todas las solicitudes pendientes al suscribirse un usuario.
        /// </summary>
        /// <param name="nombreNormalizado">Nombre normalizado del usuario.</param>
        /// <param name="usuarioId">ID del usuario.</param>
        public void NotificarSolicitudesPendientesAlSuscribir(string nombreNormalizado, int usuarioId)
        {
            try
            {
                List<SolicitudAmistadDTO> solicitudesDTO = _amistadServicio.ObtenerSolicitudesPendientesDTO(usuarioId);

                if (solicitudesDTO == null || solicitudesDTO.Count == 0)
                {
                    return;
                }

                _logger.InfoFormat("Usuario '{0}' tiene {1} solicitudes pendientes. Enviando notificaciones.", nombreNormalizado, solicitudesDTO.Count);

                foreach (var dto in solicitudesDTO)
                {
                    NotificarSolicitudActualizada(nombreNormalizado, dto);
                }
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos al recuperar las solicitudes pendientes de amistad.", ex);
            }
        }
    }
}