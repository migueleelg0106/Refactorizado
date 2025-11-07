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

        private DispatcherTimer _overlayTimer;
        private DispatcherTimer _temporizador; // <-- nuevo timer para el contador
        private int _contador = 30;

        public VentanaJuego()
        {
            InitializeComponent();

            // Timer de overlays
            _overlayTimer = new DispatcherTimer();
            _overlayTimer.Interval = TimeSpan.FromSeconds(5);
            _overlayTimer.Tick += OverlayTimer_Tick;

            // Timer del contador
            _temporizador = new DispatcherTimer();
            _temporizador.Interval = TimeSpan.FromSeconds(1);
            _temporizador.Tick += Temporizador_Tick;
        }

        private void BotonInvitarCorreo(object sender, RoutedEventArgs e)
        {
            ExpulsionJugador expulsarJugador = new ExpulsionJugador();
            expulsarJugador.ShowDialog();

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

        private void ToggleBotonLapiz_Click(object sender, RoutedEventArgs e)
        {
            if (_syncingToolUI) return;
            SetTool(true);
        }

        private void ToggleBotonBorrador_Click(object sender, RoutedEventArgs e)
        {
            if (_syncingToolUI) return;
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

            _overlayTimer.Stop();
            _overlayTimer.Start();

        }

        private void Adivinador_Click(object sender, RoutedEventArgs e)
        {
            OverlayDibujante.Visibility = Visibility.Collapsed;
            OverlayAdivinador.Visibility = Visibility.Visible;

            _overlayTimer.Stop();
            _overlayTimer.Start();

        }

        private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _overlayTimer.Stop();
            if (sender is UIElement element)
                element.Visibility = Visibility.Collapsed;
        }

        private void OverlayTimer_Tick(object sender, EventArgs e)
        {
            _overlayTimer.Stop();
            OverlayDibujante.Visibility = Visibility.Collapsed;
            OverlayAdivinador.Visibility = Visibility.Collapsed;
            IniciarTemporizador();

        }

        private void IniciarTemporizador()
        {
            _contador = 30;
            txtContador.Text = _contador.ToString();
            txtContador.Foreground = Brushes.Black;

            campoTextoPalabraAdivinar.Visibility = Visibility.Visible;
            gridInfoCancion.Visibility = Visibility.Visible;

            campoTextoPalabraAdivinar.Text = "Gasolina";
            txtArtista.Text = "Artista: Daddy Yankee";
            txtGenero.Text = "Género: Reggaeton";

            _temporizador.Start();
        }

        private void Temporizador_Tick(object sender, EventArgs e)
        {
            _contador--;
            txtContador.Text = _contador.ToString();

            if (_contador <= 10)
            {
                txtContador.Foreground = Brushes.Red;
            }

            if (_contador <= 0)
            {
                _temporizador.Stop();
                txtContador.Text = "0";

                campoTextoPalabraAdivinar.Visibility = Visibility.Collapsed;
                gridInfoCancion.Visibility = Visibility.Collapsed;

                MessageBox.Show("¡Tiempo terminado!");
            }
        }

    }
}
