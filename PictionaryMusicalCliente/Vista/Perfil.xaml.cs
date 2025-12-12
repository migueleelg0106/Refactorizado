using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PictionaryMusicalCliente.Vista
{
    public partial class Perfil : Window
    {
        public Perfil()
        {
            InitializeComponent();
            DataContextChanged += Perfil_DataContextChanged;
        }

        private void Perfil_DataContextChanged(
            object sender, 
            DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is PerfilVistaModelo vistaModelo)
            {
                vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
                Loaded += Perfil_LoadedAsync;
                Closed += Perfil_Closed;
            }
        }

        private async void Perfil_LoadedAsync(object sender, RoutedEventArgs e)
        {
            if (DataContext is PerfilVistaModelo vistaModelo)
            {
                await vistaModelo.CargarPerfilAsync().ConfigureAwait(true);
            }
        }

        private void Perfil_Closed(object sender, EventArgs e)
        {
            if (DataContext is PerfilVistaModelo vistaModelo && 
                vistaModelo.RequiereReinicioSesion)
            {
                Application.Current.Shutdown();
            }
        }

        private void PopupRedSocial_Opened(object sender, EventArgs e)
        {
            if (sender is Popup popup &&
                popup.Child is Border border &&
                border.Child is TextBox textBox)
            {
                textBox.Focus();
                textBox.CaretIndex = textBox.Text?.Length ?? 0;
            }
        }

        private void RedSocialTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox &&
                textBox.Tag is ToggleButton toggle &&
                (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape))
            {
                toggle.IsChecked = false;
                e.Handled = true;
            }
        }

        private void MarcarCamposInvalidos(IList<string> camposInvalidos)
        {
            ControlVisual.RestablecerEstadoCampo(campoTextoNombre);
            ControlVisual.RestablecerEstadoCampo(campoTextoApellido);

            if (camposInvalidos == null) return;

            if (camposInvalidos.Contains(nameof(PerfilVistaModelo.Nombre)))
            {
                ControlVisual.MarcarCampoInvalido(campoTextoNombre);
            }

            if (camposInvalidos.Contains(nameof(PerfilVistaModelo.Apellido)))
            {
                ControlVisual.MarcarCampoInvalido(campoTextoApellido);
            }
        }
    }
}