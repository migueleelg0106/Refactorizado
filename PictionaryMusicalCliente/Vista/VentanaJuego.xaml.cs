using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.VistaModelo;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Ink;
using DTOs = Servicios.Contratos.DTOs;
using System.Windows.Controls;
using System.Threading.Tasks;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente
{
    public partial class VentanaJuego : Window
    {
        private readonly VentanaJuegoVistaModelo _vistaModelo;
        private readonly Action _accionAlCerrar;
        private bool _ejecutarAccionAlCerrar = true;
        private bool _cerrandoAplicacionCompleta;

        public VentanaJuego(DTOs.SalaDTO sala, ISalasServicio salasServicio, bool esInvitado = false, string nombreJugador = null, Action accionAlCerrar = null)
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
                esInvitado)
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
                    var ventana = new ExpulsionJugador(mensaje)
                    {
                        Owner = this
                    };

                    bool? resultado = ventana.ShowDialog();
                    return resultado == true;
                },
                ManejarExpulsion = destino =>
                {
                    void EjecutarAccionExpulsion()
                    {
                        if (destino == VentanaJuegoVistaModelo.DestinoNavegacion.InicioSesion)
                        {
                            DeshabilitarAccionAlCerrar();
                        }

                        Window ventanaDestino = destino == VentanaJuegoVistaModelo.DestinoNavegacion.InicioSesion
                            ? new InicioSesion()
                            : new VentanaPrincipal();

                        ventanaDestino.Show();

                        Close();
                    }

                    if (!Dispatcher.CheckAccess())
                    {
                        Dispatcher.Invoke(EjecutarAccionExpulsion);
                    }
                    else
                    {
                        EjecutarAccionExpulsion();
                    }
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
                },
                MostrarInvitarAmigos = async vistaModeloInvitacion =>
                {
                    await MostrarInvitarAmigosAsync(vistaModeloInvitacion).ConfigureAwait(true);
                }
            };

            DataContext = _vistaModelo;

            Closing += VentanaJuego_Closing;
            Closed += VentanaJuego_ClosedAsync;
        }

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

            if (_accionAlCerrar != null && _ejecutarAccionAlCerrar && !_cerrandoAplicacionCompleta)
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

        private Task MostrarInvitarAmigosAsync(InvitarAmigosVistaModelo vistaModelo)
        {
            if (vistaModelo == null)
            {
                return Task.CompletedTask;
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
                return Dispatcher.InvokeAsync((Action)MostrarVentana).Task;
            }

            MostrarVentana();
            return Task.CompletedTask;
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
