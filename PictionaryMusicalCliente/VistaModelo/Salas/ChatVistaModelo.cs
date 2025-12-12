using log4net;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using System;
using System.Threading.Tasks;
using System.Windows;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;

namespace PictionaryMusicalCliente.VistaModelo.Salas
{
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

        public ChatVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            IChatMensajeria chatMensajeria,
            IChatReglasPartida chatReglasPartida)
            : base(ventana, localizador)
        {
            _chatMensajeria = chatMensajeria
                ?? throw new ArgumentNullException(nameof(chatMensajeria));
            _chatReglasPartida = chatReglasPartida
                ?? throw new ArgumentNullException(nameof(chatReglasPartida));
            InicializarEstadoChat();
        }

        private void InicializarEstadoChat()
        {
            _puedeEscribir = true;
            _esPartidaIniciada = false;
            _esDibujante = false;
            _nombreCancionCorrecta = string.Empty;
            _tiempoRestante = 0;
        }

        public bool PuedeEscribir
        {
            get => _puedeEscribir;
            set => EstablecerPropiedad(ref _puedeEscribir, value);
        }

        public bool EsPartidaIniciada
        {
            get => _esPartidaIniciada;
            set => EstablecerPropiedad(ref _esPartidaIniciada, value);
        }

        public bool EsDibujante
        {
            get => _esDibujante;
            set => EstablecerPropiedad(ref _esDibujante, value);
        }

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

        public event Action<string, string> MensajeChatRecibido;
        public event Action<string, string> MensajeDoradoRecibido;

        public async Task EnviarMensaje(string mensaje)
        {
            if (!ValidarMensaje(mensaje))
            {
                return;
            }

            var decision = await EvaluarMensajeAsync(mensaje).ConfigureAwait(false);
            ProcesarDecisionChat(decision, mensaje);
        }

        private bool ValidarMensaje(string mensaje)
        {
            return !string.IsNullOrWhiteSpace(mensaje);
        }

        private async Task<ChatDecision> EvaluarMensajeAsync(string mensaje)
        {
            return await _chatReglasPartida
                .EvaluarMensajeAsync(mensaje, EsPartidaIniciada, EsDibujante)
                .ConfigureAwait(false);
        }

        private void ProcesarDecisionChat(ChatDecision decision, string mensaje)
        {
            switch (decision)
            {
                case ChatDecision.CanalLibre:
                    EnviarMensajeCanalLibre(mensaje);
                    break;
                case ChatDecision.IntentoFallido:
                    EnviarMensajeIntentoFallido(mensaje);
                    break;
                case ChatDecision.MensajeBloqueado:
                    RegistrarMensajeBloqueado();
                    break;
                case ChatDecision.AciertoRegistrado:
                    MarcarAciertoRegistrado();
                    break;
            }
        }

        private void EnviarMensajeCanalLibre(string mensaje)
        {
            _logger.InfoFormat("Enviando mensaje de chat (partida no iniciada): {0}", mensaje);
            _chatMensajeria.Enviar(mensaje);
        }

        private void EnviarMensajeIntentoFallido(string mensaje)
        {
            _logger.InfoFormat("Enviando mensaje de chat (intento fallido): {0}", mensaje);
            _chatMensajeria.Enviar(mensaje);
        }

        private void RegistrarMensajeBloqueado()
        {
            _logger.Info("El dibujante no puede enviar mensajes durante su turno.");
        }

        private void MarcarAciertoRegistrado()
        {
            PuedeEscribir = false;
        }

        public void NotificarMensajeChat(string nombreJugador, string mensaje)
        {
            EjecutarEnDispatcher(() => MensajeChatRecibido?.Invoke(nombreJugador, mensaje));
        }

        public void NotificarJugadorAdivinoEnChat(string nombreJugador)
        {
            string mensajeDorado = CrearMensajeAdivinacion(nombreJugador);
            EjecutarEnDispatcher(() => MensajeDoradoRecibido?.Invoke(string.Empty, mensajeDorado));
        }

        private string CrearMensajeAdivinacion(string nombreJugador)
        {
            return string.Format(Lang.chatTextoJugadorAdivino, nombreJugador);
        }

        private void EjecutarEnDispatcher(Action accion)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            if (dispatcher.CheckAccess())
            {
                accion();
            }
            else
            {
                dispatcher.BeginInvoke(accion);
            }
        }
    }
}
