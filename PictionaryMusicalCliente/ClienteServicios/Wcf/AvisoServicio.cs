using System.Windows;
using System.Windows.Input;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Vista;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Provee mecanismos para mostrar mensajes al usuario de manera segura en hilos de UI.
    /// Permite la inyeccion de dependencias para pruebas unitarias.
    /// </summary>
    public class AvisoServicio : IAvisoServicio
    {
        /// <summary>
        /// Muestra un mensaje al usuario asegurando que se ejecute en el hilo principal.
        /// </summary>
        /// <param name="mensaje">El contenido del aviso a mostrar.</param>
        public void Mostrar(string mensaje)
        {
            if (Application.Current?.Dispatcher == null)
            {
                return;
            }

            if (Application.Current.Dispatcher.CheckAccess())
            {
                EjecutarMostrarReal(mensaje);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => EjecutarMostrarReal(mensaje));
            }
        }

        private void EjecutarMostrarReal(string mensaje)
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