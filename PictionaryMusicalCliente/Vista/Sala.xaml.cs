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
            Loaded += AlCargarSala;
            Closing += AlCerrarSeSala;
            Closed += AlCerrarSala;
        }

        private void AlCargarSala(object sender, RoutedEventArgs e)
        {
            if (DataContext is SalaVistaModelo vistaModelo)
            {
                vistaModelo.NotificarCambioHerramienta = EstablecerHerramienta;
                vistaModelo.AplicarEstiloLapiz = AplicarEstiloLapiz;
                vistaModelo.ActualizarFormaGoma = ActualizarFormaGoma;
                vistaModelo.LimpiarTrazos = LimpiarLienzo;
                vistaModelo.MostrarConfirmacion = MostrarConfirmacion;
                vistaModelo.SolicitarDatosReporte = SolicitarDatosReporte;
                vistaModelo.MostrarInvitarAmigos = MostrarInvitarAmigosAsync;
                vistaModelo.CerrarVentana = () => Close();
                vistaModelo.ChequearCierreAplicacionGlobal = DebeCerrarAplicacionPorCierreDeVentana;

                vistaModelo.TrazoRecibidoServidor += AlRecibirTrazoDelServidor;
                vistaModelo.MensajeChatRecibido += AlRecibirMensajeChat;
                vistaModelo.MensajeDoradoRecibido += AlRecibirMensajeDorado;

                RegistrarEventosLienzo();
            }
        }

        private void RegistrarEventosLienzo()
        {
            if (inkLienzoDibujo == null)
            {
                return;
            }

            inkLienzoDibujo.StrokeCollected += AlRecolectarTrazoEnLienzo;
            inkLienzoDibujo.PreviewMouseLeftButtonDown += AlPresionarBotonIzquierdoEnLienzo;
            inkLienzoDibujo.PreviewMouseMove += AlMoverRatonEnLienzo;
            inkLienzoDibujo.PreviewMouseLeftButtonUp += AlSoltarBotonIzquierdoEnLienzo;
        }

        private void AlRecolectarTrazoEnLienzo(object sender, InkCanvasStrokeCollectedEventArgs argumentosEvento)
        {
            if (argumentosEvento.Stroke == null || !(DataContext is SalaVistaModelo vistaModelo) || !vistaModelo.EsDibujante)
            {
                return;
            }

            var trazo = ConvertirStrokeATrazo(argumentosEvento.Stroke, false);
            vistaModelo.EnviarTrazoAlServidor(trazo);
        }

        private void AlPresionarBotonIzquierdoEnLienzo(object sender, MouseButtonEventArgs argumentosEvento)
        {
            if (DataContext is SalaVistaModelo vistaModelo && vistaModelo.EsDibujante && vistaModelo.EsHerramientaBorrador)
            {
                _borradoEnProgreso = true;
                _puntosBorrador.Clear();
                _puntosBorrador.Add(argumentosEvento.GetPosition(inkLienzoDibujo));
            }
        }

        private void AlMoverRatonEnLienzo(object sender, MouseEventArgs argumentosEvento)
        {
            if (_borradoEnProgreso)
            {
                _puntosBorrador.Add(argumentosEvento.GetPosition(inkLienzoDibujo));
            }
        }

        private void AlSoltarBotonIzquierdoEnLienzo(object sender, MouseButtonEventArgs argumentosEvento)
        {
            if (_borradoEnProgreso && DataContext is SalaVistaModelo vistaModelo)
            {
                _borradoEnProgreso = false;

                var trazo = ConvertirPuntosATrazoBorrador(_puntosBorrador, vistaModelo.Grosor);
                if (trazo != null)
                {
                    vistaModelo.EnviarTrazoAlServidor(trazo);
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

        private void AlRecibirTrazoDelServidor(TrazoDTO trazo)
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

        private void AlCerrarSeSala(object sender, CancelEventArgs argumentosEvento)
        {
            if (DataContext is SalaVistaModelo vistaModelo && vistaModelo.CerrarVentanaComando.CanExecute(null))
            {
                vistaModelo.CerrarVentanaComando.Execute(null);
            }
        }

        private async void AlCerrarSala(object sender, EventArgs argumentosEvento)
        {
            Loaded -= AlCargarSala;
            Closed -= AlCerrarSala;
            Closing -= AlCerrarSeSala;

            if (DataContext is SalaVistaModelo vistaModelo)
            {
                vistaModelo.TrazoRecibidoServidor -= AlRecibirTrazoDelServidor;
                vistaModelo.MensajeChatRecibido -= AlRecibirMensajeChat;
                vistaModelo.MensajeDoradoRecibido -= AlRecibirMensajeDorado;

                await vistaModelo.FinalizarAsync().ConfigureAwait(false);
            }
        }

        private bool MostrarConfirmacion(string mensaje)
        {
            var vistaModelo = new ExpulsionJugadorVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                _sonidos,
                mensaje);
            return App.VentanaServicio.MostrarVentanaDialogo(vistaModelo) == true;
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
            if (inkLienzoDibujo == null || !(DataContext is SalaVistaModelo vistaModelo))
            {
                return;
            }

            inkLienzoDibujo.DefaultDrawingAttributes = new DrawingAttributes
            {
                Color = vistaModelo.Color,
                Width = vistaModelo.Grosor,
                Height = vistaModelo.Grosor,
                FitToCurve = false,
                IgnorePressure = true
            };
        }

        private void ActualizarFormaGoma()
        {
            if (inkLienzoDibujo == null || !(DataContext is SalaVistaModelo vistaModelo))
            {
                return;
            }

            var tamano = Math.Max(1, vistaModelo.Grosor);
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
