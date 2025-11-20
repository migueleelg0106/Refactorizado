using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Administra la visualizacion y actualizacion en tiempo real de la lista de amigos.
    /// </summary>
    public interface IListaAmigosServicio : IDisposable
    {
        /// <summary>
        /// Notifica a la interfaz grafica cuando hay cambios en la lista de conectados o estados.
        /// </summary>
        event EventHandler<IReadOnlyList<DTOs.AmigoDTO>> ListaActualizada;

        /// <summary>
        /// Obtiene la coleccion local actual de amigos sin realizar una peticion al servidor.
        /// </summary>
        IReadOnlyList<DTOs.AmigoDTO> ListaActual { get; }

        /// <summary>
        /// Conecta al cliente para recibir actualizaciones de estado de los amigos del usuario.
        /// </summary>
        /// <param name="nombreUsuario">El identificador del usuario local.</param>
        Task SuscribirAsync(string nombreUsuario);

        /// <summary>
        /// Cierra la conexion de actualizaciones en tiempo real.
        /// </summary>
        /// <param name="nombreUsuario">El identificador del usuario local.</param>
        Task CancelarSuscripcionAsync(string nombreUsuario);

        /// <summary>
        /// Recupera la lista completa de amigos desde el servidor.
        /// </summary>
        /// <param name="nombreUsuario">Usuario del cual consultar los amigos.</param>
        /// <returns>Una lista de lectura con la informacion de los amigos.</returns>
        Task<IReadOnlyList<DTOs.AmigoDTO>> ObtenerAmigosAsync(string nombreUsuario);
    }
}