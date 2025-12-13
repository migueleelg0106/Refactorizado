using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using PictionaryMusicalCliente.VistaModelo.Salas;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana principal de la partida que gestiona el tablero de dibujo, chat y logica del juego.
    /// </summary>
    public partial class Sala : Window
    {
        private readonly List<Point> _puntosBorrador = new();
        private bool _borradoEnProgreso;
        private SonidoManejador _sonidos = App.SonidoManejador;

        /// <summary>
        /// Constructor por defecto. VentanaServicio asigna el DataContext.
        /// </summary>
        public Sala()
        {
            InitializeComponent();
            Loaded += Sala_Loaded;
            Closing += Sala_Closing;
            Closed += Sala_Closed;
        }

        private void Sala_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SalaVistaModelo vm)
            {
                vm.NotificarCambioHerramienta = EstablecerHerramienta;
                vm.AplicarEstiloLapiz = AplicarEstiloLapiz;
                vm.ActualizarFormaGoma = ActualizarFormaGoma;
                vm.LimpiarTrazos = LimpiarLienzo;
                vm.MostrarConfirmacion = MostrarConfirmacion;
                vm.SolicitarDatosReporte = SolicitarDatosReporte;
                vm.MostrarInvitarAmigos = MostrarInvitarAmigosAsync;
                vm.CerrarVentana = () => Close();
                vm.ChequearCierreAplicacionGlobal = DebeCerrarAplicacionPorCierreDeVentana;

                vm.TrazoRecibidoServidor += VistaModelo_TrazoRecibidoServidor;
                vm.MensajeChatRecibido += VistaModelo_MensajeChatRecibido;
                vm.MensajeDoradoRecibido += VistaModelo_MensajeDoradoRecibido;

                RegistrarEventosLienzo();
            }
        }

        private void RegistrarEventosLienzo()
        {
            if (inkLienzoDibujo == null)
            {
                return;
            }

            inkLienzoDibujo.StrokeCollected += Ink_StrokeCollected;
            inkLienzoDibujo.PreviewMouseLeftButtonDown += Ink_PreviewMouseLeftButtonDown;
            inkLienzoDibujo.PreviewMouseMove += Ink_PreviewMouseMove;
            inkLienzoDibujo.PreviewMouseLeftButtonUp += Ink_PreviewMouseLeftButtonUp;
        }

        private void Ink_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            if (e.Stroke == null || !(DataContext is SalaVistaModelo vm) || !vm.EsDibujante)
            {
                return;
            }

            var trazo = ConvertirStrokeATrazo(e.Stroke, false);
            vm.EnviarTrazoAlServidor(trazo);
        }

        private void Ink_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is SalaVistaModelo vm && vm.EsDibujante && vm.EsHerramientaBorrador)
            {
                _borradoEnProgreso = true;
                _puntosBorrador.Clear();
                _puntosBorrador.Add(e.GetPosition(inkLienzoDibujo));
            }
        }

        private void Ink_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_borradoEnProgreso)
            {
                _puntosBorrador.Add(e.GetPosition(inkLienzoDibujo));
            }
        }

        private void Ink_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_borradoEnProgreso && DataContext is SalaVistaModelo vm)
            {
                _borradoEnProgreso = false;

                var trazo = ConvertirPuntosATrazoBorrador(_puntosBorrador, vm.Grosor);
                if (trazo != null)
                {
                    vm.EnviarTrazoAlServidor(trazo);
                }

                _puntosBorrador.Clear();
            }
        }

        private static TrazoDTO ConvertirPuntosATrazoBorrador(IEnumerable<Point> puntos, double grosor)
        {
            if (puntos == null)
            {
                return null;
            }

            var listaPuntos = puntos.ToList();
            if (listaPuntos.Count == 0)
            {
                return null;
            }

            return new TrazoDTO
            {
                PuntosX = listaPuntos.Select(p => p.X).ToArray(),
                PuntosY = listaPuntos.Select(p => p.Y).ToArray(),
                ColorHex = Colors.Transparent.ToString(),
                Grosor = grosor,
                EsBorrado = true
            };
        }

        private static TrazoDTO ConvertirStrokeATrazo(Stroke stroke, bool esBorrado)
        {
            if (stroke == null)
            {
                return null;
            }

            var puntos = stroke.StylusPoints;

            return new TrazoDTO
            {
                PuntosX = puntos.Select(p => p.X).ToArray(),
                PuntosY = puntos.Select(p => p.Y).ToArray(),
                ColorHex = ColorAHex(stroke.DrawingAttributes.Color),
                Grosor = stroke.DrawingAttributes.Width,
                EsBorrado = esBorrado
            };
        }

        private void VistaModelo_TrazoRecibidoServidor(TrazoDTO trazo)
        {
            if (trazo == null || inkLienzoDibujo == null)
            {
                return;
            }

            if (trazo.EsBorrado)
            {
                AplicarBorradoRemoto(trazo);
                return;
            }

            if (trazo.PuntosX == null || trazo.PuntosY == null)
            {
                return;
            }

            var puntos = new StylusPointCollection();
            for (int i = 0; i < Math.Min(trazo.PuntosX.Length, trazo.PuntosY.Length); i++)
            {
                puntos.Add(new StylusPoint(trazo.PuntosX[i], trazo.PuntosY[i]));
            }

            var atributos = new DrawingAttributes
            {
                Color = (Color)ColorConverter.ConvertFromString(trazo.ColorHex ?? Colors.Black.ToString()),
                Width = trazo.Grosor,
                Height = trazo.Grosor,
                FitToCurve = false,
                IgnorePressure = true
            };

            var stroke = new Stroke(puntos)
            {
                DrawingAttributes = atributos
            };

            inkLienzoDibujo.Strokes.Add(stroke);
        }

        private void AplicarBorradoRemoto(TrazoDTO trazo)
        {
            if (trazo.PuntosX == null || trazo.PuntosY == null)
            {
                return;
            }

            if (trazo.EsLimpiarTodo)
            {
                inkLienzoDibujo.Strokes.Clear();
                return;
            }

            var puntosTrayectoria = new List<Point>();
            for (int i = 0; i < Math.Min(trazo.PuntosX.Length, trazo.PuntosY.Length); i++)
            {
                puntosTrayectoria.Add(new Point(trazo.PuntosX[i], trazo.PuntosY[i]));
            }

            if (puntosTrayectoria.Count == 0)
            {
                return;
            }

            var tamano = Math.Max(1, trazo.Grosor);
            var formaBorrador = new EllipseStylusShape(tamano, tamano);
            var strokesActuales = inkLienzoDibujo.Strokes.ToList();

            foreach (var stroke in strokesActuales)
            {
                var resultado = stroke.GetEraseResult(puntosTrayectoria, formaBorrador);
                inkLienzoDibujo.Strokes.Remove(stroke);

                if (resultado != null && resultado.Count > 0)
                {
                    inkLienzoDibujo.Strokes.Add(resultado);
                }
            }
        }

        private void Sala_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is SalaVistaModelo vm && vm.CerrarVentanaComando.CanExecute(null))
            {
                vm.CerrarVentanaComando.Execute(null);
            }
        }

        private async void Sala_Closed(object sender, EventArgs e)
        {
            Loaded -= Sala_Loaded;
            Closed -= Sala_Closed;
            Closing -= Sala_Closing;

            if (DataContext is SalaVistaModelo vm)
            {
                vm.TrazoRecibidoServidor -= VistaModelo_TrazoRecibidoServidor;
                vm.MensajeChatRecibido -= VistaModelo_MensajeChatRecibido;
                vm.MensajeDoradoRecibido -= VistaModelo_MensajeDoradoRecibido;

                await vm.FinalizarAsync().ConfigureAwait(false);
            }
        }

        private bool MostrarConfirmacion(string mensaje)
        {
            var vm = new ExpulsionJugadorVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                _sonidos,
                mensaje);
            return App.VentanaServicio.MostrarVentanaDialogo(vm) == true;
        }

        private ResultadoReporteJugador SolicitarDatosReporte(string nombreJugador)
        {
            var vistaModelo = new ReportarJugadorVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                _sonidos,
                nombreJugador);
            bool? resultado = App.VentanaServicio.MostrarVentanaDialogo(vistaModelo);

            return new ResultadoReporteJugador
            {
                Confirmado = resultado == true,
                Motivo = vistaModelo.Motivo
            };
        }

        private void AbrirDialogo(Window ventana)
        {
            if (ventana == null)
            {
                return;
            }

            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private async Task MostrarInvitarAmigosAsync(InvitarAmigosVistaModelo vistaModelo)
        {
            if (vistaModelo == null)
            {
                return;
            }

            void MostrarVentana()
            {
                App.VentanaServicio.MostrarVentanaDialogo(vistaModelo);
            }

            if (!Dispatcher.CheckAccess())
            {
                await Dispatcher.InvokeAsync((Action)MostrarVentana);
            }
            else
            {
                MostrarVentana();
            }
        }

        private void EstablecerHerramienta(bool esLapiz)
        {
            if (inkLienzoDibujo == null)
            {
                return;
            }

            inkLienzoDibujo.EditingMode = esLapiz
                ? InkCanvasEditingMode.Ink
                : InkCanvasEditingMode.EraseByPoint;

            if (esLapiz)
            {
                AplicarEstiloLapiz();
            }
            else
            {
                ActualizarFormaGoma();
            }
        }

        private void AplicarEstiloLapiz()
        {
            if (inkLienzoDibujo == null || !(DataContext is SalaVistaModelo vm))
            {
                return;
            }

            inkLienzoDibujo.DefaultDrawingAttributes = new DrawingAttributes
            {
                Color = vm.Color,
                Width = vm.Grosor,
                Height = vm.Grosor,
                FitToCurve = false,
                IgnorePressure = true
            };
        }

        private void ActualizarFormaGoma()
        {
            if (inkLienzoDibujo == null || !(DataContext is SalaVistaModelo vm))
            {
                return;
            }

            var tamano = Math.Max(1, vm.Grosor);
            inkLienzoDibujo.EraserShape = new EllipseStylusShape(tamano, tamano);
        }

        private void LimpiarLienzo()
        {
            inkLienzoDibujo?.Strokes.Clear();
        }

        private static string ColorAHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private bool DebeCerrarAplicacionPorCierreDeVentana()
        {
            var aplicacion = Application.Current;

            if (aplicacion?.Dispatcher?.HasShutdownStarted == true ||
                aplicacion?.Dispatcher?.HasShutdownFinished == true)
            {
                return true;
            }

            if (aplicacion == null)
            {
                return true;
            }

            foreach (Window ventana in aplicacion.Windows)
            {
                if (!ReferenceEquals(ventana, this) && ventana.IsVisible)
                {
                    return false;
                }
            }

            return true;
        }

        private void CampoTextoChat_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Return) && DataContext is SalaVistaModelo vm)
            {
                if (vm.EnviarMensajeChatComando?.CanExecute(null) == true)
                {
                    vm.EnviarMensajeChatComando.Execute(null);
                }

                e.Handled = true;
            }
        }

        private void VistaModelo_MensajeChatRecibido(string nombreJugador, string mensaje)
        {
            AgregarMensajeAlChat(nombreJugador, mensaje, Colors.Black);
        }

        private void VistaModelo_MensajeDoradoRecibido(string nombreJugador, string mensaje)
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
