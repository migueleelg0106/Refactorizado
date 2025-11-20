using System;
using System.Windows;
using System.Windows.Input;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante
{
    /// <summary>
    /// Provee mecanismos para mostrar mensajes al usuario de manera segura en hilos de UI.
    /// Permite la inyeccion de dependencias para pruebas unitarias.
    /// </summary>
    public static class AvisoAyudante
    {
        private static Action<string> _accionMostrar = EjecutarMostrarReal;

        /// <summary>
        /// Permite cambiar la logica de mostrar avisos (util para Unit Tests).
        /// </summary>
        /// <param name="accion">La accion delegada que reemplazara la logica de UI.</param>
        public static void DefinirMostrarAviso(Action<string> accion)
        {
            _accionMostrar = accion ?? EjecutarMostrarReal;
        }

        /// <summary>
        /// Muestra un mensaje al usuario asegurando que se ejecute en el hilo principal.
        /// </summary>
        /// <param name="mensaje">El contenido del aviso a mostrar.</param>
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
            if (Application.Current == null)
            {
                return;
            }

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