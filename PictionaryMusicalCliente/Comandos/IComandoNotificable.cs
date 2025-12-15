using System.Windows.Input;

namespace PictionaryMusicalCliente.Comandos
{
    /// <summary>
    /// Define la interfaz base para los comandos que permiten notificar cambios
    /// en la disponibilidad de ejecucion.
    /// </summary>
    public interface IComandoNotificable : ICommand
    {
        /// <summary>
        /// Fuerza la reevaluacion del estado de ejecucion del comando.
        /// </summary>
        void NotificarPuedeEjecutar();
    }
}