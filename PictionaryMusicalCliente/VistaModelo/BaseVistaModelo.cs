using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PictionaryMusicalCliente.VistaModelo
{
    public abstract class BaseVistaModelo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool EstablecerPropiedad<T>(ref T campo, T valor, [CallerMemberName] string nombrePropiedad = null)
        {
            if (Equals(campo, valor))
            {
                return false;
            }

            campo = valor;
            NotificarCambio(nombrePropiedad);
            return true;
        }

        protected void NotificarCambio([CallerMemberName] string nombrePropiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}
