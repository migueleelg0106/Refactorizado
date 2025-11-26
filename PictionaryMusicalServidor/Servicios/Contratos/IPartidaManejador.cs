using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de la partida del juego.
    /// Proporciona operaciones para controlar el flujo de juego durante una partida activa.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IPartidaManejadorCallback))]
    public interface IPartidaManejador
    {
        /// <summary>
        /// Inicia la partida en la sala especificada.
        /// </summary>
        /// <param name="idSala">Identificador de la sala donde se iniciara la partida.</param>
        [OperationContract]
        void IniciarPartida(string idSala);

        /// <summary>
        /// Envia un trazo de dibujo a los demas jugadores de la sala.
        /// </summary>
        /// <param name="trazo">Datos del trazo a enviar.</param>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="idJugador">Identificador del jugador que envia el trazo.</param>
        [OperationContract(IsOneWay = true)]
        void EnviarTrazo(TrazoDTO trazo, string idSala, string idJugador);

        /// <summary>
        /// Envia un intento de adivinanza de la cancion por parte de un jugador.
        /// </summary>
        /// <param name="idSala">Identificador de la sala.</param>
        /// <param name="palabra">Palabra o frase que el jugador intenta como respuesta.</param>
        /// <param name="segundosRestantes">Segundos restantes en el cronometro al momento del intento.</param>
        [OperationContract]
        void EnviarIntento(string idSala, string palabra, int segundosRestantes);
    }
}
