using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class CambioContrasena : Window
    {
        public CambioContrasena()
        {
            InitializeComponent();
        }

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
            if (DataContext is CambioContrasenaVistaModelo vistaModelo && sender is PasswordBox passwordBox)
            {
                vistaModelo.NuevaContrasena = passwordBox.Password;
            }
        }

        private void ContrasenaConfirmacionPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CambioContrasenaVistaModelo vistaModelo && sender is PasswordBox passwordBox)
            {
                vistaModelo.ConfirmacionContrasena = passwordBox.Password;
            }
        }

        private void MarcarCamposInvalidos(IList<string> camposInvalidos)
        {
            ControlVisual.RestablecerEstadoCampo(bloqueContrasenaNueva);
            ControlVisual.RestablecerEstadoCampo(bloqueContrasenaConfirmacion);

            if (camposInvalidos == null)
            {
                return;
            }

            if (camposInvalidos.Contains(nameof(CambioContrasenaVistaModelo.NuevaContrasena)))
            {
                ControlVisual.MarcarCampoInvalido(bloqueContrasenaNueva);
            }

            if (camposInvalidos.Contains(nameof(CambioContrasenaVistaModelo.ConfirmacionContrasena)))
            {
                ControlVisual.MarcarCampoInvalido(bloqueContrasenaConfirmacion);
            }
        }
    }
}
