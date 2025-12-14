using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Servicios.Notificadores
{
    /// <summary>
    /// Implementacion del gestor de notificaciones que maneja los callbacks WCF 
    /// y el control de errores de comunicacion.
    /// </summary>
    public class GestorNotificacionesSalaInterna : IGestorNotificacionesSalaInterna
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(GestorNotificacionesSalaInterna));
        private readonly Dictionary<string, ISalasManejadorCallback> _callbacks;
        private readonly object _sincronizacion = new object();

        /// <summary>
        /// Inicializa una nueva instancia del gestor de notificaciones.
        /// </summary>
        public GestorNotificacionesSalaInterna()
        {
            _callbacks = new Dictionary<string, ISalasManejadorCallback>
                (StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registra un cliente para recibir notificaciones de la sala.
        /// </summary>
        public void Registrar(string nombreUsuario, ISalasManejadorCallback callback)
        {
            lock (_sincronizacion)
            {
                _callbacks[nombreUsuario] = callback;
            }
        }

        /// <summary>
        /// Elimina el registro de notificaciones de un cliente.
        /// </summary>
        public void Remover(string nombreUsuario)
        {
            lock (_sincronizacion)
            {
                _callbacks.Remove(nombreUsuario);
            }
        }

        /// <summary>
        /// Obtiene el callback asociado a un usuario especifico.
        /// </summary>
        public ISalasManejadorCallback ObtenerCallback(string nombreUsuario)
        {
            lock (_sincronizacion)
            {
                if (_callbacks.TryGetValue(nombreUsuario, out var callback))
                {
                    return callback;
                }
                return new SalasCallbackNulo();
            }
        }

        /// <summary>
        /// Limpia todos los registros de notificaciones.
        /// </summary>
        public void Limpiar()
        {
            lock (_sincronizacion)
            {
                _callbacks.Clear();
            }
        }

        /// <summary>
        /// Notifica a los integrantes que un nuevo jugador ha ingresado y envia la sala 
        /// actualizada.
        /// </summary>
        public void NotificarIngreso(string codigoSala, string nombreUsuario, 
            SalaDTO salaActualizada)
        {
            var destinatarios = ObtenerDestinatariosExcluyendo(nombreUsuario);
            foreach (var callback in destinatarios)
            {
                EjecutarSeguro(() => callback.NotificarJugadorSeUnio(codigoSala, nombreUsuario));
            }
            NotificarActualizacionGlobal(salaActualizada);
        }

        /// <summary>
        /// Notifica a los integrantes que un jugador ha salido y envia la sala actualizada.
        /// </summary>
        public void NotificarSalida(string codigoSala, string nombreUsuario, 
            SalaDTO salaActualizada)
        {
            var destinatarios = ObtenerTodosLosDestinatarios();
            foreach (var callback in destinatarios)
            {
                EjecutarSeguro(() => callback.NotificarJugadorSalio(codigoSala, nombreUsuario));
                EjecutarSeguro(() => callback.NotificarSalaActualizada(salaActualizada));
            }
        }

        /// <summary>
        /// Notifica una expulsion especifica al afectado y actualiza a los demas.
        /// </summary>
        public void NotificarExpulsion(string codigoSala, string nombreExpulsado, 
            ISalasManejadorCallback callbackExpulsado, SalaDTO salaActualizada)
        {
            EjecutarSeguro(() => callbackExpulsado.NotificarJugadorExpulsado
            (codigoSala, nombreExpulsado));
            NotificarSalida(codigoSala, nombreExpulsado, salaActualizada);
        }

        /// <summary>
        /// Notifica a todos los integrantes que la sala ha sido cancelada.
        /// </summary>
        public void NotificarCancelacion(string codigoSala)
        {
            var destinatarios = ObtenerTodosLosDestinatarios();
            foreach (var callback in destinatarios)
            {
                EjecutarSeguro(() => callback.NotificarSalaCancelada(codigoSala));
            }
        }

        private List<ISalasManejadorCallback> ObtenerDestinatariosExcluyendo
            (string usuarioExcluido)
        {
            lock (_sincronizacion)
            {
                return _callbacks
                    .Where(callbackRegistrado => !string.Equals(callbackRegistrado.Key, usuarioExcluido,
                        StringComparison.OrdinalIgnoreCase))
                    .Select(callbackRegistrado => callbackRegistrado.Value)
                    .ToList();
            }
        }

        private List<ISalasManejadorCallback> ObtenerTodosLosDestinatarios()
        {
            lock (_sincronizacion)
            {
                return _callbacks.Values.ToList();
            }
        }

        private void NotificarActualizacionGlobal(SalaDTO sala)
        {
            var destinatarios = ObtenerTodosLosDestinatarios();
            foreach (var callback in destinatarios)
            {
                EjecutarSeguro(() => callback.NotificarSalaActualizada(sala));
            }
        }

        private static void EjecutarSeguro(Action accion)
        {
            try
            {
                accion();
            }
            catch (CommunicationException excepcion)
            {
                _logger.Warn("Error de comunicacion al notificar cliente en sala.", excepcion);
            }
            catch (TimeoutException excepcion)
            {
                _logger.Warn("Tiempo de espera agotado al notificar cliente en sala.", excepcion);
            }
            catch (ObjectDisposedException excepcion)
            {
                _logger.Error("Error inesperado al ejecutar notificacion WCF en sala.", excepcion);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado al ejecutar notificacion WCF en sala.", excepcion);
            }
        }

        /// <summary>
        /// Implementacion del patron Null Object para evitar referencias nulas en callbacks.
        /// </summary>
        private sealed class SalasCallbackNulo : ISalasManejadorCallback
        {
            public void NotificarJugadorSeUnio(string codigoSala, string nombreJugador) { }
            public void NotificarJugadorSalio(string codigoSala, string nombreJugador) { }
            public void NotificarListaSalasActualizada(SalaDTO[] salas) { }
            public void NotificarSalaActualizada(SalaDTO sala) { }
            public void NotificarJugadorExpulsado(string codigoSala, string nombreJugador) { }
            public void NotificarSalaCancelada(string codigoSala) { }
        }
    }
}
