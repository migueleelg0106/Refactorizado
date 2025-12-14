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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChatManejador : IChatManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ChatManejador));

        private static readonly Dictionary<string, List<ClienteChat>> _clientesPorSala =
            new Dictionary<string, List<ClienteChat>>(StringComparer.OrdinalIgnoreCase);

        private static readonly object _sincronizacion = new object();

        private readonly IValidadorNombreUsuario _validadorUsuario;

        /// <summary>
        /// Constructor por defecto para WCF.
        /// Inicializa las dependencias manualmente.
        /// </summary>
        public ChatManejador() : this(new ValidadorNombreUsuario())
        {
        }

        /// <summary>
        /// Constructor con inyeccion de dependencias.
        /// </summary>
        public ChatManejador(IValidadorNombreUsuario validadorUsuario)
        {
            _validadorUsuario = validadorUsuario ??
                throw new ArgumentNullException(nameof(validadorUsuario));
        }

        /// <summary>
        /// Permite a un jugador unirse al chat de una sala especifica.
        /// Registra el callback del cliente y notifica a los demas participantes que alguien entro.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="nombreJugador">Nombre del jugador que se une.</param>
        public void UnirseChatSala(string idSala, string nombreJugador)
        {
            try
            {
                ValidarEntradaUnirse(idSala, nombreJugador);

                var callback = ObtenerCallbackActual();
                var idSalaNormalizado = idSala.Trim();
                var nombreNormalizado = nombreJugador.Trim();

                var clientesANotificar = GestionarIngresoSala(
                    idSalaNormalizado,
                    nombreNormalizado,
                    callback);

                NotificarIngresoMasivo(
                    idSalaNormalizado,
                    nombreNormalizado,
                    clientesANotificar);

                ConfigurarEventosCanal(idSalaNormalizado, nombreNormalizado);
            }
            catch (FaultException excepcion)
            {
                _logger.Warn("Error de validacion al unirse al chat.", excepcion);
                throw;
            }
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Datos invalidos al unirse al chat.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (CommunicationException excepcion)
            {
                _logger.Error("Error de comunicacion al unirse al chat.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
            catch (TimeoutException excepcion)
            {
                _logger.Error("Tiempo de espera agotado al unirse al chat.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
            catch (ObjectDisposedException excepcion)
            {
                _logger.Error("Error inesperado al unirse al chat.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado al unirse al chat.", excepcion);
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
        public void EnviarMensaje(string idSala, string mensaje, string nombreJugador)
        {
            try
            {
                _validadorUsuario.Validar(nombreJugador, nameof(nombreJugador));

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

                NotificarMensajeATodos(
                    idSalaNormalizado,
                    nombreNormalizado,
                    mensajeNormalizado);
            }
            catch (FaultException excepcion)
            {
                _logger.Warn("Error de validacion al enviar mensaje de chat.", excepcion);
                throw;
            }
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Datos invalidos al enviar mensaje.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (CommunicationException excepcion)
            {
                _logger.Error("Error de comunicacion al enviar mensaje de chat.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
            catch (TimeoutException excepcion)
            {
                _logger.Error("Tiempo de espera agotado al enviar mensaje de chat.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
            catch (ObjectDisposedException excepcion)
            {
                _logger.Error("Error inesperado al enviar mensaje de chat.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado al enviar mensaje de chat.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
        }

        /// <summary>
        /// Permite a un jugador salir del chat de una sala.
        /// Elimina el callback y notifica a los demas participantes que el jugador salio.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="nombreJugador">Nombre del jugador que sale.</param>
        public void SalirChatSala(string idSala, string nombreJugador)
        {
            try
            {
                _validadorUsuario.Validar(nombreJugador, nameof(nombreJugador));

                if (string.IsNullOrWhiteSpace(idSala))
                {
                    throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);
                }

                var idSalaNormalizado = idSala.Trim();
                var nombreNormalizado = nombreJugador.Trim();

                RemoverCliente(idSalaNormalizado, nombreNormalizado);
            }
            catch (FaultException excepcion)
            {
                _logger.Warn("Error de validacion al salir del chat.", excepcion);
                throw;
            }
            catch (ArgumentException excepcion)
            {
                _logger.Warn("Datos invalidos al salir del chat.", excepcion);
                throw new FaultException(excepcion.Message);
            }
            catch (CommunicationException excepcion)
            {
                _logger.Error("Error de comunicacion al salir del chat.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
            catch (TimeoutException excepcion)
            {
                _logger.Error("Tiempo de espera agotado al salir del chat.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
            catch (ObjectDisposedException excepcion)
            {
                _logger.Error("Error inesperado al salir del chat.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado al salir del chat.", excepcion);
                throw new FaultException(MensajesError.Cliente.ErrorInesperado);
            }
        }

        private void ValidarEntradaUnirse(string idSala, string nombreJugador)
        {
            _validadorUsuario.Validar(nombreJugador, nameof(nombreJugador));

            if (string.IsNullOrWhiteSpace(idSala))
            {
                throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);
            }
        }

        private List<ClienteChat> GestionarIngresoSala(
            string idSala,
            string nombreJugador,
            IChatManejadorCallback callback)
        {
            lock (_sincronizacion)
            {
                if (!_clientesPorSala.TryGetValue(idSala, out var clientesSala))
                {
                    clientesSala = new List<ClienteChat>();
                    _clientesPorSala[idSala] = clientesSala;
                }

                var clienteExistente = clientesSala.Find(c =>
                    string.Equals(
                        c.NombreJugador,
                        nombreJugador,
                        StringComparison.OrdinalIgnoreCase));

                if (clienteExistente != null)
                {
                    clienteExistente.Callback = callback;
                }
                else
                {
                    clientesSala.Add(new ClienteChat(nombreJugador, callback));
                }

                return clientesSala
                    .Where(c => !string.Equals(
                        c.NombreJugador,
                        nombreJugador,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        private void NotificarIngresoMasivo(
            string idSala,
            string nombreJugador,
            List<ClienteChat> destinatarios)
        {
            foreach (var cliente in destinatarios)
            {
                EjecutarNotificacionSegura(
                    idSala,
                    cliente,
                    cb => cb.NotificarJugadorUnido(nombreJugador));
            }
        }

        private void RemoverCliente(string idSala, string nombreJugador)
        {
            var clientesANotificar = ProcesarSalidaSala(idSala, nombreJugador);

            if (clientesANotificar != null && clientesANotificar.Count > 0)
            {
                NotificarSalidaMasiva(idSala, nombreJugador, clientesANotificar);
            }
        }

        private List<ClienteChat> ProcesarSalidaSala(string idSala, string nombreJugador)
        {
            lock (_sincronizacion)
            {
                if (!_clientesPorSala.TryGetValue(idSala, out var clientesSala))
                {
                    return new List<ClienteChat>();
                }

                var clienteRemovido = clientesSala.RemoveAll(c =>
                    string.Equals(
                        c.NombreJugador,
                        nombreJugador,
                        StringComparison.OrdinalIgnoreCase)) > 0;

                if (!clienteRemovido)
                {
                    return new List<ClienteChat>();
                }

                var remanentes = clientesSala.ToList();

                if (clientesSala.Count == 0)
                {
                    _clientesPorSala.Remove(idSala);
                }

                return remanentes;
            }
        }

        private void NotificarSalidaMasiva(
            string idSala,
            string nombreJugador,
            List<ClienteChat> destinatarios)
        {
            foreach (var cliente in destinatarios)
            {
                EjecutarNotificacionSegura(
                    idSala,
                    cliente,
                    cb => cb.NotificarJugadorSalio(nombreJugador));
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

        private void NotificarMensajeATodos(
            string idSala,
            string nombreJugador,
            string mensaje)
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
                EjecutarNotificacionSegura(
                    idSala,
                    cliente,
                    callback => callback.RecibirMensaje(nombreJugador, mensaje));
            }
        }

        private void EjecutarNotificacionSegura(
            string idSala,
            ClienteChat cliente,
            Action<IChatManejadorCallback> accion)
        {
            try
            {
                accion(cliente.Callback);
            }
            catch (CommunicationException excepcion)
            {
                _logger.Warn("Error de comunicacion con cliente. Removiendo callback.", excepcion);
                RemoverClienteSinNotificar(idSala, cliente.NombreJugador);
            }
            catch (TimeoutException excepcion)
            {
                _logger.Warn("Timeout al notificar cliente. Removiendo callback.", excepcion);
                RemoverClienteSinNotificar(idSala, cliente.NombreJugador);
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.Warn(
                    "Operacion invalida en canal WCF. Removiendo callback.",
                    excepcion);
                RemoverClienteSinNotificar(idSala, cliente.NombreJugador);
            }
            catch (Exception excepcion)
            {
                _logger.Warn(
                    "Operacion invalida en canal WCF. Removiendo callback.",
                    excepcion);
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
                        string.Equals(
                            c.NombreJugador,
                            nombreJugador,
                            StringComparison.OrdinalIgnoreCase));

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
