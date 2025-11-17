using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Idiomas;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.VistaModelo.VentanaPrincipal;
using System;
using System.Windows;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente
{
    public partial class VentanaPrincipal : Window
    {
        private readonly MusicaManejador _servicioMusica;

        private readonly IListaAmigosServicio _listaAmigosServicio;
        private readonly IAmigosServicio _amigosServicio;
        private readonly ISalasServicio _salasServicio;
        private bool _abrioVentanaJuego;
        private readonly VentanaPrincipalVistaModelo _vistaModelo;

        public VentanaPrincipal()
        {
            InitializeComponent();

            _servicioMusica = new MusicaManejador();
            _servicioMusica.ReproducirEnBucle("ventana_principal_musica.mp3");

            _listaAmigosServicio = new ListaAmigosServicio();
            _amigosServicio = new AmigosServicio();
            _salasServicio = new SalasServicio();

            _vistaModelo = new VentanaPrincipalVistaModelo(
                LocalizacionServicio.Instancia,
                _listaAmigosServicio,
                _amigosServicio,
                _salasServicio)
            {
                AbrirPerfil = () => MostrarDialogo(new Perfil()),
                AbrirAjustes = () => MostrarDialogo(new Ajustes(_servicioMusica)),
                AbrirComoJugar = () => MostrarDialogo(new ComoJugar()),
                AbrirClasificacion = () => MostrarDialogo(new Clasificacion()),
                AbrirBuscarAmigo = () => MostrarDialogo(new BusquedaAmigo(_amigosServicio)),
                AbrirSolicitudes = () => MostrarDialogo(new Solicitudes(_amigosServicio)),
                ConfirmarEliminarAmigo = MostrarConfirmacionEliminar,
                IniciarJuego = MostrarVentanaJuego,
                UnirseSala = MostrarVentanaJuego
            };

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

            _listaAmigosServicio?.Dispose();
            _amigosServicio?.Dispose();

            if (!_abrioVentanaJuego)
            {
                _salasServicio?.Dispose();
            }
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
            if (ventana == null)
            {
                return;
            }

            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void MostrarVentanaJuego(DTOs.SalaDTO sala)
        {
            _servicioMusica.Detener();
            _servicioMusica.Dispose();

            _abrioVentanaJuego = true;

            var ventanaJuego = new VentanaJuego(sala, _salasServicio);
            ventanaJuego.Show();

            Close();
        }

        private void VentanaPrincipal_Cerrado(object sender, EventArgs e)
        {
            _servicioMusica.Detener();
            _servicioMusica.Dispose();
        }
    }
}