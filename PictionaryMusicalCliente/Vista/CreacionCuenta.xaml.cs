using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.InicioSesion;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Logica de interaccion para la ventana de creacion de una nueva cuenta de usuario.
    /// Gestiona la visualizacion de errores y la comunicacion con el ViewModel.
    /// </summary>
    public partial class CreacionCuenta : Window
    {
        /// <summary>
        /// Inicializa una nueva instancia de la ventana de Creacion de Cuenta.
        /// Configura el evento de cambio de contexto de datos para inyectar dependencias visuales.
        /// </summary>
        public CreacionCuenta()
        {
            InitializeComponent();
            DataContextChanged += AlCambiarContextoDeDatosCreacionCuenta;
        }

        private void AlCambiarContextoDeDatosCreacionCuenta(
            object remitente,
            DependencyPropertyChangedEventArgs argumentosEvento)
        {
            if (argumentosEvento.NewValue is CreacionCuentaVistaModelo vistaModelo)
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

        private void AlCambiarContrasena(object remitente, RoutedEventArgs argumentosEvento)
        {
            if (remitente is PasswordBox cajaContrasena && 
                DataContext is CreacionCuentaVistaModelo vistaModelo)
            {
                vistaModelo.Contrasena = cajaContrasena.Password;
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