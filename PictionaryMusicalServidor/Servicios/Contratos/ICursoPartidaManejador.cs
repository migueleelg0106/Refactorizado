using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion del curso de una partida en progreso.
    /// Proporciona operaciones para el control del flujo de juego durante una partida activa.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(ICursoPartidaManejadorCallback))]
    public interface ICursoPartidaManejador
    {
        /// <summary>
        /// Suscribe a un jugador a la partida de una sala especifica y registra su callback para
        /// notificaciones.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="idJugador">Identificador unico del jugador.</param>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        /// <param name="esHost">Indica si el jugador es el host de la sala.</param>
        [OperationContract]
        void SuscribirJugador(string idSala, string idJugador, string nombreUsuario, bool esHost);

        /// <summary>
        /// Inicia la partida de la sala indicada.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="idJugadorSolicitante">Identificador del jugador que solicita el inicio.
        /// </param>
        [OperationContract]
        void IniciarPartida(string idSala, string idJugadorSolicitante);

        /// <summary>
        /// Envia un mensaje de chat al flujo de la partida.
        /// </summary>
        /// <param name="mensaje">Mensaje a procesar.</param>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="idJugador">Identificador del jugador que envia el mensaje.</param>
        [OperationContract]
        void EnviarMensajeJuego(string mensaje, string idSala, string idJugador);

        /// <summary>
        /// Envia un trazo de dibujo para la sala especificada.
        /// </summary>
        /// <param name="trazo">Trazo que se debe procesar.</param>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="idJugador">Identificador del jugador que envia el trazo.</param>
        [OperationContract(IsOneWay = true)]
        void EnviarTrazo(TrazoDTO trazo, string idSala, string idJugador);
    }
}
