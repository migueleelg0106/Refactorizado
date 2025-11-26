using log4net;
using System;
using System.Windows;

namespace PictionaryMusicalCliente.VistaModelo.Salas
{
    /// <summary>
    /// Controla la logica del chat de la sala de juego.
    /// Esta clase proporciona la base para la funcionalidad del chat (pendiente de implementar).
    /// </summary>
    public class ChatVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool _puedeEscribir;

        /// <summary>
        /// Inicializa la VistaModelo del chat.
        /// </summary>
        public ChatVistaModelo()
        {
            _puedeEscribir = true;
        }

        /// <summary>
        /// Indica si el jugador actual puede escribir en el chat.
        /// </summary>
        public bool PuedeEscribir
        {
            get => _puedeEscribir;
            set => EstablecerPropiedad(ref _puedeEscribir, value);
        }

        /// <summary>
        /// Evento para notificar mensajes de chat entrantes.
        /// </summary>
        public event Action<string, string> MensajeChatRecibido;

        /// <summary>
        /// Accion para enviar mensaje al servidor.
        /// </summary>
        public Action<string> EnviarMensajeAlServidor { get; set; }

        /// <summary>
        /// Envia un mensaje de chat.
        /// </summary>
        /// <param name="mensaje">Contenido del mensaje.</param>
        public void EnviarMensaje(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return;
            }

            _logger.InfoFormat("Enviando mensaje de chat: {0}", mensaje);
            EnviarMensajeAlServidor?.Invoke(mensaje);
        }

        /// <summary>
        /// Procesa la notificacion de un mensaje de chat recibido.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador que envio el mensaje.</param>
        /// <param name="mensaje">Contenido del mensaje.</param>
        public void NotificarMensajeChat(string nombreJugador, string mensaje)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.Invoke(() => MensajeChatRecibido?.Invoke(nombreJugador, mensaje));
        }

        /// <summary>
        /// Notifica que un jugador adivino la cancion mostrando un mensaje en el chat.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador que adivino.</param>
        public void NotificarJugadorAdivinoEnChat(string nombreJugador)
        {
            string mensaje = $"{nombreJugador} ha adivinado la canción";
            MensajeChatRecibido?.Invoke(nombreJugador, mensaje);
        }
    }
}
