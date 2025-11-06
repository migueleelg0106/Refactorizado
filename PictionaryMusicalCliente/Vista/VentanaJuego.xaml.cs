using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente
{
    public partial class VentanaJuego : Window
    {
        private bool _juegoIniciado = false;
        private double _grosor = 6;
        private Color _color = Colors.Black;
        private bool _syncingToolUI = false;

        private DispatcherTimer _temporizador;

        public VentanaJuego()
        {
            InitializeComponent();
            
            _temporizador = new DispatcherTimer();
            _temporizador.Interval = TimeSpan.FromSeconds(5);
            _temporizador.Tick += OverlayTimer_Tick;
        }

        private void BotonInvitarCorreo(object sender, RoutedEventArgs e)
        {
            // de momento esta aqui para la prueba de la ventana pero esto ira cuando se implemente la funcion de expulsar jugador que es la que tenemos duda
            ExpulsionJugador expulsarJugador = new ExpulsionJugador();
            expulsarJugador.ShowDialog();
            // esto igual es momentaneo en lo que se hace lo de los botones dinamicos de esa lista
            InvitacionAmigos invitarAmigos = new InvitacionAmigos();
            invitarAmigos.ShowDialog();
        }

        private void BotonAjustes(object sender, RoutedEventArgs e)
        {
            AjustesPartida ajustesPartida = new AjustesPartida();
            ajustesPartida.Owner = this;
            ajustesPartida.ShowDialog();
        }

        private void BotonIniciarPartida(object sender, RoutedEventArgs e)
        {
            if (_juegoIniciado) return;
            _juegoIniciado = true;

            cuadriculaDibujo.Visibility = Visibility.Visible;
            SetTool(true);
            AplicarEstiloLapiz();
            ActualizarEraserShape();

            botonIniciarPartida.IsEnabled = false;
            botonIniciarPartida.Content = LangResources.Lang.partidaTextoPartidaEnCurso;
        }

        private void SetTool(bool isPencil)
        {
            if (ink == null) return;

            // Evita que cambiar IsChecked dispare Click/Checked en cascada
            _syncingToolUI = true;
            if (toggleBotonLapiz != null) toggleBotonLapiz.IsChecked = isPencil;
            if (toggleBotonBorrador != null) toggleBotonBorrador.IsChecked = !isPencil;
            _syncingToolUI = false;

            ink.EditingMode = isPencil
                ? InkCanvasEditingMode.Ink
                : InkCanvasEditingMode.EraseByPoint;

            if (isPencil) AplicarEstiloLapiz();
            else ActualizarEraserShape();
        }

        // Handlers de los ToggleButton basados en Click
        private void ToggleBotonLapiz_Click(object sender, RoutedEventArgs e)
        {
            if (_syncingToolUI) return;   // click provocado por SetTool
            SetTool(true);
        }

        private void ToggleBotonBorrador_Click(object sender, RoutedEventArgs e)
        {
            if (_syncingToolUI) return;   // click provocado por SetTool
            SetTool(false);
        }

        private void AplicarEstiloLapiz()
        {
            if (ink == null) return;
            ink.DefaultDrawingAttributes = new DrawingAttributes
            {
                Color = _color,
                Width = _grosor,
                Height = _grosor,
                FitToCurve = false,
                IgnorePressure = true
            };
        }

        private void ActualizarEraserShape()
        {
            if (ink == null) return;
            var size = Math.Max(1, _grosor);
            ink.EraserShape = new EllipseStylusShape(size, size);
        }

        private void Grosor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && double.TryParse(btn.Tag?.ToString(), out var nuevo))
            {
                _grosor = nuevo;
                if (ink?.EditingMode == InkCanvasEditingMode.Ink)
                    AplicarEstiloLapiz();
                else
                    ActualizarEraserShape();
            }
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string colorName)
            {
                _color = (Color)ColorConverter.ConvertFromString(colorName);
                SetTool(true);
                AplicarEstiloLapiz();
            }
        }

        private void BotonBorrar_Click(object sender, RoutedEventArgs e)
        {
            ink?.Strokes.Clear();
        }
        private void Dibujante_Click(object sender, RoutedEventArgs e)
        {
            OverlayAdivinador.Visibility = Visibility.Collapsed;
            OverlayDibujante.Visibility = Visibility.Visible;

            _temporizador.Stop();
            _temporizador.Start();
        }

        private void Adivinador_Click(object sender, RoutedEventArgs e)
        {
            OverlayDibujante.Visibility = Visibility.Collapsed;
            OverlayAdivinador.Visibility = Visibility.Visible;

            _temporizador.Stop();
            _temporizador.Start();
        }

        private void Overlay_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _temporizador.Stop();

            if (sender is UIElement element)
            {
                element.Visibility = Visibility.Collapsed;
            }
        }

        private void OverlayTimer_Tick(object sender, EventArgs e)
        {
            _temporizador.Stop();

            OverlayDibujante.Visibility = Visibility.Collapsed;
            OverlayAdivinador.Visibility = Visibility.Collapsed;
        }
    }
}
