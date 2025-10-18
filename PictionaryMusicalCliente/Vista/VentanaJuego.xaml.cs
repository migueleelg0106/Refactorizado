using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente
{
    public partial class VentanaJuego : Window
    {
        private bool _juegoIniciado = false;
        private double _grosor = 6;
        private Color _color = Colors.Black;
        private bool _syncingToolUI = false;


        public VentanaJuego()
        {
            InitializeComponent();
        }

        private void BotonInvitarCorreo(object sender, RoutedEventArgs e)
        {
            // de momento esta aqui para la prueba de la ventana pero esto ira cuando se implemente la duncion de expulsar jugador que es la que tenemos duda
            ExpulsarJugador expulsarJugador = new ExpulsarJugador();
            expulsarJugador.ShowDialog();
            // esto igual es momentaneo en lo que se hace lo de los botones dinamicos de esa lista
            InvitarAmigos invitarAmigos = new InvitarAmigos();
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

            grdDibujo.Visibility = Visibility.Visible;
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
            if (tbtnLapiz != null) tbtnLapiz.IsChecked = isPencil;
            if (tbtnBorrador != null) tbtnBorrador.IsChecked = !isPencil;
            _syncingToolUI = false;

            ink.EditingMode = isPencil
                ? InkCanvasEditingMode.Ink
                : InkCanvasEditingMode.EraseByPoint;

            if (isPencil) AplicarEstiloLapiz();
            else ActualizarEraserShape();
        }

        // Handlers de los ToggleButton basados en Click
        private void tbtnLapiz_Click(object sender, RoutedEventArgs e)
        {
            if (_syncingToolUI) return;   // click provocado por SetTool
            SetTool(true);
        }

        private void tbtnBorrador_Click(object sender, RoutedEventArgs e)
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

        private void btnBorrar_Click(object sender, RoutedEventArgs e)
        {
            ink?.Strokes.Clear();
        }
    }
}
