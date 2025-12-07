using System.Collections.Generic;
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de salas de juego.
    /// Proporciona operaciones para crear, unirse, abandonar y gestionar salas con soporte de 
    /// callbacks.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(ISalasManejadorCallback))]
    public interface ISalasManejador
    {
        /// <summary>
        /// Crea una nueva sala de juego.
        /// </summary>
        /// <param name="nombreCreador">Nombre del usuario que crea la sala.</param>
        /// <param name="configuracion">Configuracion de la partida para la sala.</param>
        /// <returns>Datos de la sala creada.</returns>
        [OperationContract]
        SalaDTO CrearSala(string nombreCreador, ConfiguracionPartidaDTO configuracion);

        /// <summary>
        /// Une un usuario a una sala de juego existente.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <param name="nombreUsuario">Nombre del usuario que se une a la sala.</param>
        /// <returns>Datos de la sala a la que se unio.</returns>
        [OperationContract]
        SalaDTO UnirseSala(string codigoSala, string nombreUsuario);

        /// <summary>
        /// Obtiene la lista de todas las salas de juego disponibles.
        /// </summary>
        /// <returns>Lista de salas disponibles.</returns>
        [OperationContract]
        IList<SalaDTO> ObtenerSalas();

        /// <summary>
        /// Permite a un usuario abandonar una sala de juego.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <param name="nombreUsuario">Nombre del usuario que abandona la sala.</param>
        [OperationContract]
        void AbandonarSala(string codigoSala, string nombreUsuario);

        /// <summary>
        /// Suscribe al cliente para recibir notificaciones sobre cambios en la lista de salas.
        /// </summary>
        [OperationContract]
        void SuscribirListaSalas();

        /// <summary>
        /// Cancela la suscripcion del cliente de notificaciones de la lista de salas.
        /// </summary>
        [OperationContract]
        void CancelarSuscripcionListaSalas();

        /// <summary>
        /// Expulsa un jugador de una sala de juego.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <param name="nombreHost">Nombre del usuario anfitrion que expulsa.</param>
        /// <param name="nombreJugadorAExpulsar">Nombre del jugador a expulsar.</param>
        [OperationContract]
        void ExpulsarJugador(string codigoSala, string nombreHost, string nombreJugadorAExpulsar);

        /// <summary>
        /// Obtiene una sala por su codigo identificador.
        /// Metodo expuesto para uso interno de otros servicios (Invitaciones, CursoPartida).
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        /// <returns>Datos de la sala como DTO.</returns>
        [OperationContract]
        SalaDTO ObtenerSalaPorCodigo(string codigoSala);

        /// <summary>
        /// Marca una sala como iniciada para prevenir que nuevos jugadores se unan.
        /// Metodo expuesto para uso interno del manejador de curso de partida.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        [OperationContract]
        void MarcarPartidaComoIniciada(string codigoSala);
        
        /// <summary>
        /// Marca una partida como finalizada para prevenir mandar mensajes de salida
        /// posteriores a la finalización.
        /// </summary>
        /// <param name="codigoSala">Codigo identificador de la sala.</param>
        [OperationContract]
        void MarcarPartidaComoFinalizada(string codigoSala);
    }
}