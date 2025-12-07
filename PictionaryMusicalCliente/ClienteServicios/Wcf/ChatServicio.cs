using System;
using System.Threading.Tasks;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Representa los posibles resultados al evaluar un mensaje en el chat.
    /// </summary>
    public enum ChatDecision
    {
        CanalLibre,
        MensajeBloqueado,
        IntentoFallido,
        AciertoRegistrado
    }

    /// <summary>
    /// Define la capacidad de enviar mensajes al sistema de chat.
    /// </summary>
    public interface IChatMensajeria
    {
        /// <summary>
        /// Envia un mensaje de texto al canal de chat.
        /// </summary>
        /// <param name="mensaje">Contenido del mensaje.</param>
        void Enviar(string mensaje);
    }

    /// <summary>
    /// Define las operaciones necesarias para registrar puntuaciones en el juego.
    /// </summary>
    public interface IChatAciertosServicio
    {
        /// <summary>
        /// Obtiene el nombre del jugador que esta utilizando la sesion actual.
        /// </summary>
        /// <returns>Nombre del jugador.</returns>
        string ObtenerNombreJugadorActual();

        /// <summary>
        /// Registra los puntos ganados tanto por el adivinador como por el dibujante.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador que acerto.</param>
        /// <param name="puntosAdivinador">Puntos para quien adivino.</param>
        /// <param name="puntosDibujante">Puntos para quien estaba dibujando.</param>
        Task RegistrarAciertoAsync(
            string nombreJugador,
            int puntosAdivinador,
            int puntosDibujante);
    }

    /// <summary>
    /// Define la logica de negocio para evaluar los mensajes del chat segun el estado de la 
    /// partida.
    /// </summary>
    public interface IChatReglasPartida
    {
        /// <summary>
        /// Obtiene o establece el nombre de la cancion que se debe adivinar.
        /// </summary>
        string NombreCancionCorrecta { get; set; }

        /// <summary>
        /// Obtiene o establece el tiempo restante de la ronda para el calculo de puntos.
        /// </summary>
        int TiempoRestante { get; set; }

        /// <summary>
        /// Evalua un mensaje para determinar si es un acierto, un mensaje normal o si debe 
        /// bloquearse.
        /// </summary>
        /// <param name="mensaje">Texto ingresado por el usuario.</param>
        /// <param name="esPartidaIniciada">Indica si la ronda esta activa.</param>
        /// <param name="esDibujante">Indica si el usuario actual es quien dibuja.</param>
        /// <returns>La decision tomada sobre el mensaje.</returns>
        Task<ChatDecision> EvaluarMensajeAsync(
            string mensaje,
            bool esPartidaIniciada,
            bool esDibujante);
    }

    /// <summary>
    /// Implementacion delegada de la mensajeria que invoca una accion externa.
    /// </summary>
    public class ChatMensajeriaDelegado : IChatMensajeria
    {
        private readonly Action<string> _enviarMensaje;

        /// <summary>
        /// Inicializa una nueva instancia con la accion de envio.
        /// </summary>
        /// <param name="enviarMensaje">Accion a ejecutar al enviar.</param>
        public ChatMensajeriaDelegado(Action<string> enviarMensaje)
        {
            _enviarMensaje = enviarMensaje ??
                throw new ArgumentNullException(nameof(enviarMensaje));
        }

        /// <inheritdoc />
        public void Enviar(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return;
            }
            _enviarMensaje(mensaje);
        }
    }

    /// <summary>
    /// Implementacion delegada que conecta la logica de chat con el registro de aciertos.
    /// </summary>
    public class ChatAciertosDelegado : IChatAciertosServicio
    {
        private readonly Func<string> _obtenerNombreJugador;
        private readonly Func<string, int, int, Task> _registrarAcierto;

        /// <summary>
        /// Inicializa el delegado con las funciones necesarias para operar.
        /// </summary>
        /// <param name="obtenerNombreJugador">Funcion para obtener el usuario actual.</param>
        /// <param name="registrarAcierto">Funcion asincrona para registrar puntos.</param>
        public ChatAciertosDelegado(
            Func<string> obtenerNombreJugador,
            Func<string, int, int, Task> registrarAcierto)
        {
            _obtenerNombreJugador = obtenerNombreJugador
                ?? throw new ArgumentNullException(nameof(obtenerNombreJugador));
            _registrarAcierto = registrarAcierto
                ?? throw new ArgumentNullException(nameof(registrarAcierto));
        }

        /// <inheritdoc />
        public string ObtenerNombreJugadorActual()
        {
            return _obtenerNombreJugador();
        }

        /// <inheritdoc />
        public Task RegistrarAciertoAsync(
            string nombreJugador,
            int puntosAdivinador,
            int puntosDibujante)
        {
            return _registrarAcierto(nombreJugador, puntosAdivinador, puntosDibujante);
        }
    }

    /// <summary>
    /// Implementa las reglas de negocio para el chat durante la partida.
    /// </summary>
    public class ChatReglasPartida : IChatReglasPartida
    {
        private const double PorcentajePuntosDibujante = 0.2;
        private readonly IChatAciertosServicio _chatAciertosServicio;

        /// <summary>
        /// Inicializa las reglas de partida con el servicio de aciertos necesario.
        /// </summary>
        /// <param name="chatAciertosServicio">Servicio para registrar puntuaciones.</param>
        public ChatReglasPartida(IChatAciertosServicio chatAciertosServicio)
        {
            _chatAciertosServicio = chatAciertosServicio
                ?? throw new ArgumentNullException(nameof(chatAciertosServicio));
        }

        /// <inheritdoc />
        public string NombreCancionCorrecta { get; set; } = string.Empty;

        /// <inheritdoc />
        public int TiempoRestante { get; set; }

        /// <inheritdoc />
        public async Task<ChatDecision> EvaluarMensajeAsync(
            string mensaje,
            bool esPartidaIniciada,
            bool esDibujante)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return ChatDecision.IntentoFallido;
            }

            ChatDecision? decisionPorEstado = ObtenerDecisionPorEstado(
                esPartidaIniciada,
                esDibujante);

            if (decisionPorEstado.HasValue)
            {
                return decisionPorEstado.Value;
            }

            if (!EsRespuestaCorrecta(mensaje))
            {
                return ChatDecision.IntentoFallido;
            }

            await RegistrarAciertoConPuntajeAsync().ConfigureAwait(false);
            return ChatDecision.AciertoRegistrado;
        }

        private bool EsRespuestaCorrecta(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(NombreCancionCorrecta))
            {
                return false;
            }

            return string.Equals(
                mensaje?.Trim(),
                NombreCancionCorrecta.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        private async Task RegistrarAciertoConPuntajeAsync()
        {
            string nombreJugador = _chatAciertosServicio.ObtenerNombreJugadorActual();
            (int puntosAdivinador, int puntosDibujante) = CalcularPuntos(TiempoRestante);

            await _chatAciertosServicio.RegistrarAciertoAsync(
                nombreJugador,
                puntosAdivinador,
                puntosDibujante)
                .ConfigureAwait(false);
        }

        private static ChatDecision? ObtenerDecisionPorEstado(
            bool esPartidaIniciada,
            bool esDibujante)
        {
            if (!esPartidaIniciada)
            {
                return ChatDecision.CanalLibre;
            }

            if (esDibujante)
            {
                return ChatDecision.MensajeBloqueado;
            }

            return null;
        }

        private static (int Adivinador, int Dibujante) CalcularPuntos(int tiempoRestante)
        {
            int puntosAdivinador = tiempoRestante;
            int puntosDibujante = (int)(puntosAdivinador * PorcentajePuntosDibujante);
            return (puntosAdivinador, puntosDibujante);
        }
    }
}