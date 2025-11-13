using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.VistaModelo;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using DTOs = Servicios.Contratos.DTOs;
using System.Windows.Controls;

namespace PictionaryMusicalCliente
{
    public partial class VentanaJuego : Window
    {
        private readonly VentanaJuegoVistaModelo _vistaModelo;
        private readonly ISalasServicio _salasServicio;
        private readonly Action _accionAlCerrar;
        private readonly bool _esInvitado;
        private bool _ejecutarAccionAlCerrar = true;
        private bool _cerrandoAplicacionCompleta;

        public VentanaJuego(DTOs.SalaDTO sala, ISalasServicio salasServicio, bool esInvitado = false, string nombreJugador = null, Action accionAlCerrar = null)
        {
            InitializeComponent();

            _salasServicio = salasServicio ?? throw new ArgumentNullException(nameof(salasServicio));
            _accionAlCerrar = accionAlCerrar;
            _esInvitado = esInvitado;

            _vistaModelo = new VentanaJuegoVistaModelo(sala, _salasServicio, new InvitacionesServicio(), nombreJugador, esInvitado)
            {
                AbrirAjustesPartida = manejadorCancion =>
                {
                    var ajustes = new AjustesPartida(manejadorCancion);
                    AbrirDialogo(ajustes);
                },
                NotificarCambioHerramienta = EstablecerHerramienta,
                AplicarEstiloLapiz = AplicarEstiloLapiz,
                ActualizarFormaGoma = ActualizarFormaGoma,
                LimpiarTrazos = () => ink?.Strokes.Clear(),
                MostrarMensaje = AvisoAyudante.Mostrar,
                MostrarConfirmacion = mensaje =>
                {
                    var resultado = MessageBox.Show(
                        mensaje,
                        Lang.expulsarJugadorTextoConfirmar,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    return resultado == MessageBoxResult.Yes;
                },
                CerrarVentana = () =>
                {
                    if (!Dispatcher.CheckAccess())
                    {
                        Dispatcher.Invoke(() => Close());
                    }
                    else
                    {
                        Close();
                    }
                }
            };

            DataContext = _vistaModelo;

            Closing += VentanaJuego_Closing;
            Closed += VentanaJuego_ClosedAsync;
        }

        public bool EsInvitado => _esInvitado;

        public void DeshabilitarAccionAlCerrar()
        {
            _ejecutarAccionAlCerrar = false;
        }

        private void VentanaJuego_Closing(object sender, CancelEventArgs e)
        {
            _cerrandoAplicacionCompleta = DebeCerrarAplicacionPorCierreDeVentana();

            if (_cerrandoAplicacionCompleta)
            {
                _ejecutarAccionAlCerrar = false;
            }
        }

        private async void VentanaJuego_ClosedAsync(object sender, EventArgs e)
        {
            Closed -= VentanaJuego_ClosedAsync;
            Closing -= VentanaJuego_Closing;
            await _vistaModelo.FinalizarAsync().ConfigureAwait(false);

            _salasServicio?.Dispose();

            if (_accionAlCerrar != null && _ejecutarAccionAlCerrar && !_cerrandoAplicacionCompleta)
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(_accionAlCerrar);
                }
                else
                {
                    _accionAlCerrar();
                }
            }
        }

        private bool DebeCerrarAplicacionPorCierreDeVentana()
        {
            var aplicacion = Application.Current;

            if (aplicacion?.Dispatcher?.HasShutdownStarted == true || aplicacion?.Dispatcher?.HasShutdownFinished == true)
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
