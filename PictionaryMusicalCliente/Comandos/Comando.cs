using System;
using System.Windows.Input;

namespace PictionaryMusicalCliente.Comandos
{
    /// <summary>
    /// Implementacion ligera de ICommand para acciones sincronas simples.
    /// </summary>
    public sealed class Comando : ICommand
    {
        private readonly Action _ejecutar;
        private readonly Func<bool> _puedeEjecutar;

        /// <summary>
        /// Crea una instancia del comando.
        /// </summary>
        /// <param name="ejecutar">Accion a realizar.</param>
        /// <param name="puedeEjecutar">Funcion que determina si se puede ejecutar.</param>
        public Comando(Action ejecutar, Func<bool> puedeEjecutar = null)
        {
            _ejecutar = ejecutar ?? throw new ArgumentNullException(nameof(ejecutar));
            _puedeEjecutar = puedeEjecutar;
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            return _puedeEjecutar?.Invoke() ?? true;
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            _ejecutar();
        }

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Dispara manualmente el evento de cambio de estado.
        /// </summary>
        public void NotificarPuedeEjecutarCambio()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}