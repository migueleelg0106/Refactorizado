using log4net;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace PictionaryMusicalCliente.VistaModelo.Salas
{
    /// <summary>
    /// Controla la logica del chat de la sala de juego.
    /// Esta clase proporciona la base para la funcionalidad del chat y actua como
    /// validador de respuestas cuando la partida esta iniciada.
    /// </summary>
    public class ChatVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IChatMensajeria _chatMensajeria;
        private readonly IChatReglasPartida _chatReglasPartida;

        private bool _puedeEscribir;
        private bool _esPartidaIniciada;
        private bool _esDibujante;
        private string _nombreCancionCorrecta;
        private int _tiempoRestante;

        /// <summary>
        /// Inicializa la VistaModelo del chat.
        /// </summary>
        public ChatVistaModelo(IChatMensajeria chatMensajeria,
            IChatReglasPartida chatReglasPartida)
        {
            _chatMensajeria = chatMensajeria
                ?? throw new ArgumentNullException(nameof(chatMensajeria));
            _chatReglasPartida = chatReglasPartida
                ?? throw new ArgumentNullException(nameof(chatReglasPartida));
            _puedeEscribir = true;
            _esPartidaIniciada = false;
            _esDibujante = false;
            _nombreCancionCorrecta = string.Empty;
            _tiempoRestante = 0;
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
        /// Indica si la partida ha iniciado.
        /// </summary>
        public bool EsPartidaIniciada
        {
            get => _esPartidaIniciada;
            set => EstablecerPropiedad(ref _esPartidaIniciada, value);
        }

        /// <summary>
        /// Indica si el jugador actual es el dibujante del turno.
        /// </summary>
        public bool EsDibujante
        {
            get => _esDibujante;
            set => EstablecerPropiedad(ref _esDibujante, value);
        }

        /// <summary>
        /// Nombre de la cancion correcta que debe adivinarse.
        /// </summary>
        public string NombreCancionCorrecta
        {
            get => _nombreCancionCorrecta;
            set
            {
                if (EstablecerPropiedad(ref _nombreCancionCorrecta, value))
                {
                    _chatReglasPartida.NombreCancionCorrecta = value;
                }
            }
        }

        /// <summary>
        /// Tiempo restante del turno actual en segundos.
        /// </summary>
        public int TiempoRestante
        {
            get => _tiempoRestante;
            set
            {
                if (EstablecerPropiedad(ref _tiempoRestante, value))
                {
                    _chatReglasPartida.TiempoRestante = value;
                }
            }
        }

        /// <summary>
        /// Evento para notificar mensajes de chat entrantes.
        /// </summary>
        public event Action<string, string> MensajeChatRecibido;

        /// <summary>
        /// Evento para notificar mensajes dorados (aciertos) al chat.
        /// </summary>
        public event Action<string, string> MensajeDoradoRecibido;

        /// <summary>
        /// Envia un mensaje de chat con logica de intercepcion segun el estado del juego.
        /// </summary>
        /// <param name="mensaje">Contenido del mensaje.</param>
        public async Task EnviarMensaje(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return;
            }

            var decision = await _chatReglasPartida
                .EvaluarMensajeAsync(mensaje, EsPartidaIniciada, EsDibujante)
                .ConfigureAwait(false);

            switch (decision)
            {
                case ChatDecision.CanalLibre:
                    _logger.InfoFormat(
                        "Enviando mensaje de chat (partida no iniciada): {0}",
                        mensaje);
                    _chatMensajeria.Enviar(mensaje);
                    break;
                case ChatDecision.IntentoFallido:
                    _logger.InfoFormat("Enviando mensaje de chat (intento fallido): {0}", mensaje);
                    _chatMensajeria.Enviar(mensaje);
                    break;
                case ChatDecision.MensajeBloqueado:
                    _logger.Info("El dibujante no puede enviar mensajes durante su turno.");
                    break;
                case ChatDecision.AciertoRegistrado:
                    PuedeEscribir = false;
                    break;
            }
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

            if (dispatcher.CheckAccess())
            {
                MensajeChatRecibido?.Invoke(nombreJugador, mensaje);
            }
            else
            {
                dispatcher.BeginInvoke(() => MensajeChatRecibido?.Invoke(nombreJugador, mensaje));
            }
        }

        /// <summary>
        /// Notifica que un jugador adivino la cancion mostrando un mensaje dorado en el chat.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador que adivino.</param>
        public void NotificarJugadorAdivinoEnChat(string nombreJugador)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }
            string mensajeDorado = string.Format(Lang.chatTextoJugadorAdivino, nombreJugador);

            if (dispatcher.CheckAccess())
            {
                MensajeDoradoRecibido?.Invoke(string.Empty, mensajeDorado);
            }
            else
            {
                dispatcher.BeginInvoke(() => MensajeDoradoRecibido?.Invoke(string.Empty, mensajeDorado));
            }
        }
    }
}
