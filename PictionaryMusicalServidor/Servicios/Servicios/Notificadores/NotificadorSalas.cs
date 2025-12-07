using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;

namespace PictionaryMusicalServidor.Servicios.Servicios.Notificadores
{
    /// <summary>
    /// Servicio especializado en notificaciones de cambios en salas.
    /// Gestiona las suscripciones y notificaciones de listas de salas.
    /// </summary>
    internal class NotificadorSalas : INotificadorSalas
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificadorSalas));
        private readonly ConcurrentDictionary<Guid, ISalasManejadorCallback> _suscripciones =
            new ConcurrentDictionary<Guid, ISalasManejadorCallback>();
        private readonly Func<IEnumerable<SalaInternaManejador>> _obtenerSalas;

        public NotificadorSalas(Func<IEnumerable<SalaInternaManejador>> obtenerSalas)
        {
            _obtenerSalas = obtenerSalas;
        }

        /// <summary>
        /// Suscribe un callback a las notificaciones de la lista de salas.
        /// </summary>
        /// <param name="callback">Callback a suscribir.</param>
        /// <returns>ID de la suscripcion.</returns>
        public Guid Suscribir(ISalasManejadorCallback callback)
        {
            var sesionId = Guid.NewGuid();
            _suscripciones.AddOrUpdate(sesionId, callback, (_, __) => callback);

            return sesionId;
        }

        /// <summary>
        /// Elimina una suscripcion especifica.
        /// </summary>
        /// <param name="sesionId">ID de la suscripcion a eliminar.</param>
        public void Desuscribir(Guid sesionId)
        {
            _suscripciones.TryRemove(sesionId, out _);
        }

        /// <summary>
        /// Elimina todas las suscripciones asociadas a un callback especifico.
        /// </summary>
        /// <param name="callback">Callback a desuscribir.</param>
        public void DesuscribirPorCallback(ISalasManejadorCallback callback)
        {
            var keysToRemove = _suscripciones
                .Where(kvp => ReferenceEquals(kvp.Value, callback))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _suscripciones.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Notifica la lista actualizada de salas a un callback especifico.
        /// </summary>
        /// <param name="callback">Callback a notificar.</param>
        public void NotificarListaSalas(ISalasManejadorCallback callback)
        {
            try
            {
                var salas = _obtenerSalas().Select(s => s.ToDto()).ToArray();
                callback.NotificarListaSalasActualizada(salas);
            }
            catch (CommunicationException ex)
            {
                _logger.Warn(
                    "Error de comunicacion al notificar la lista de salas a los suscriptores.", 
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Warn("Timeout al notificar la lista de salas a los suscriptores.", ex);
            }
            catch (ObjectDisposedException ex)
            {
                _logger.Error(
                    "Error inesperado al notificar la lista de salas a los suscriptores.", ex);
            }
        }

        /// <summary>
        /// Notifica la lista actualizada de salas a todos los suscriptores.
        /// </summary>
        public void NotificarListaSalasATodos()
        {
            var salas = _obtenerSalas().Select(s => s.ToDto()).ToArray();

            foreach (var kvp in _suscripciones)
            {
                try
                {
                    kvp.Value.NotificarListaSalasActualizada(salas);
                }
                catch (CommunicationException ex)
                {
                    _logger.Warn(
                        "Error de comunicacion al notificar masivamente. Eliminando suscripcion defectuosa.", 
                        ex);
                    _suscripciones.TryRemove(kvp.Key, out _);
                }
                catch (TimeoutException ex)
                {
                    _logger.Warn(
                        "Error de comunicacion al notificar masivamente. Eliminando suscripcion defectuosa.",
                        ex);
                        _suscripciones.TryRemove(kvp.Key, out _);
                }
                catch (ObjectDisposedException ex)
                {
                    _logger.Error(
                        "Error inesperado al notificar la lista de salas a los suscriptores.", ex);
                }
            }
        }
    }
}