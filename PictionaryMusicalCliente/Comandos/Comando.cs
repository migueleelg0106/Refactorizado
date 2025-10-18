using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Comandos
{
    public sealed class Comando : ICommand
    {
        private readonly Action _ejecutar;
        private readonly Func<bool> _puedeEjecutar;
        
        public Comando(Action ejecutar, Func<bool> puedeEjecutar = null)
        {
            _ejecutar = ejecutar ?? throw new ArgumentNullException(nameof(ejecutar));
            _puedeEjecutar = puedeEjecutar;
        }

        public bool CanExecute(object parameter)
        {
            return _puedeEjecutar?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _ejecutar();
        }

        public event EventHandler CanExecuteChanged;
        public void NotificarPuedeEjecutarCambio()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
