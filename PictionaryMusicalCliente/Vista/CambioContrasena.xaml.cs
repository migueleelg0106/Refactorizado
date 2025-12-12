using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Perfil;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana para gestionar el cambio de contrasena del usuario.
    /// </summary>
    public partial class CambioContrasena : Window
    {
        /// <summary>
        /// Constructor por defecto. VentanaServicio asigna el DataContext.
        /// </summary>
        public CambioContrasena()
        {
            InitializeComponent();
            
            if (DataContext is CambioContrasenaVistaModelo vistaModelo)
            {
                vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
            }
            
            DataContextChanged += CambioContrasena_DataContextChanged;
        }

        private void CambioContrasena_DataContextChanged(object sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is CambioContrasenaVistaModelo vistaModelo)
            {
                vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
            }
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