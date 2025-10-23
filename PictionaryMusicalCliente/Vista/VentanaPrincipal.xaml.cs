using System;
using System.Windows;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Idiomas;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class VentanaPrincipal : Window
    {
        private readonly IListaAmigosServicio _listaAmigosService;
        private readonly IAmigosServicio _amigosService;
        private readonly VentanaPrincipalVistaModelo _vistaModelo;

        public VentanaPrincipal()
        {
            InitializeComponent();

            _listaAmigosService = new ListaAmigosServicio();
            _amigosService = new AmigosServicio();

            _vistaModelo = new VentanaPrincipalVistaModelo(
                LocalizacionServicio.Instancia,
                _listaAmigosService,
                _amigosService)
            {
                AbrirPerfil = () => MostrarDialogo(new Perfil()),
                AbrirAjustes = () => MostrarDialogo(new Ajustes()),
                AbrirComoJugar = () => MostrarDialogo(new ComoJugar()),
                AbrirClasificacion = () => MostrarDialogo(new Clasificacion()),
                AbrirBuscarAmigo = () => MostrarDialogo(new BuscarAmigo(_amigosService)),
                AbrirSolicitudes = () => MostrarDialogo(new Solicitudes(_amigosService)),
                ConfirmarEliminarAmigo = MostrarConfirmacionEliminar,
                AbrirInvitaciones = () => MostrarDialogo(new Invitaciones()),
                IniciarJuego = _ => MostrarVentanaJuego(),
                UnirseSala = _ => AvisoAyudante.Mostrar(Lang.errorTextoNoEncuentraPartida)
            };

            _vistaModelo.MostrarMensaje = AvisoAyudante.Mostrar;

            DataContext = _vistaModelo;

            Loaded += VentanaPrincipal_Loaded;
            Closed += VentanaPrincipal_Closed;
        }

        private async void VentanaPrincipal_Loaded(object sender, RoutedEventArgs e)
        {
            await _vistaModelo.InicializarAsync().ConfigureAwait(true);
        }

        private async void VentanaPrincipal_Closed(object sender, EventArgs e)
        {
            Loaded -= VentanaPrincipal_Loaded;
            Closed -= VentanaPrincipal_Closed;

            await _vistaModelo.FinalizarAsync().ConfigureAwait(false);

            _listaAmigosService?.Dispose();
            _amigosService?.Dispose();
        }

        private bool? MostrarConfirmacionEliminar(string amigo)
        {
            var ventana = new EliminarAmigo(amigo)
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

        private void MostrarVentanaJuego()
        {
            var ventana = new VentanaJuego
            {
                Owner = this
            };

            ventana.Show();
        }
    }
}