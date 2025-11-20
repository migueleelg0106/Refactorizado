using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de gestion de salas de juego.
    /// </summary>
    public interface ISalasServicio : IDisposable
    {
        /// <summary>
        /// Evento que se dispara cuando un jugador se une a una sala.
        /// </summary>
        event EventHandler<string> JugadorSeUnio;
        
        /// <summary>
        /// Evento que se dispara cuando un jugador sale de una sala.
        /// </summary>
        event EventHandler<string> JugadorSalio;
        
        /// <summary>
        /// Evento que se dispara cuando un jugador es expulsado de una sala.
        /// </summary>
        event EventHandler<string> JugadorExpulsado;
        
        /// <summary>
        /// Evento que se dispara cuando la lista de salas se actualiza.
        /// </summary>
        event EventHandler<IReadOnlyList<DTOs.SalaDTO>> ListaSalasActualizada;
        
        /// <summary>
        /// Evento que se dispara cuando una sala especifica se actualiza.
        /// </summary>
        event EventHandler<DTOs.SalaDTO> SalaActualizada;

        /// <summary>
        /// Crea una nueva sala de juego.
        /// </summary>
        /// <param name="nombreCreador">Nombre del creador de la sala.</param>
        /// <param name="configuracion">Configuracion de la partida.</param>
        /// <returns>Informacion de la sala creada.</returns>
        Task<DTOs.SalaDTO> CrearSalaAsync(string nombreCreador, DTOs.ConfiguracionPartidaDTO configuracion);

        /// <summary>
        /// Une a un usuario a una sala existente.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala.</param>
        /// <param name="nombreUsuario">Nombre del usuario que se une.</param>
        /// <returns>Informacion actualizada de la sala.</returns>
        Task<DTOs.SalaDTO> UnirseSalaAsync(string codigoSala, string nombreUsuario);

        /// <summary>
        /// Permite a un usuario abandonar una sala.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala.</param>
        /// <param name="nombreUsuario">Nombre del usuario que abandona.</param>
        Task AbandonarSalaAsync(string codigoSala, string nombreUsuario);

        /// <summary>
        /// Expulsa a un jugador de una sala.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala.</param>
        /// <param name="nombreHost">Nombre del host que expulsa.</param>
        /// <param name="nombreJugadorAExpulsar">Nombre del jugador a expulsar.</param>
        Task ExpulsarJugadorAsync(string codigoSala, string nombreHost, string nombreJugadorAExpulsar);

        /// <summary>
        /// Suscribe al cliente para recibir actualizaciones de la lista de salas.
        /// </summary>
        Task SuscribirListaSalasAsync();

        /// <summary>
        /// Cancela la suscripcion a actualizaciones de la lista de salas.
        /// </summary>
        Task CancelarSuscripcionListaSalasAsync();

        /// <summary>
        /// Obtiene la lista actual de salas disponibles.
        /// </summary>
        IReadOnlyList<DTOs.SalaDTO> ListaSalasActual { get; }
    }
}
