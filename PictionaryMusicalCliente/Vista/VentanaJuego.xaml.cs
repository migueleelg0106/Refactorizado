using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.VistaModelo.VentanaJuego;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Controls;
using System.Threading.Tasks;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using System.Windows.Input;
using System.Linq;

namespace PictionaryMusicalCliente
{
    public partial class VentanaJuego : Window
    {
        private readonly VentanaJuegoVistaModelo _vistaModelo;
        private readonly Action _accionAlCerrar;

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

            _vistaModelo = new VentanaJuegoVistaModelo(
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
            _vistaModelo.LimpiarTrazos = () => ink?.Strokes.Clear();
            _vistaModelo.MostrarMensaje = AvisoAyudante.Mostrar;
            _vistaModelo.MostrarConfirmacion = MostrarConfirmacion;
            _vistaModelo.MostrarInvitarAmigos = MostrarInvitarAmigosAsync;

            _vistaModelo.ManejarNavegacion = EjecutarNavegacion;
            _vistaModelo.CerrarVentana = () => Close();

            _vistaModelo.ChequearCierreAplicacionGlobal = DebeCerrarAplicacionPorCierreDeVentana;

            DataContext = _vistaModelo;

            Closing += VentanaJuego_Closing;
            Closed += VentanaJuego_ClosedAsync;
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

        private void EjecutarNavegacion(VentanaJuegoVistaModelo.DestinoNavegacion destino)
        {
            Window ventanaDestino = destino == VentanaJuegoVistaModelo.DestinoNavegacion.InicioSesion
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
            if (ventana == null) return;
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private async Task MostrarInvitarAmigosAsync(InvitarAmigosVistaModelo vistaModelo)
        {
            if (vistaModelo == null) return;

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
            if (lienzoTinta == null) return;

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
            if (lienzoTinta == null || _vistaModelo == null) return;

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
            if (lienzoTinta == null || _vistaModelo == null) return;

            var tamano = Math.Max(1, _vistaModelo.Grosor);
            lienzoTinta.EraserShape = new EllipseStylusShape(tamano, tamano);
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
    }
}