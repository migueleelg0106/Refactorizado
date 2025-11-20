using System;
using System.Windows.Input;

namespace PictionaryMusicalCliente.Comandos
{
    /// <summary>
    /// Implementación básica de <see cref="ICommand"/> que permite enlazar acciones
    /// desde la vista modelo hacia la vista.
    /// </summary>
    public class ComandoDelegado : IComandoNotificable
    {
        private readonly Action<object> _ejecutar;
        private readonly Predicate<object> _puedeEjecutar;

        /// <summary>
        /// Inicializa una nueva instancia del comando con acciones sin parámetros.
        /// </summary>
        /// <param name="ejecutar">Acción a ejecutar.</param>
        /// <param name="puedeEjecutar">Función opcional para determinar validacion.</param>
        public ComandoDelegado(Action ejecutar, Func<bool> puedeEjecutar = null)
            : this(
                  ejecutar != null ? new Action<object>(_ => ejecutar()) : null,
                  puedeEjecutar != null ? new Predicate<object>(_ => puedeEjecutar()) : null)
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia del comando.
        /// </summary>
        /// <param name="ejecutar">Acción a ejecutar.</param>
        /// <param name="puedeEjecutar">Función opcional para determinar validacion.</param>
        public ComandoDelegado(
            Action<object> ejecutar,
            Predicate<object> puedeEjecutar = null)
        {
            _ejecutar = ejecutar ?? throw new ArgumentNullException(nameof(ejecutar));
            _puedeEjecutar = puedeEjecutar;
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            return _puedeEjecutar?.Invoke(parameter) ?? true;
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            _ejecutar(parameter);
        }

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <inheritdoc />
        public void NotificarPuedeEjecutar()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}