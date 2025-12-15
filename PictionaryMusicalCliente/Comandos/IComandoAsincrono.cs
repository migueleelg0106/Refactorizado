using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Comandos
{
    /// <summary>
    /// Representa un comando que expone su operacion principal de forma asincronica.
    /// </summary>
    public interface IComandoAsincrono : IComandoNotificable
    {
        /// <summary>
        /// Ejecuta el comando de forma asincronica con el parametro proporcionado.
        /// </summary>
        /// <param name="parametro">Parametro que se pasa al comando.</param>
        /// <returns>Tarea que representa la ejecucion del comando.</returns>
        Task EjecutarAsync(object parametro);
    }
}