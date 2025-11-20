using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Modelos;

namespace PictionaryMusicalServidor.Servicios.Servicios.Notificadores
{
    /// <summary>
    /// Servicio especializado en notificaciones de cambios en salas.
    /// Gestiona las suscripciones y notificaciones de listas de salas.
    /// </summary>
    internal class NotificadorSalas
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NotificadorSalas));
        private readonly ConcurrentDictionary<Guid, ISalasCallback> _suscripciones = new();
        private readonly Func<IEnumerable<SalaInterna>> _obtenerSalas;

        public NotificadorSalas(Func<IEnumerable<SalaInterna>> obtenerSalas)
        {
            _obtenerSalas = obtenerSalas;
        }

        /// <summary>
        /// Suscribe un callback a las notificaciones de la lista de salas.
        /// </summary>
        /// <param name="callback">Callback a suscribir.</param>
        /// <returns>ID de la suscripción.</returns>
        public Guid Suscribir(ISalasCallback callback)
        {
            var sesionId = Guid.NewGuid();
            _suscripciones.AddOrUpdate(sesionId, callback, (_, __) => callback);
            _logger.Info($"Nueva suscripción a lista de salas. Sesión ID: {sesionId}");
            return sesionId;
        }

        /// <summary>
        /// Elimina una suscripción específica.
        /// </summary>
        /// <param name="sesionId">ID de la suscripción a eliminar.</param>
        public void Desuscribir(Guid sesionId)
        {
            if (_suscripciones.TryRemove(sesionId, out _))
            {
                _logger.Info($"Suscripción a lista de salas eliminada. Sesión ID: {sesionId}");
            }
        }

        /// <summary>
        /// Elimina todas las suscripciones asociadas a un callback específico.
        /// </summary>
        /// <param name="callback">Callback a desuscribir.</param>
        public void DesuscribirPorCallback(ISalasCallback callback)
        {
            var keysToRemove = _suscripciones
                .Where(kvp => ReferenceEquals(kvp.Value, callback))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _suscripciones.TryRemove(key, out _);
            }

            if (keysToRemove.Count > 0)
            {
                _logger.Info($"Se eliminaron {keysToRemove.Count} suscripciones por limpieza de callback.");
            }
        }

        /// <summary>
        /// Notifica la lista actualizada de salas a un callback específico.
        /// </summary>
        /// <param name="callback">Callback a notificar.</param>
        public void NotificarListaSalas(ISalasCallback callback)
        {
            try
            {
                var salas = _obtenerSalas().Select(s => s.ToDto()).ToArray();
                callback.NotificarListaSalasActualizada(salas);
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                _logger.Warn(MensajesError.Log.SalaNotificarListaComunicacion, ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Warn(MensajesError.Log.SalaNotificarListaTimeout, ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.ComunicacionOperacionInvalida, ex);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaNotificarListaErrorGeneral, ex);
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
                catch (System.ServiceModel.CommunicationException ex)
                {
                    _logger.Warn($"{MensajesError.Log.SalaNotificarListaComunicacion} (Sesión: {kvp.Key})", ex);
                    _suscripciones.TryRemove(kvp.Key, out _);
                }
                catch (TimeoutException ex)
                {
                    _logger.Warn($"{MensajesError.Log.SalaNotificarListaTimeout} (Sesión: {kvp.Key})", ex);
                    _suscripciones.TryRemove(kvp.Key, out _);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.Warn(MensajesError.Log.ComunicacionOperacionInvalida, ex);
                }
                catch (Exception ex)
                {
                    _logger.Error(MensajesError.Log.SalaNotificarListaErrorGeneral, ex);
                }
            }
        }
    }
}