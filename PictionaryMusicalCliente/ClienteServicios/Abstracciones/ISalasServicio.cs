using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Gestiona la logica de conexion, administracion y eventos en tiempo real de las salas de 
    /// juego.
    /// </summary>
    public interface ISalasServicio : IDisposable
    {
        /// <summary>
        /// Notifica cuando un nuevo participante ingresa a la sala actual.
        /// </summary>
        event EventHandler<string> JugadorSeUnio;

        /// <summary>
        /// Notifica cuando un participante abandona voluntariamente la sala actual.
        /// </summary>
        event EventHandler<string> JugadorSalio;

        /// <summary>
        /// Notifica cuando un participante es removido forzosamente por el anfitrion.
        /// </summary>
        event EventHandler<string> JugadorExpulsado;

        /// <summary>
        /// Notifica cuando la sala es cancelada por la salida del anfitrion.
        /// </summary>
        event EventHandler<string> SalaCancelada;

        /// <summary>
        /// Se dispara cuando cambia el listado global de salas disponibles en el servidor.
        /// </summary>
        event EventHandler<IReadOnlyList<DTOs.SalaDTO>> ListaSalasActualizada;

        /// <summary>
        /// Notifica cambios en la configuracion o estado de la sala en la que se encuentra el 
        /// usuario.
        /// </summary>
        event EventHandler<DTOs.SalaDTO> SalaActualizada;

        /// <summary>
        /// Solicita la creacion de una nueva sala con las reglas especificadas.
        /// </summary>
        /// <param name="nombreCreador">Identificador del usuario que sera anfitrion.</param>
        /// <param name="configuracion">Reglas y parametros de la partida.</param>
        /// <returns>La informacion de la sala creada.</returns>
        Task<DTOs.SalaDTO> CrearSalaAsync(
            string nombreCreador,
            DTOs.ConfiguracionPartidaDTO configuracion);

        /// <summary>
        /// Intenta agregar al usuario actual a una sala existente.
        /// </summary>
        /// <param name="codigoSala">Codigo unico de la sala.</param>
        /// <param name="nombreUsuario">Identificador del usuario que se une.</param>
        /// <returns>La informacion actual de la sala.</returns>
        Task<DTOs.SalaDTO> UnirseSalaAsync(string codigoSala, string nombreUsuario);

        /// <summary>
        /// Desconecta al usuario de la sala actual y notifica a los demas integrantes.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala a abandonar.</param>
        /// <param name="nombreUsuario">Usuario que sale.</param>
        Task AbandonarSalaAsync(string codigoSala, string nombreUsuario);

        /// <summary>
        /// Elimina a un jugador de la sala por decision del anfitrion.
        /// </summary>
        /// <param name="codigoSala">Codigo de la sala.</param>
        /// <param name="nombreHost">Nombre del anfitrion que ejecuta la accion.</param>
        /// <param name="nombreJugadorAExpulsar">Jugador objetivo a eliminar.</param>
        Task ExpulsarJugadorAsync(
            string codigoSala,
            string nombreHost,
            string nombreJugadorAExpulsar);

        /// <summary>
        /// Inicia la recepcion de actualizaciones sobre las salas publicas disponibles.
        /// </summary>
        Task SuscribirListaSalasAsync();

        /// <summary>
        /// Detiene la recepcion de actualizaciones del listado de salas.
        /// </summary>
        Task CancelarSuscripcionListaSalasAsync();

        /// <summary>
        /// Obtiene la ultima lista conocida de salas disponibles.
        /// </summary>
        IReadOnlyList<DTOs.SalaDTO> ListaSalasActual { get; }
    }
}