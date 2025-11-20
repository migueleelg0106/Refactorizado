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

        public NotificadorAmigos(ManejadorCallback<IAmigosManejadorCallback> manejadorCallback)
        {
            _manejadorCallback = manejadorCallback;
        }

        /// <summary>
        /// Notifica una solicitud de amistad actualizada a un usuario.
        /// </summary>
        /// <param name="nombreUsuario">Usuario a notificar.</param>
        /// <param name="solicitud">Detalles de la solicitud.</param>
        public void NotificarSolicitudActualizada(string nombreUsuario, SolicitudAmistadDTO solicitud)
        {
            _logger.Info($"Notificando solicitud de amistad (Emisor: {solicitud.UsuarioEmisor}) a '{nombreUsuario}'. Estado aceptada: {solicitud.SolicitudAceptada}");
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
            _logger.Info($"Notificando eliminación de amistad a '{nombreUsuario}' con {solicitud.UsuarioEmisor}/{solicitud.UsuarioReceptor}.");
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
                List<SolicitudAmistadDTO> solicitudesDTO = ServicioAmistad.ObtenerSolicitudesPendientesDTO(usuarioId);

                if (solicitudesDTO == null || solicitudesDTO.Count == 0)
                {
                    return;
                }

                _logger.Info($"Usuario '{nombreNormalizado}' tiene {solicitudesDTO.Count} solicitudes pendientes. Enviando notificaciones.");

                foreach (var dto in solicitudesDTO)
                {
                    NotificarSolicitudActualizada(nombreNormalizado, dto);
                }
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.AmistadSolicitudesPendientesErrorDatos, ex);
            }
        }
    }
}