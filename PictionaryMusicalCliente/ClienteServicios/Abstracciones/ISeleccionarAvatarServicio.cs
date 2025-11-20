using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Gestiona la logica de seleccion y recuperacion de recursos graficos para el perfil.
    /// </summary>
    public interface ISeleccionarAvatarServicio
    {
        /// <summary>
        /// Recupera el objeto de avatar correspondiente al identificador seleccionado.
        /// </summary>
        /// <param name="idAvatar">Identificador numerico del avatar.</param>
        /// <returns>El objeto de dominio con los datos del avatar.</returns>
        Task<ObjetoAvatar> SeleccionarAvatarAsync(int idAvatar);
    }
}