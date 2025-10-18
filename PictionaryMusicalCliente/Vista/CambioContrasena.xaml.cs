using System.Windows;
using System.Windows.Controls;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class CambioContrasena : Window
    {
        public CambioContrasena()
        {
            InitializeComponent();
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
    }
}
