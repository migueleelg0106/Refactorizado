using PictionaryMusicalCliente.VistaModelo.Salas;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// UserControl para el chat de la sala.
    /// </summary>
    public partial class Chat : UserControl
    {
        public Chat()
        {
            InitializeComponent();
            Loaded += AlCargarChat;
        }

        private void AlCargarChat(object sender, RoutedEventArgs e)
        {
            if (DataContext is SalaVistaModelo vistaModelo)
            {
                vistaModelo.MensajeChatRecibido += AlRecibirMensajeChat;
                vistaModelo.MensajeDoradoRecibido += AlRecibirMensajeDorado;
            }

            Unloaded += AlDescargarChat;
        }

        private void AlDescargarChat(object sender, RoutedEventArgs e)
        {
            if (DataContext is SalaVistaModelo vistaModelo)
            {
                vistaModelo.MensajeChatRecibido -= AlRecibirMensajeChat;
                vistaModelo.MensajeDoradoRecibido -= AlRecibirMensajeDorado;
            }
        }

        private void AlPresionarTeclaEnCampoTextoChat(object remitente, KeyEventArgs argumentosEvento)
        {
            if ((argumentosEvento.Key == Key.Enter || argumentosEvento.Key == Key.Return) && DataContext is SalaVistaModelo vistaModelo)
            {
                if (vistaModelo.EnviarMensajeChatComando?.CanExecute(null) == true)
                {
                    vistaModelo.EnviarMensajeChatComando.Execute(null);
                }

                argumentosEvento.Handled = true;
            }
        }

        private void AlRecibirMensajeChat(string nombreJugador, string mensaje)
        {
            AgregarMensajeAlChat(nombreJugador, mensaje, Colors.Black);
        }

        private void AlRecibirMensajeDorado(string nombreJugador, string mensaje)
        {
            AgregarMensajeAlChat(nombreJugador, mensaje, Colors.Goldenrod);
        }

        private void AgregarMensajeAlChat(string nombreJugador, string mensaje, Color color)
        {
            if (panelApilableChat == null)
            {
                return;
            }

            string texto = string.IsNullOrWhiteSpace(nombreJugador)
                ? mensaje
                : $"{nombreJugador}: {mensaje}";

            var textoBloque = new TextBlock
            {
                Text = texto,
                Foreground = new SolidColorBrush(color),
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Comic Sans MS"),
                Margin = new Thickness(0, 2, 0, 2)
            };

            panelApilableChat.Children.Add(textoBloque);

            desplazamientoChat?.ScrollToEnd();
        }
    }
}
