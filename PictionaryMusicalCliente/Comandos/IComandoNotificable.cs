using System.Windows.Input;

namespace PictionaryMusicalCliente.Comandos
{
    /// <summary>
    /// Define la interfaz base para los comandos que permiten notificar cambios
    /// en la disponibilidad de ejecución.
    /// </summary>
    public interface IComandoNotificable : ICommand
    {
        /// <summary>
        /// Fuerza la reevaluación del estado de ejecución del comando.
        /// </summary>
        void NotificarPuedeEjecutar();
    }
}
