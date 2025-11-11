using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.VistaModelo;
using System;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using DTOs = Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente
{
    public partial class VentanaJuego : Window
    {
        private readonly VentanaJuegoVistaModelo _vistaModelo;

        public VentanaJuego(DTOs.SalaDTO sala, ISalasServicio salasServicio)
        {
            InitializeComponent();

            _vistaModelo = new VentanaJuegoVistaModelo(sala, salasServicio)
            {
                AbrirExpulsionJugador = () => AbrirDialogo(new ExpulsionJugador()),
                AbrirInvitacionAmigos = () => AbrirDialogo(new InvitacionAmigos()),
                AbrirAjustesPartida = manejadorCancion =>
                {
                    var ajustes = new AjustesPartida(manejadorCancion);
                    AbrirDialogo(ajustes);
                },
                NotificarCambioHerramienta = EstablecerHerramienta,
                AplicarEstiloLapiz = AplicarEstiloLapiz,
                ActualizarFormaGoma = ActualizarFormaGoma,
                LimpiarTrazos = () => ink?.Strokes.Clear(),
                MostrarMensaje = AvisoAyudante.Mostrar
            };

            DataContext = _vistaModelo;
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

        private void EstablecerHerramienta(bool esLapiz)
        {
            if (ink == null)
            {
                return;
            }

            ink.EditingMode = esLapiz
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
            if (ink == null)
            {
                return;
            }

            ink.DefaultDrawingAttributes = new DrawingAttributes
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
            if (ink == null)
            {
                return;
            }

            var size = Math.Max(1, _vistaModelo.Grosor);
            ink.EraserShape = new EllipseStylusShape(size, size);
        }
    }
}
