using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Perfil;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana para gestionar el cambio de contraseña del usuario.
    /// </summary>
    public partial class CambioContrasena : Window
    {
        /// <summary>
        /// Inicializa la ventana.
        /// </summary>
        public CambioContrasena()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Asigna la vista modelo y configura los delegados de interacción visual.
        /// </summary>
        /// <param name="vistaModelo">La lógica de negocio para el cambio de contraseña.</param>
        public void ConfigurarVistaModelo(CambioContrasenaVistaModelo vistaModelo)
        {
            if (vistaModelo == null)
            {
                return;
            }

            vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
            DataContext = vistaModelo;
        }

        private void ContrasenaNuevaPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CambioContrasenaVistaModelo vistaModelo &&
                sender is PasswordBox passwordBox)
            {
                vistaModelo.NuevaContrasena = passwordBox.Password;
            }
        }

        private void ContrasenaConfirmacionPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CambioContrasenaVistaModelo vistaModelo &&
                sender is PasswordBox passwordBox)
            {
                vistaModelo.ConfirmacionContrasena = passwordBox.Password;
            }
        }

        private void MarcarCamposInvalidos(IList<string> camposInvalidos)
        {
            ControlVisual.RestablecerEstadoCampo(campoContrasenaNueva);
            ControlVisual.RestablecerEstadoCampo(campoContrasenaConfirmacion);

            if (camposInvalidos == null)
            {
                return;
            }

            if (camposInvalidos.Contains(nameof(CambioContrasenaVistaModelo.NuevaContrasena)))
            {
                ControlVisual.MarcarCampoInvalido(campoContrasenaNueva);
            }

            if (camposInvalidos.Contains(
                nameof(CambioContrasenaVistaModelo.ConfirmacionContrasena)))
            {
                ControlVisual.MarcarCampoInvalido(campoContrasenaConfirmacion);
            }
        }
    }
}