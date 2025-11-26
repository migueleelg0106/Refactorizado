using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de callback para notificar eventos de la partida a los clientes.
    /// </summary>
    [ServiceContract]
    public interface IPartidaManejadorCallback
    {
        /// <summary>
        /// Notifica el inicio de una nueva ronda con la informacion correspondiente.
        /// </summary>
        /// <param name="info">Datos de la ronda que inicia.</param>
        [OperationContract(IsOneWay = true)]
        void IniciarRonda(RondaDTO info);

        /// <summary>
        /// Notifica que la ronda actual ha finalizado con los resultados.
        /// </summary>
        /// <param name="resultado">Resultados de la ronda finalizada.</param>
        [OperationContract(IsOneWay = true)]
        void DetenerRonda(ResultadoRondaDTO resultado);

        /// <summary>
        /// Notifica la actualizacion del cronometro durante la ronda.
        /// </summary>
        /// <param name="segundos">Segundos restantes en el cronometro.</param>
        [OperationContract(IsOneWay = true)]
        void ActualizarCronometro(int segundos);

        /// <summary>
        /// Notifica la recepcion de un trazo de dibujo en el lienzo compartido.
        /// </summary>
        /// <param name="trazo">Datos del trazo recibido.</param>
        [OperationContract(IsOneWay = true)]
        void RecibirTrazo(TrazoDTO trazo);

        /// <summary>
        /// Notifica que un jugador ha adivinado correctamente la cancion.
        /// </summary>
        /// <param name="usuario">Nombre del usuario que adivino.</param>
        /// <param name="puntos">Puntos obtenidos por el acierto.</param>
        [OperationContract(IsOneWay = true)]
        void NotificarAcierto(string usuario, int puntos);
    }
}
