using System.Collections.Generic;
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de salas de juego.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(ISalasCallback))]
    public interface ISalasManejador
    {
        /// <summary>
        /// Crea una nueva sala de juego con la configuracion especificada.
        /// </summary>
        /// <param name="nombreCreador">Nombre del usuario que crea la sala.</param>
        /// <param name="configuracion">Configuracion de la partida para la sala.</param>
        /// <returns>Informacion de la sala creada.</returns>
        [OperationContract]
        SalaDTO CrearSala(string nombreCreador, ConfiguracionPartidaDTO configuracion);

        /// <summary>
        /// Permite a un usuario unirse a una sala existente.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala a la que se desea unir.</param>
        /// <param name="nombreUsuario">Nombre del usuario que se une.</param>
        /// <returns>Informacion actualizada de la sala.</returns>
        [OperationContract]
        SalaDTO UnirseSala(string codigoSala, string nombreUsuario);

        /// <summary>
        /// Obtiene la lista de todas las salas disponibles.
        /// </summary>
        /// <returns>Lista de salas disponibles.</returns>
        [OperationContract]
        IList<SalaDTO> ObtenerSalas();

        /// <summary>
        /// Permite a un usuario abandonar una sala.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala a abandonar.</param>
        /// <param name="nombreUsuario">Nombre del usuario que abandona.</param>
        [OperationContract]
        void AbandonarSala(string codigoSala, string nombreUsuario);

        /// <summary>
        /// Suscribe al cliente para recibir actualizaciones de la lista de salas.
        /// </summary>
        [OperationContract]
        void SuscribirListaSalas();

        /// <summary>
        /// Cancela la suscripcion para dejar de recibir actualizaciones de la lista de salas.
        /// </summary>
        [OperationContract]
        void CancelarSuscripcionListaSalas();

        /// <summary>
        /// Permite al host de una sala expulsar a un jugador.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala.</param>
        /// <param name="nombreHost">Nombre del host que expulsa.</param>
        /// <param name="nombreJugadorAExpulsar">Nombre del jugador a expulsar.</param>
        [OperationContract]
        void ExpulsarJugador(string codigoSala, string nombreHost, string nombreJugadorAExpulsar);
    }
}
