using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de gestion de lista de amigos.
    /// </summary>
    public interface IListaAmigosServicio : IDisposable
    {
        /// <summary>
        /// Evento que se dispara cuando la lista de amigos se actualiza.
        /// </summary>
        event EventHandler<IReadOnlyList<DTOs.AmigoDTO>> ListaActualizada;

        /// <summary>
        /// Obtiene la lista actual de amigos.
        /// </summary>
        IReadOnlyList<DTOs.AmigoDTO> ListaActual { get; }

        /// <summary>
        /// Suscribe al usuario para recibir actualizaciones de su lista de amigos.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario a suscribir.</param>
        Task SuscribirAsync(string nombreUsuario);

        /// <summary>
        /// Cancela la suscripcion a actualizaciones de la lista de amigos.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        Task CancelarSuscripcionAsync(string nombreUsuario);

        /// <summary>
        /// Obtiene la lista de amigos de un usuario.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        /// <returns>Lista de amigos.</returns>
        Task<IReadOnlyList<DTOs.AmigoDTO>> ObtenerAmigosAsync(string nombreUsuario);
    }
}
