using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
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
using PictionaryMusicalCliente.VistaModelo.Amigos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.VistaModelo.Salas;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Ventana principal de la partida que gestiona el tablero de dibujo, chat y logica del juego.
    /// </summary>
    public partial class VentanaJuego : Window
    {
        private readonly SalaVistaModelo _vistaModelo;
        private readonly Action _accionAlCerrar;
        private readonly List<Point> _puntosBorrador = new();
        private bool _borradoEnProgreso;

        /// <summary>
        /// Inicializa la partida con la configuracion de la sala y el usuario.
        /// </summary>
        public VentanaJuego(
            SalaDTO sala,
            ISalasServicio salasServicio,
            bool esInvitado = false,
            string nombreJugador = null,
            Action accionAlCerrar = null)
        {
            InitializeComponent();

            if (salasServicio == null)
            {
                throw new ArgumentNullException(nameof(salasServicio));
            }

            _accionAlCerrar = accionAlCerrar;

            _vistaModelo = new SalaVistaModelo(
                sala,
                salasServicio,
                nombreJugador,
                esInvitado);

            _vistaModelo.AbrirAjustesPartida = manejadorCancion =>
            {
                var ajustes = new AjustesPartida(manejadorCancion);
                AbrirDialogo(ajustes);
            };
            _vistaModelo.NotificarCambioHerramienta = EstablecerHerramienta;
            _vistaModelo.AplicarEstiloLapiz = AplicarEstiloLapiz;
            _vistaModelo.ActualizarFormaGoma = ActualizarFormaGoma;
            _vistaModelo.LimpiarTrazos = LimpiarLienzo;
            _vistaModelo.MostrarMensaje = AvisoAyudante.Mostrar;
            _vistaModelo.MostrarConfirmacion = MostrarConfirmacion;
            _vistaModelo.MostrarInvitarAmigos = MostrarInvitarAmigosAsync;

            _vistaModelo.ManejarNavegacion = EjecutarNavegacion;
            _vistaModelo.CerrarVentana = () => Close();

            _vistaModelo.ChequearCierreAplicacionGlobal = DebeCerrarAplicacionPorCierreDeVentana;

            _vistaModelo.TrazoRecibidoServidor += VistaModelo_TrazoRecibidoServidor;
            _vistaModelo.MensajeChatRecibido += VistaModelo_MensajeChatRecibido;
            _vistaModelo.MensajeDoradoRecibido += VistaModelo_MensajeDoradoRecibido;

            DataContext = _vistaModelo;

            RegistrarEventosLienzo();

            Closing += VentanaJuego_Closing;
            Closed += VentanaJuego_ClosedAsync;
        }

        private void RegistrarEventosLienzo()
        {
            if (ink == null)
            {
                return;
            }

            ink.StrokeCollected += Ink_StrokeCollected;
            ink.PreviewMouseLeftButtonDown += Ink_PreviewMouseLeftButtonDown;
            ink.PreviewMouseMove += Ink_PreviewMouseMove;
            ink.PreviewMouseLeftButtonUp += Ink_PreviewMouseLeftButtonUp;
        }

        private void Ink_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            if (!_vistaModelo.EsDibujante || e.Stroke == null)
            {
                return;
            }

            var trazo = ConvertirStrokeATrazo(e.Stroke, false);
            _vistaModelo.EnviarTrazoAlServidor(trazo);
        }

        private void Ink_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_vistaModelo.EsDibujante || !_vistaModelo.EsHerramientaBorrador)
            {
                return;
            }

            _borradoEnProgreso = true;
            _puntosBorrador.Clear();
            _puntosBorrador.Add(e.GetPosition(ink));
        }

        private void Ink_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_borradoEnProgreso)
            {
                return;
            }

            _puntosBorrador.Add(e.GetPosition(ink));
        }

        private void Ink_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_borradoEnProgreso)
            {
                return;
            }

            _borradoEnProgreso = false;

            var trazo = ConvertirPuntosATrazoBorrador(_puntosBorrador, _vistaModelo.Grosor);
            if (trazo != null)
            {
                _vistaModelo.EnviarTrazoAlServidor(trazo);
            }

            _puntosBorrador.Clear();
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
            if (trazo == null || ink == null)
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

            ink.Strokes.Add(stroke);
        }

        private void AplicarBorradoRemoto(TrazoDTO trazo)
        {
            if (trazo.PuntosX == null || trazo.PuntosY == null)
            {
                return;
            }

            if (trazo.EsLimpiarTodo)
            {
                ink.Strokes.Clear();
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
            var strokesActuales = ink.Strokes.ToList();

            foreach (var stroke in strokesActuales)
            {
                var resultado = stroke.GetEraseResult(puntosTrayectoria, formaBorrador);
                ink.Strokes.Remove(stroke);

                if (resultado != null && resultado.Count > 0)
                {
                    ink.Strokes.Add(resultado);
                }
            }
        }

        private void VentanaJuego_Closing(object sender, CancelEventArgs e)
        {
            if (_vistaModelo.CerrarVentanaComando.CanExecute(null))
            {
                _vistaModelo.CerrarVentanaComando.Execute(null);
            }
        }

        private async void VentanaJuego_ClosedAsync(object sender, EventArgs e)
        {
            Closed -= VentanaJuego_ClosedAsync;
            Closing -= VentanaJuego_Closing;

            _vistaModelo.TrazoRecibidoServidor -= VistaModelo_TrazoRecibidoServidor;
            _vistaModelo.MensajeChatRecibido -= VistaModelo_MensajeChatRecibido;
            _vistaModelo.MensajeDoradoRecibido -= VistaModelo_MensajeDoradoRecibido;

            await _vistaModelo.FinalizarAsync().ConfigureAwait(false);

            if (_accionAlCerrar != null && _vistaModelo.DebeEjecutarAccionAlCerrar())
            {
                if (!Dispatcher.CheckAccess())
                {
                    await Dispatcher.InvokeAsync(_accionAlCerrar);
                }
                else
                {
                    _accionAlCerrar();
                }
            }
        }

        private void EjecutarNavegacion(SalaVistaModelo.DestinoNavegacion destino)
        {
            Window ventanaDestino = destino == SalaVistaModelo.DestinoNavegacion.
                InicioSesion
                ? new InicioSesion()
                : new VentanaPrincipal();

            ventanaDestino.Show();
            Close();
        }

        private bool MostrarConfirmacion(string mensaje)
        {
            var ventana = new ExpulsionJugador(mensaje)
            {
                Owner = this
            };

            bool? resultado = ventana.ShowDialog();

            return resultado == true;
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
                var ventana = new InvitarAmigos(vistaModelo)
                {
                    Owner = this
                };
                ventana.ShowDialog();
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
            var lienzoTinta = (InkCanvas)this.FindName("ink");
            if (lienzoTinta == null)
            {
                return;
            }

            lienzoTinta.EditingMode = esLapiz
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
            var lienzoTinta = (InkCanvas)this.FindName("ink");
            if (lienzoTinta == null || _vistaModelo == null)
            {
                return;
            }

            lienzoTinta.DefaultDrawingAttributes = new DrawingAttributes
            {
                Color = _vistaModelo.Color,
                Width = _vistaModelo.Grosor,
                Height = _vistaModelo.Grosor,
                FitToCurve = false,
                IgnorePressure = true
            };
        }

        private void ActualizarFormaGoma()
        {
            var lienzoTinta = (InkCanvas)this.FindName("ink");
            if (lienzoTinta == null || _vistaModelo == null)
            {
                return;
            }

            var tamano = Math.Max(1, _vistaModelo.Grosor);
            lienzoTinta.EraserShape = new EllipseStylusShape(tamano, tamano);
        }

        private void LimpiarLienzo()
        {
            ink?.Strokes.Clear();
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
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                if (_vistaModelo.EnviarMensajeChatComando?.CanExecute(null) == true)
                {
                    _vistaModelo.EnviarMensajeChatComando.Execute(null);
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

            scrollChat?.ScrollToEnd();
        }
    }
}
