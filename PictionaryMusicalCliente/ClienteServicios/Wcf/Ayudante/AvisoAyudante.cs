using System.Windows;
using System.Windows.Input;

namespace PictionaryMusicalCliente.Utilidades
{
    public static class AvisoAyudante
    {
        public static void Mostrar(string mensaje)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                EjecutarMostrar(mensaje);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    EjecutarMostrar(mensaje);
                });
            }
        }

        private static void EjecutarMostrar(string mensaje)
        {
            Cursor cursorAnterior = Mouse.OverrideCursor;
            Mouse.OverrideCursor = null;

            try
            {
                new Avisos(mensaje).ShowDialog();
            }
            finally
            {
                Mouse.OverrideCursor = cursorAnterior;
            }
        }
    }
}