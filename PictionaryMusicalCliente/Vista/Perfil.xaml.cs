using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
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

            IPerfilServicio perfilService = new PerfilServicio();
            IAvatarServicio avatarService = new AvatarServicio();
            ISeleccionarAvatarServicio seleccionarAvatarService = new SeleccionarAvatarDialogoServicio(avatarService);
            ICambioContrasenaServicio cambioContrasenaService = new CambioContrasenaServicio();
            IVerificarCodigoDialogoServicio verificarCodigoDialogService = new VerificarCodigoDialogoServicio();
            IRecuperacionCuentaServicio recuperacionCuentaDialogService =
                new RecuperacionCuentaDialogoServicio(verificarCodigoDialogService);

            var vistaModelo = new PerfilVistaModelo(
                perfilService,
                seleccionarAvatarService,
                cambioContrasenaService,
                recuperacionCuentaDialogService,
                avatarService)
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
            ControlVisual.RestablecerEstadoCampo(bloqueTextoNombre);
            ControlVisual.RestablecerEstadoCampo(bloqueTextoApellido);

            if (camposInvalidos == null)
            {
                return;
            }

            if (camposInvalidos.Contains(nameof(PerfilVistaModelo.Nombre)))
            {
                ControlVisual.MarcarCampoInvalido(bloqueTextoNombre);
            }

            if (camposInvalidos.Contains(nameof(PerfilVistaModelo.Apellido)))
            {
                ControlVisual.MarcarCampoInvalido(bloqueTextoApellido);
            }
        }
    }
}
