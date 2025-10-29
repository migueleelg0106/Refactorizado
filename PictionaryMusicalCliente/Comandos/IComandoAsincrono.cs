using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Comandos
{
    /// <summary>
    /// Representa un comando que expone su operación principal de forma asincrónica.
    /// </summary>
    public interface IComandoAsincrono : IComandoNotificable
    {
        /// <summary>
        /// Ejecuta el comando de forma asincrónica con el parámetro proporcionado.
        /// </summary>
        /// <param name="parametro">Parámetro que se pasa al comando.</param>
        /// <returns>Tarea que representa la ejecución del comando.</returns>
        Task EjecutarAsync(object parametro);
    }
}
