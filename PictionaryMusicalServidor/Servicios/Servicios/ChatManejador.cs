using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de chat entre jugadores.
    /// Gestiona la comunicacion en tiempo real entre jugadores conectados a una sala.
    /// Utiliza un diccionario estatico para administrar los clientes conectados por sala.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChatManejador : IChatManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ChatManejador));
        private static readonly Dictionary<string, List<ClienteChat>> _clientesPorSala = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _sincronizacion = new();

        /// <summary>
        /// Permite a un jugador unirse al chat de una sala especifica.
        /// Registra el callback del cliente y notifica a los demas participantes que alguien entro.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="nombreJugador">Nombre del jugador que se une.</param>
        /// <exception cref="FaultException">Se lanza si los datos son invalidos o hay errores de comunicacion.</exception>
        public void Unirse(string idSala, string nombreJugador)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreJugador, nameof(nombreJugador));

                if (string.IsNullOrWhiteSpace(idSala))
                {
                    throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);
                }

                var callback = ObtenerCallbackActual();
                var idSalaNormalizado = idSala.Trim();
                var nombreNormalizado = nombreJugador.Trim();
                List<ClienteChat> clientesANotificar;

                lock (_sincronizacion)
                {
                    if (!_clientesPorSala.TryGetValue(idSalaNormalizado, out var clientesSala))
                    {
                        clientesSala = new List<ClienteChat>();
                        _clientesPorSala[idSalaNormalizado] = clientesSala;
                    }

                    var clienteExistente = clientesSala.Find(c =>
                        string.Equals(c.NombreJugador, nombreNormalizado, StringComparison.OrdinalIgnoreCase));

                    if (clienteExistente != null)
                    {
                        clienteExistente.Callback = callback;
                        _logger.InfoFormat("Jugador '{0}' se reconecto al chat de la sala '{1}'.", nombreNormalizado, idSalaNormalizado);
                    }
                    else
                    {
                        clientesSala.Add(new ClienteChat(nombreNormalizado, callback));
                        _logger.InfoFormat("Jugador '{0}' se unio al chat de la sala '{1}'. Total en sala: {2}.", nombreNormalizado, idSalaNormalizado, clientesSala.Count);
                    }

                    clientesANotificar = clientesSala
                        .Where(c => !string.Equals(c.NombreJugador, nombreNormalizado, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                foreach (var cliente in clientesANotificar)
                {
                    EjecutarNotificacionSegura(idSalaNormalizado, cliente, cb => cb.NotificarJugadorUnido(nombreNormalizado));
                }

                ConfigurarEventosCanal(idSalaNormalizado, nombreNormalizado);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos invalidos al unirse al chat.", ex);
                throw new FaultException(ex.Message);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicacion al unirse al chat.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al unirse al chat.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
        }

        /// <summary>
        /// Envia un mensaje a todos los participantes del chat de una sala.
        /// Distribuye el mensaje invocando el callback RecibirMensaje de cada cliente conectado.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="mensaje">Contenido del mensaje a enviar.</param>
        /// <param name="nombreJugador">Nombre del jugador que envia el mensaje.</param>
        /// <exception cref="FaultException">Se lanza si los datos son invalidos.</exception>
        public void EnviarMensaje(string idSala, string mensaje, string nombreJugador)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreJugador, nameof(nombreJugador));

                if (string.IsNullOrWhiteSpace(idSala))
                {
                    throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);
                }

                if (string.IsNullOrWhiteSpace(mensaje))
                {
                    return;
                }

                var idSalaNormalizado = idSala.Trim();
                var nombreNormalizado = nombreJugador.Trim();
                var mensajeNormalizado = mensaje.Trim();

                NotificarMensajeATodos(idSalaNormalizado, nombreNormalizado, mensajeNormalizado);

                _logger.DebugFormat("Mensaje de '{0}' enviado a sala '{1}'.", nombreNormalizado, idSalaNormalizado);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos invalidos al enviar mensaje.", ex);
                throw new FaultException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al enviar mensaje de chat.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
        }

        /// <summary>
        /// Permite a un jugador salir del chat de una sala.
        /// Elimina el callback del cliente y notifica a los demas participantes que el jugador salio.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="nombreJugador">Nombre del jugador que sale.</param>
        /// <exception cref="FaultException">Se lanza si los datos son invalidos.</exception>
        public void Salir(string idSala, string nombreJugador)
        {
            try
            {
                ValidadorNombreUsuario.Validar(nombreJugador, nameof(nombreJugador));

                if (string.IsNullOrWhiteSpace(idSala))
                {
                    throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);
                }

                var idSalaNormalizado = idSala.Trim();
                var nombreNormalizado = nombreJugador.Trim();

                RemoverCliente(idSalaNormalizado, nombreNormalizado);

                _logger.InfoFormat("Jugador '{0}' salio del chat de la sala '{1}'.", nombreNormalizado, idSalaNormalizado);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.Warn("Datos invalidos al salir del chat.", ex);
                throw new FaultException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al salir del chat.", ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
        }

        private static IChatManejadorCallback ObtenerCallbackActual()
        {
            var contexto = OperationContext.Current;
            if (contexto == null)
            {
                throw new FaultException(MensajesError.Cliente.ErrorContextoOperacion);
            }

            var callback = contexto.GetCallbackChannel<IChatManejadorCallback>();
            if (callback == null)
            {
                throw new FaultException(MensajesError.Cliente.ErrorObtenerCallback);
            }

            return callback;
        }

        private void ConfigurarEventosCanal(string idSala, string nombreJugador)
        {
            var canal = OperationContext.Current?.Channel;
            if (canal != null)
            {
                canal.Closed += (_, __) => RemoverCliente(idSala, nombreJugador);
                canal.Faulted += (_, __) => RemoverCliente(idSala, nombreJugador);
            }
        }

        private void RemoverCliente(string idSala, string nombreJugador)
        {
            List<ClienteChat> clientesANotificar = null;

            lock (_sincronizacion)
            {
                if (!_clientesPorSala.TryGetValue(idSala, out var clientesSala))
                {
                    return;
                }

                var clienteRemovido = clientesSala.RemoveAll(c =>
                    string.Equals(c.NombreJugador, nombreJugador, StringComparison.OrdinalIgnoreCase)) > 0;

                if (clienteRemovido)
                {
                    clientesANotificar = clientesSala.ToList();

                    if (clientesSala.Count == 0)
                    {
                        _clientesPorSala.Remove(idSala);
                        _logger.InfoFormat("Sala de chat '{0}' eliminada (sin participantes).", idSala);
                    }
                }
            }

            if (clientesANotificar != null)
            {
                foreach (var cliente in clientesANotificar)
                {
                    EjecutarNotificacionSegura(idSala, cliente, cb => cb.NotificarJugadorSalio(nombreJugador));
                }
            }
        }

        private void NotificarMensajeATodos(string idSala, string nombreJugador, string mensaje)
        {
            List<ClienteChat> clientesANotificar;

            lock (_sincronizacion)
            {
                if (!_clientesPorSala.TryGetValue(idSala, out var clientesSala))
                {
                    return;
                }

                clientesANotificar = clientesSala.ToList();
            }

            foreach (var cliente in clientesANotificar)
            {
                EjecutarNotificacionSegura(idSala, cliente, callback => callback.RecibirMensaje(nombreJugador, mensaje));
            }
        }

        private void EjecutarNotificacionSegura(string idSala, ClienteChat cliente, Action<IChatManejadorCallback> accion)
        {
            try
            {
                accion(cliente.Callback);
            }
            catch (CommunicationException ex)
            {
                _logger.WarnFormat("Error de comunicacion con jugador '{0}' en sala '{1}'. Se quitara su callback.", cliente.NombreJugador, idSala);
                _logger.Warn(ex);
                RemoverClienteSinNotificar(idSala, cliente.NombreJugador);
            }
            catch (TimeoutException ex)
            {
                _logger.WarnFormat("Timeout al notificar a jugador '{0}' en sala '{1}'. Se quitara su callback.", cliente.NombreJugador, idSala);
                _logger.Warn(ex);
                RemoverClienteSinNotificar(idSala, cliente.NombreJugador);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Operacion invalida en comunicacion WCF. El canal no esta en el estado correcto para la operacion.", ex);
                RemoverClienteSinNotificar(idSala, cliente.NombreJugador);
            }
        }

        private void RemoverClienteSinNotificar(string idSala, string nombreJugador)
        {
            lock (_sincronizacion)
            {
                if (_clientesPorSala.TryGetValue(idSala, out var clientesSala))
                {
                    clientesSala.RemoveAll(c =>
                        string.Equals(c.NombreJugador, nombreJugador, StringComparison.OrdinalIgnoreCase));

                    if (clientesSala.Count == 0)
                    {
                        _clientesPorSala.Remove(idSala);
                    }
                }
            }
        }

        /// <summary>
        /// Representa un cliente conectado al chat con su nombre y callback.
        /// </summary>
        private sealed class ClienteChat
        {
            public ClienteChat(string nombreJugador, IChatManejadorCallback callback)
            {
                NombreJugador = nombreJugador;
                Callback = callback;
            }

            public string NombreJugador { get; }
            public IChatManejadorCallback Callback { get; set; }
        }
    }
}
