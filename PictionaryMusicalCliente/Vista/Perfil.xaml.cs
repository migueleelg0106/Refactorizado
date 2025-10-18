using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class Perfil : Window
    {
        public Perfil()
        {
            InitializeComponent();

            IPerfilService perfilService = new PerfilService();
            ISeleccionarAvatarService seleccionarAvatarService = new SeleccionarAvatarDialogService();
            ICambioContrasenaService cambioContrasenaService = new CambioContrasenaService();
            IVerificarCodigoDialogService verificarCodigoDialogService = new VerificarCodigoDialogService();
            IRecuperacionCuentaDialogService recuperacionCuentaDialogService =
                new RecuperacionCuentaDialogService(verificarCodigoDialogService);

            var vistaModelo = new PerfilVistaModelo(
                perfilService,
                seleccionarAvatarService,
                cambioContrasenaService,
                recuperacionCuentaDialogService)
            {
                CerrarAccion = Close
            };

            vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;

            DataContext = vistaModelo;
        }

        private async void Perfil_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PerfilVistaModelo vistaModelo)
            {
                await vistaModelo.CargarPerfilAsync().ConfigureAwait(true);
            }
        }

        private void PopupRedSocial_Opened(object sender, EventArgs e)
        {
            if (sender is Popup popup && popup.Child is Border border && border.Child is TextBox textBox)
            {
                textBox.Focus();
                textBox.CaretIndex = textBox.Text?.Length ?? 0;
            }
        }

        private void RedSocialTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is ToggleButton toggle)
            {
                if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape)
                {
                    toggle.IsChecked = false;
                    e.Handled = true;
                }
            }
        }

        private void MarcarCamposInvalidos(IList<string> camposInvalidos)
        {
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoNombre);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoApellido);

            if (camposInvalidos == null)
            {
                return;
            }

            if (camposInvalidos.Contains(nameof(PerfilVistaModelo.Nombre)))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoNombre);
            }

            if (camposInvalidos.Contains(nameof(PerfilVistaModelo.Apellido)))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoApellido);
            }
        }
    }
}
