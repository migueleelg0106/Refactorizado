using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Manejador genérico para callbacks de servicios WCF.
    /// Gestiona el ciclo de vida de las suscripciones de callbacks.
    /// </summary>
    /// <typeparam name="TCallback">Tipo de callback a gestionar.</typeparam>
    internal class ManejadorCallback<TCallback> where TCallback : class
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ManejadorCallback<TCallback>));
        private readonly ConcurrentDictionary<string, TCallback> _suscripciones;

        public ManejadorCallback(StringComparer comparer = null)
        {
            _suscripciones = comparer != null 
                ? new ConcurrentDictionary<string, TCallback>(comparer)
                : new ConcurrentDictionary<string, TCallback>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registra una suscripción de callback para un usuario.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        /// <param name="callback">Callback a registrar.</param>
        public void Suscribir(string nombreUsuario, TCallback callback)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || callback == null)
            {
                return;
            }

            _suscripciones.AddOrUpdate(nombreUsuario, callback, (_, __) => callback);
            _logger.InfoFormat("Usuario '{0}' suscrito correctamente al callback {1}.", nombreUsuario, typeof(TCallback).Name);
        }

        /// <summary>
        /// Configura eventos de canal para limpieza automática de suscripción.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        public void ConfigurarEventosCanal(string nombreUsuario)
        {
            var canal = OperationContext.Current?.Channel;
            if (canal != null)
            {
                canal.Closed += (_, __) => 
                {
                    _logger.InfoFormat("Canal cerrado para usuario '{0}'. Desuscribiendo.", nombreUsuario);
                    Desuscribir(nombreUsuario);
                };
                canal.Faulted += (_, __) => 
                {
                    _logger.WarnFormat("Canal fallado (Faulted) para usuario '{0}'. Desuscribiendo.", nombreUsuario);
                    Desuscribir(nombreUsuario);
                };
            }
        }

        /// <summary>
        /// Elimina la suscripción de un usuario.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        public void Desuscribir(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            if (_suscripciones.TryRemove(nombreUsuario, out _))
            {
                _logger.InfoFormat("Usuario '{0}' desuscrito del callback {1}.", nombreUsuario, typeof(TCallback).Name);
            }
        }

        /// <summary>
        /// Intenta obtener el callback registrado para un usuario.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        /// <param name="callback">Callback registrado si existe.</param>
        /// <returns>True si el callback existe, false en caso contrario.</returns>
        public bool TryGetCallback(string nombreUsuario, out TCallback callback)
        {
            return _suscripciones.TryGetValue(nombreUsuario, out callback);
        }

        /// <summary>
        /// Obtiene el callback actual del contexto de operación.
        /// </summary>
        /// <returns>Callback del contexto actual.</returns>
        /// <exception cref="FaultException">Se lanza si no se puede obtener el callback.</exception>
        public static TCallback ObtenerCallbackActual()
        {
            var contexto = OperationContext.Current;
            if (contexto != null)
            {
                var callback = contexto.GetCallbackChannel<TCallback>();
                if (callback != null)
                {
                    return callback;
                }
                
                // Se lanza excepción, pero no se loguea aquí como Error porque puede ser manejado arriba.
                // El FaultException será atrapado por el WCF stack.
                throw new FaultException(MensajesError.Cliente.ErrorObtenerCallback);
            }

            throw new FaultException(MensajesError.Cliente.ErrorContextoOperacion);
        }

        /// <summary>
        /// Ejecuta una acción de notificación con manejo automático de errores de comunicación.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario a notificar.</param>
        /// <param name="accionNotificacion">Acción de notificación a ejecutar.</param>
        public void Notificar(string nombreUsuario, Action<TCallback> accionNotificacion)
        {
            if (!TryGetCallback(nombreUsuario, out var callback))
            {
                _logger.WarnFormat("Intento de notificación a '{0}' fallido: No se encontró callback activo.", nombreUsuario);
                return;
            }

            try
            {
                accionNotificacion(callback);
            }
            catch (CommunicationException ex)
            {
                _logger.Warn(string.Format("Error de comunicación al notificar a '{0}'. Desuscribiendo.", nombreUsuario), ex);
                Desuscribir(nombreUsuario);
            }
            catch (TimeoutException ex)
            {
                _logger.Warn(string.Format("Timeout al notificar a '{0}'. Desuscribiendo.", nombreUsuario), ex);
                Desuscribir(nombreUsuario);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Operación inválida en comunicación WCF. El canal no está en el estado correcto para la operación.", ex);
            }
        }
    }
}