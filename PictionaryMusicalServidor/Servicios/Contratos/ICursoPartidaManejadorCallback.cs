using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de callback para notificar eventos del curso de la partida a los clientes.
    /// </summary>
    [ServiceContract]
    public interface ICursoPartidaManejadorCallback
    {
        /// <summary>
        /// Notifica que la partida ha sido iniciada por el host.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotificarPartidaIniciada();

        /// <summary>
        /// Notifica el inicio de una nueva ronda junto con su informacion.
        /// </summary>
        /// <param name="ronda">Datos de la ronda.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarInicioRonda(RondaDTO ronda);

        /// <summary>
        /// Notifica que un jugador adivino la cancion en curso.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador.</param>
        /// <param name="puntos">Puntos obtenidos.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarJugadorAdivino(string nombreJugador, int puntos);

        /// <summary>
        /// Notifica un mensaje de chat recibido durante la partida.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador que envio el mensaje.</param>
        /// <param name="mensaje">Contenido del mensaje.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarMensajeChat(string nombreJugador, string mensaje);

        /// <summary>
        /// Notifica a los clientes que un trazo fue dibujado en el lienzo compartido.
        /// </summary>
        /// <param name="trazo">Trazo recibido.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarTrazoRecibido(TrazoDTO trazo);

        /// <summary>
        /// Notifica que la ronda actual ha finalizado.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotificarFinRonda();

        /// <summary>
        /// Notifica que la partida termino y envia los resultados finales.
        /// </summary>
        /// <param name="resultado">Resultados de la partida.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarFinPartida(ResultadoPartidaDTO resultado);
    }
}
