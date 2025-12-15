using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using PictionaryMusicalCliente.VistaModelo.Salas;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana principal de la partida que gestiona el tablero de dibujo, chat y logica del juego.
    /// </summary>
    public partial class Sala : Window
    {
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
                vistaModelo.MostrarConfirmacion = MostrarConfirmacion;
                vistaModelo.SolicitarDatosReporte = SolicitarDatosReporte;
                vistaModelo.MostrarInvitarAmigos = MostrarInvitarAmigosAsync;
                vistaModelo.CerrarVentana = () => Close();
                vistaModelo.ChequearCierreAplicacionGlobal = 
                    DebeCerrarAplicacionPorCierreDeVentana;
            }
        }

        private void AlCerrarSeSala(object sender, CancelEventArgs argumentosEvento)
        {
            if (DataContext is SalaVistaModelo vistaModelo && 
                vistaModelo.CerrarVentanaComando.CanExecute(null))
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
            App.VentanaServicio.MostrarVentanaDialogo(vistaModelo);
            return vistaModelo.DialogResult == true;
        }

        private ResultadoReporteJugador SolicitarDatosReporte(string nombreJugador)
        {
            var vistaModelo = new ReportarJugadorVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                _sonidos,
                nombreJugador);
            App.VentanaServicio.MostrarVentanaDialogo(vistaModelo);

            return new ResultadoReporteJugador
            {
                Confirmado = vistaModelo.DialogResult == true,
                Motivo = vistaModelo.Motivo
            };
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
    }
}
