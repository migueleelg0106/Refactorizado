using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Manejador generico para callbacks de servicios WCF.
    /// Gestiona el ciclo de vida de las suscripciones de callbacks.
    /// </summary>
    /// <typeparam name="TCallback">Tipo de callback a gestionar.</typeparam>
    internal class ManejadorCallback<TCallback> where TCallback : class
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(ManejadorCallback<TCallback>));
        private readonly ConcurrentDictionary<string, TCallback> _suscripciones;

        public ManejadorCallback(StringComparer comparer = null)
        {
            _suscripciones = comparer != null 
                ? new ConcurrentDictionary<string, TCallback>(comparer)
                : new ConcurrentDictionary<string, TCallback>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registra una suscripcion de callback para un usuario.
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
        }

        /// <summary>
        /// Configura eventos de canal para limpieza automatica de suscripcion.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        public void ConfigurarEventosCanal(string nombreUsuario)
        {
            var canal = OperationContext.Current?.Channel;
            if (canal != null)
            {
                canal.Closed += (_, __) => 
                {
                    Desuscribir(nombreUsuario);
                };
                canal.Faulted += (_, __) => 
                {
                    _logger.WarnFormat(
                        "Canal fallado (Faulted) para usuario '{0}'. Desuscribiendo.", 
                        nombreUsuario);
                    Desuscribir(nombreUsuario);
                };
            }
        }

        /// <summary>
        /// Elimina la suscripcion de un usuario.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        public void Desuscribir(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            _suscripciones.TryRemove(nombreUsuario, out _);
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
        /// Obtiene el callback actual del contexto de operacion.
        /// </summary>
        /// <returns>Callback del contexto actual.</returns>
        /// <exception cref="FaultException">Se lanza si no se puede obtener el callback.
        /// </exception>
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
                
                throw new FaultException(MensajesError.Cliente.ErrorObtenerCallback);
            }

            throw new FaultException(MensajesError.Cliente.ErrorContextoOperacion);
        }

        /// <summary>
        /// Ejecuta una accion de notificacion con manejo automatico de errores de comunicacion.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario a notificar.</param>
        /// <param name="accionNotificacion">Accion de notificacion a ejecutar.</param>
        public void Notificar(string nombreUsuario, Action<TCallback> accionNotificacion)
        {
            if (!TryGetCallback(nombreUsuario, out var callback))
            {
                return;
            }

            try
            {
                accionNotificacion(callback);
            }
            catch (CommunicationException excepcion)
            {
                _logger.ErrorFormat("Error de comunicacion al notificar a '{0}'. Desuscribiendo.",
                    nombreUsuario, excepcion);
                    Desuscribir(nombreUsuario);
            }
            catch (TimeoutException excepcion)
            {
                _logger.ErrorFormat("Timeout al notificar a '{0}'. Desuscribiendo.",
                    nombreUsuario, excepcion);
                    Desuscribir(nombreUsuario);
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Warn("Operacion invalida en comunicacion WCF.", excepcion);
            }
            catch (Exception excepcion)
            {
                _logger.Warn("Operacion invalida en comunicacion WCF.", excepcion);
            }
        }
    }
}
