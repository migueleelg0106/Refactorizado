using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PictionaryMusicalCliente.Comandos
{
    /// <summary>
    /// Implementacion de comando que coordina la ejecucion de operaciones asincronicas.
    /// </summary>
    public class ComandoAsincrono : IComandoAsincrono
    {
        private readonly Func<object, Task> _ejecutarAsincrono;
        private readonly Predicate<object> _puedeEjecutar;
        private bool _estaEjecutando;

        /// <summary>
        /// Inicializa una nueva instancia del comando asincronico.
        /// </summary>
        /// <param name="ejecutarAsincrono">Funcion que representa la ejecucion del comando.
        /// </param>
        /// <param name="puedeEjecutar">Funcion opcional para validar ejecucion.</param>
        public ComandoAsincrono(Func<Task> ejecutarAsincrono, Func<bool> puedeEjecutar = null)
            : this(
                  ejecutarAsincrono != null ? new Func<object, Task>(_ => ejecutarAsincrono()) 
                    : null,
                  puedeEjecutar != null ? new Predicate<object>(_ => puedeEjecutar()) : null)
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia del comando asincronico con acceso al parametro.
        /// </summary>
        /// <param name="ejecutarAsincrono">Funcion que representa la ejecucion del comando.
        /// </param>
        /// <param name="puedeEjecutar">Funcion opcional para validar ejecucion.</param>
        public ComandoAsincrono(
            Func<object, Task> ejecutarAsincrono,
            Predicate<object> puedeEjecutar = null)
        {
            _ejecutarAsincrono = ejecutarAsincrono ??
                throw new ArgumentNullException(nameof(ejecutarAsincrono));
            _puedeEjecutar = puedeEjecutar;
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            if (_estaEjecutando)
            {
                return false;
            }

            return _puedeEjecutar?.Invoke(parameter) ?? true;
        }

        /// <inheritdoc />
        public async void Execute(object parameter)
        {
            await EjecutarAsync(parameter);
        }

        /// <inheritdoc />
        public async Task EjecutarAsync(object parametro)
        {
            if (!CanExecute(parametro))
            {
                return;
            }

            try
            {
                _estaEjecutando = true;
                NotificarPuedeEjecutar();
                await _ejecutarAsincrono(parametro).ConfigureAwait(true);
            }
            finally
            {
                _estaEjecutando = false;
                NotificarPuedeEjecutar();
            }
        }

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged;

        /// <inheritdoc />
        public void NotificarPuedeEjecutar()
        {
            if (Application.Current?.Dispatcher != null &&
                !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => NotificarPuedeEjecutar()));
                return;
            }

            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            CommandManager.InvalidateRequerySuggested();
        }
    }
}