using System;
using System.Windows;
using System.Windows.Input;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante
{
    public static class AvisoAyudante
    {
        private static Action<string> _accionMostrar = EjecutarMostrarReal;

        /// <summary>
        /// Permite cambiar la lógica de mostrar avisos (útil para Unit Tests).
        /// </summary>
        public static void DefinirMostrarAviso(Action<string> accion)
        {
            _accionMostrar = accion ?? EjecutarMostrarReal;
        }

        public static void Mostrar(string mensaje)
        {
            if (_accionMostrar == EjecutarMostrarReal)
            {
                if (Application.Current?.Dispatcher != null)
                {
                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        _accionMostrar(mensaje);
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => _accionMostrar(mensaje));
                    }
                }
            }
            else
            {
                _accionMostrar(mensaje);
            }
        }

        private static void EjecutarMostrarReal(string mensaje)
        {
            if (Application.Current == null) return; 

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