using PictionaryMusicalCliente.VistaModelo.Salas;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Control de usuario para el area de juego de la partida (lienzo).
    /// </summary>
    public partial class Partida : UserControl
    {
        private readonly List<Point> _puntosBorrador = new();
        private bool _borradoEnProgreso;

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public Partida()
        {
            InitializeComponent();
            Loaded += AlCargarPartida;
        }

        private void AlCargarPartida(object sender, RoutedEventArgs e)
        {
            if (DataContext is PartidaVistaModelo vistaModelo)
            {
                vistaModelo.NotificarCambioHerramienta = EstablecerHerramienta;
                vistaModelo.AplicarEstiloLapiz = AplicarEstiloLapiz;
                vistaModelo.ActualizarFormaGoma = ActualizarFormaGoma;
                vistaModelo.LimpiarTrazos = LimpiarLienzo;

                vistaModelo.TrazoRecibidoServidor += AlRecibirTrazoDelServidor;

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

        private void AlRecolectarTrazoEnLienzo(object sender,
            InkCanvasStrokeCollectedEventArgs argumentosEvento)
        {
            if (argumentosEvento.Stroke == null || !(DataContext is PartidaVistaModelo vistaModelo)
                || !vistaModelo.EsDibujante)
            {
                return;
            }

            var trazo = ConvertirLineaATrazo(argumentosEvento.Stroke, false);
            vistaModelo.EnviarTrazoAlServidor?.Invoke(trazo);
        }

        private void AlPresionarBotonIzquierdoEnLienzo(object sender,
            MouseButtonEventArgs argumentosEvento)
        {
            if (DataContext is PartidaVistaModelo vistaModelo && vistaModelo.EsDibujante
                && vistaModelo.EsHerramientaBorrador)
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

        private void AlSoltarBotonIzquierdoEnLienzo(object sender, 
            MouseButtonEventArgs argumentosEvento)
        {
            if (_borradoEnProgreso && DataContext is PartidaVistaModelo vistaModelo)
            {
                _borradoEnProgreso = false;

                var trazo = ConvertirPuntosATrazoBorrador(_puntosBorrador, vistaModelo.Grosor);
                if (trazo != null)
                {
                    vistaModelo.EnviarTrazoAlServidor?.Invoke(trazo);
                }

                _puntosBorrador.Clear();
            }
        }

        private static TrazoDTO ConvertirPuntosATrazoBorrador(IEnumerable<Point> puntos, 
            double grosor)
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
                PuntosX = listaPuntos.Select(puntos => puntos.X).ToArray(),
                PuntosY = listaPuntos.Select(puntos => puntos.Y).ToArray(),
                ColorHex = Colors.Transparent.ToString(),
                Grosor = grosor,
                EsBorrado = true
            };
        }

        private static TrazoDTO ConvertirLineaATrazo(Stroke linea, bool esBorrado)
        {
            if (linea == null)
            {
                return null;
            }

            var puntos = linea.StylusPoints;

            return new TrazoDTO
            {
                PuntosX = puntos.Select(puntos => puntos.X).ToArray(),
                PuntosY = puntos.Select(puntos => puntos.Y).ToArray(),
                ColorHex = ColorAHex(linea.DrawingAttributes.Color),
                Grosor = linea.DrawingAttributes.Width,
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
                Color = (Color)ColorConverter.ConvertFromString(trazo.ColorHex ?? 
                    Colors.Black.ToString()),
                Width = trazo.Grosor,
                Height = trazo.Grosor,
                FitToCurve = false,
                IgnorePressure = true
            };

            var linea = new Stroke(puntos)
            {
                DrawingAttributes = atributos
            };

            inkLienzoDibujo.Strokes.Add(linea);
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
            var lineasActuales = inkLienzoDibujo.Strokes.ToList();

            foreach (var linea in lineasActuales)
            {
                var resultado = linea.GetEraseResult(puntosTrayectoria, formaBorrador);
                inkLienzoDibujo.Strokes.Remove(linea);

                if (resultado != null && resultado.Count > 0)
                {
                    inkLienzoDibujo.Strokes.Add(resultado);
                }
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
            if (inkLienzoDibujo == null || !(DataContext is PartidaVistaModelo vistaModelo))
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
            if (inkLienzoDibujo == null || !(DataContext is PartidaVistaModelo vistaModelo))
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
    }
}
