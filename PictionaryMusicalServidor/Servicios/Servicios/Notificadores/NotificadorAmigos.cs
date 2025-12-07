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
    internal class NotificadorAmigos : INotificadorAmigos
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificadorAmigos));
        private readonly ManejadorCallback<IAmigosManejadorCallback> _manejadorCallback;
        private readonly IAmistadServicio _amistadServicio;

        public NotificadorAmigos(ManejadorCallback<IAmigosManejadorCallback> manejadorCallback, 
            IAmistadServicio amistadServicio)
        {
            _manejadorCallback = manejadorCallback;
            _amistadServicio = amistadServicio;
        }

        /// <summary>
        /// Notifica una solicitud de amistad actualizada a un usuario.
        /// </summary>
        /// <param name="nombreUsuario">Usuario a notificar.</param>
        /// <param name="solicitud">Detalles de la solicitud.</param>
        public void NotificarSolicitudActualizada(string nombreUsuario, 
            SolicitudAmistadDTO solicitud)
        {
            _manejadorCallback.Notificar(nombreUsuario, callback =>
            {
                callback.NotificarSolicitudActualizada(solicitud);
            });
        }

        /// <summary>
        /// Notifica la eliminacion de una amistad a un usuario.
        /// </summary>
        /// <param name="nombreUsuario">Usuario a notificar.</param>
        /// <param name="solicitud">Detalles de la relacion eliminada.</param>
        public void NotificarAmistadEliminada(string nombreUsuario, SolicitudAmistadDTO solicitud)
        {
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
        public void NotificarSolicitudesPendientesAlSuscribir(string nombreNormalizado, 
            int usuarioId)
        {
            try
            {
                List<SolicitudAmistadDTO> solicitudesDTO = 
                    _amistadServicio.ObtenerSolicitudesPendientesDTO(usuarioId);

                if (solicitudesDTO == null || solicitudesDTO.Count == 0)
                {
                    return;
                }

                foreach (var dto in solicitudesDTO)
                {
                    NotificarSolicitudActualizada(nombreNormalizado, dto);
                }
            }
            catch (DataException ex)
            {
                _logger.Error(
                    "Error de datos al recuperar las solicitudes pendientes de amistad.", ex);
            }
        }
    }
}