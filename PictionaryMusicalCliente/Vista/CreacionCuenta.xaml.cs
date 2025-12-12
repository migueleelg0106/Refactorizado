using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.InicioSesion;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PictionaryMusicalCliente.Vista
{
    public partial class CreacionCuenta : Window
    {
        public CreacionCuenta()
        {
            InitializeComponent();
            DataContextChanged += CreacionCuenta_DataContextChanged;
        }

        private void CreacionCuenta_DataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is CreacionCuentaVistaModelo vistaModelo)
            {
                vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
                vistaModelo.MostrarMensaje = mensaje =>
                {
                    if (!string.IsNullOrWhiteSpace(mensaje))
                    {
                        MessageBox.Show(mensaje, Title, MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                };
            }
        }

        private void PasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && 
                DataContext is CreacionCuentaVistaModelo vistaModelo)
            {
                vistaModelo.Contrasena = passwordBox.Password;
            }
        }

        private void MarcarCamposInvalidos(IList<string> camposInvalidos)
        {
            ControlVisual.RestablecerEstadoCampo(campoTextoUsuario);
            ControlVisual.RestablecerEstadoCampo(campoTextoNombre);
            ControlVisual.RestablecerEstadoCampo(campoTextoApellido);
            ControlVisual.RestablecerEstadoCampo(campoTextoCorreo);
            ControlVisual.RestablecerEstadoCampo(campoContrasenaContrasena);

            if (camposInvalidos == null) return;

            if (DataContext is CreacionCuentaVistaModelo vistaModelo)
            {
                if (camposInvalidos.Contains(nameof(vistaModelo.Usuario)))
                {
                    ControlVisual.MarcarCampoInvalido(campoTextoUsuario);
                }

                if (camposInvalidos.Contains(nameof(vistaModelo.Nombre)))
                {
                    ControlVisual.MarcarCampoInvalido(campoTextoNombre);
                }

                if (camposInvalidos.Contains(nameof(vistaModelo.Apellido)))
                {
                    ControlVisual.MarcarCampoInvalido(campoTextoApellido);
                }

                if (camposInvalidos.Contains(nameof(vistaModelo.Correo)))
                {
                    ControlVisual.MarcarCampoInvalido(campoTextoCorreo);
                }

                if (camposInvalidos.Contains(nameof(vistaModelo.Contrasena)))
                {
                    ControlVisual.MarcarCampoInvalido(campoContrasenaContrasena);
                }
            }
        }
    }
}