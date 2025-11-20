using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de seleccion de avatar mediante dialogo.
    /// </summary>
    public interface ISeleccionarAvatarServicio
    {
        /// <summary>
        /// Muestra el dialogo de seleccion de avatar.
        /// </summary>
        /// <param name="idAvatar">Identificador del avatar actual.</param>
        /// <returns>Avatar seleccionado o null si se cancelo.</returns>
        Task<ObjetoAvatar> SeleccionarAvatarAsync(int idAvatar);
    }
}
