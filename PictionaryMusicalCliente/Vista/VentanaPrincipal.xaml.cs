using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.ClienteServicios.Idiomas;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using System;
using System.Windows;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente
{
    public partial class VentanaPrincipal : Window
    {
        private readonly MusicaManejador _servicioMusica;
        private readonly VentanaPrincipalVistaModelo _vistaModelo;
        private readonly ISalasServicio _salasServicio; 

        private readonly IListaAmigosServicio _listaAmigosServicio;
        private readonly IAmigosServicio _amigosServicio;

        public VentanaPrincipal()
        {
            InitializeComponent();

            _servicioMusica = new MusicaManejador();
            _listaAmigosServicio = new ListaAmigosServicio();
            _amigosServicio = new AmigosServicio();
            _salasServicio = new SalasServicio();

            _servicioMusica.ReproducirEnBucle("ventana_principal_musica.mp3");
            
            _vistaModelo = new VentanaPrincipalVistaModelo(
                LocalizacionServicio.Instancia,
                _listaAmigosServicio,
                _amigosServicio,
                _salasServicio);

            _vistaModelo.AbrirPerfil = () => MostrarDialogo(new Perfil());
            _vistaModelo.AbrirAjustes = () => MostrarDialogo(new Ajustes(_servicioMusica));
            _vistaModelo.AbrirComoJugar = () => MostrarDialogo(new ComoJugar());
            _vistaModelo.AbrirClasificacion = () => MostrarDialogo(new Clasificacion());
            _vistaModelo.AbrirBuscarAmigo = () => MostrarDialogo(new BusquedaAmigo(_amigosServicio));

            _vistaModelo.AbrirSolicitudes = () => MostrarDialogo(new Solicitudes(_amigosServicio));

            _vistaModelo.ConfirmarEliminarAmigo = MostrarConfirmacionEliminar;

            _vistaModelo.IniciarJuego = MostrarVentanaJuego;
            _vistaModelo.UnirseSala = MostrarVentanaJuego;

            _vistaModelo.MostrarMensaje = AvisoAyudante.Mostrar;

            DataContext = _vistaModelo;

            Loaded += VentanaPrincipal_LoadedAsync;
            Closed += VentanaPrincipal_ClosedAsync;
        }

        private async void VentanaPrincipal_LoadedAsync(object sender, RoutedEventArgs e)
        {
            await _vistaModelo.InicializarAsync().ConfigureAwait(true);
        }

        private async void VentanaPrincipal_ClosedAsync(object sender, EventArgs e)
        {
            Loaded -= VentanaPrincipal_LoadedAsync;
            Closed -= VentanaPrincipal_ClosedAsync;

            await _vistaModelo.FinalizarAsync().ConfigureAwait(false);

            _servicioMusica?.Dispose();
        }

        private bool? MostrarConfirmacionEliminar(string amigo)
        {
            var ventana = new EliminacionAmigo(amigo)
            {
                Owner = this
            };
            return ventana.ShowDialog();
        }

        private void MostrarDialogo(Window ventana)
        {
            // Lógica genérica de diálogo
            if (ventana == null) return;
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void MostrarVentanaJuego(DTOs.SalaDTO sala)
        {

            var ventanaJuego = new VentanaJuego(sala, _salasServicio);
            ventanaJuego.Show();

            this.Close();
        }
    }
}