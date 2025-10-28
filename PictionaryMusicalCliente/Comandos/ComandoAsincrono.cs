using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PictionaryMusicalCliente.Comandos
{
    /// <summary>
    /// Implementación de comando que coordina la ejecución de operaciones asincrónicas.
    /// </summary>
    public class ComandoAsincrono : IComandoAsincrono
    {
        private readonly Func<object, Task> _ejecutarAsincrono;
        private readonly Predicate<object> _puedeEjecutar;
        private bool _estaEjecutando;

        /// <summary>
        /// Inicializa una nueva instancia del comando asincrónico.
        /// </summary>
        /// <param name="ejecutarAsincrono">Función que representa la ejecución del comando.</param>
        /// <param name="puedeEjecutar">Función opcional para determinar si el comando puede ejecutarse.</param>
        public ComandoAsincrono(Func<Task> ejecutarAsincrono, Func<bool> puedeEjecutar = null)
            : this(ejecutarAsincrono != null ? new Func<object, Task>(_ => ejecutarAsincrono()) : null,
                  puedeEjecutar != null ? new Predicate<object>(_ => puedeEjecutar()) : null)
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia del comando asincrónico con acceso al parámetro.
        /// </summary>
        /// <param name="ejecutarAsincrono">Función que representa la ejecución del comando.</param>
        /// <param name="puedeEjecutar">Función opcional para determinar si el comando puede ejecutarse.</param>
        public ComandoAsincrono(Func<object, Task> ejecutarAsincrono, Predicate<object> puedeEjecutar = null)
        {
            _ejecutarAsincrono = ejecutarAsincrono ?? throw new ArgumentNullException(nameof(ejecutarAsincrono));
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
            await EjecutarAsincrono(parameter);
        }

        /// <inheritdoc />
        public async Task EjecutarAsincrono(object parametro)
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
            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => NotificarPuedeEjecutar()));
                return;
            }

            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
