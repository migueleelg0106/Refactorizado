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
            
            DataContextChanged += AlCambiarContextoDeDatosCambioContrasena;
        }

        private void AlCambiarContextoDeDatosCambioContrasena(object remitente,
            DependencyPropertyChangedEventArgs argumentosEvento)
        {
            if (argumentosEvento.NewValue is CambioContrasenaVistaModelo vistaModelo)
            {
                vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
            }
        }

        private void AlCambiarContrasenaNueva(object remitente, RoutedEventArgs argumentosEvento)
        {
            if (DataContext is CambioContrasenaVistaModelo vistaModelo &&
                remitente is PasswordBox cajaContrasena)
            {
                vistaModelo.NuevaContrasena = cajaContrasena.Password;
            }
        }

        private void AlCambiarContrasenaConfirmacion(object remitente, 
            RoutedEventArgs argumentosEvento)
        {
            if (DataContext is CambioContrasenaVistaModelo vistaModelo &&
                remitente is PasswordBox cajaContrasena)
            {
                vistaModelo.ConfirmacionContrasena = cajaContrasena.Password;
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