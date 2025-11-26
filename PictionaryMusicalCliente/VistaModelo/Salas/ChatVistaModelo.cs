using log4net;
using PictionaryMusicalCliente.Properties.Langs;
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

        private const double PorcentajePuntosDibujante = 0.2;

        private bool _puedeEscribir;
        private bool _esPartidaIniciada;
        private bool _esDibujante;
        private string _nombreCancionCorrecta;
        private int _tiempoRestante;

        /// <summary>
        /// Inicializa la VistaModelo del chat.
        /// </summary>
        public ChatVistaModelo()
        {
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
            set => EstablecerPropiedad(ref _nombreCancionCorrecta, value);
        }

        /// <summary>
        /// Tiempo restante del turno actual en segundos.
        /// </summary>
        public int TiempoRestante
        {
            get => _tiempoRestante;
            set => EstablecerPropiedad(ref _tiempoRestante, value);
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
        /// Accion para enviar mensaje al servidor de chat.
        /// </summary>
        public Action<string> EnviarMensajeAlServidor { get; set; }

        /// <summary>
        /// Funcion para registrar un acierto en el servidor de juego.
        /// Parametros: nombreJugador, puntosAdivinador, puntosDibujante.
        /// </summary>
        public Func<string, int, int, Task> RegistrarAciertoEnServidor { get; set; }

        /// <summary>
        /// Accion para obtener el nombre del jugador actual.
        /// </summary>
        public Func<string> ObtenerNombreJugadorActual { get; set; }

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

            if (!EsPartidaIniciada)
            {
                _logger.InfoFormat("Enviando mensaje de chat (partida no iniciada): {0}", mensaje);
                EnviarMensajeAlServidor?.Invoke(mensaje);
                return;
            }

            if (EsDibujante)
            {
                _logger.Info("El dibujante no puede enviar mensajes durante su turno.");
                return;
            }

            if (EsRespuestaCorrecta(mensaje))
            {
                await ProcesarAciertoAsync().ConfigureAwait(false);
            }
            else
            {
                _logger.InfoFormat("Enviando mensaje de chat (intento fallido): {0}", mensaje);
                EnviarMensajeAlServidor?.Invoke(mensaje);
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

        private bool EsRespuestaCorrecta(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(NombreCancionCorrecta))
            {
                return false;
            }

            return string.Equals(
                mensaje.Trim(),
                NombreCancionCorrecta.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        private async Task ProcesarAciertoAsync()
        {
            string nombreJugador = ObtenerNombreJugadorActual?.Invoke() ?? string.Empty;

            int puntosAdivinador = TiempoRestante;
            int puntosDibujante = (int)(puntosAdivinador * PorcentajePuntosDibujante);

            _logger.InfoFormat(
                "Acierto detectado. Jugador: {0}, Puntos adivinador: {1}, Puntos dibujante: {2}",
                nombreJugador,
                puntosAdivinador,
                puntosDibujante);

            PuedeEscribir = false;

            if (RegistrarAciertoEnServidor != null)
            {
                await RegistrarAciertoEnServidor(
                    nombreJugador,
                    puntosAdivinador,
                    puntosDibujante)
                    .ConfigureAwait(false);
            }
        }
    }
}
