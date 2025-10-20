using System;
using System.Windows;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class VentanaPrincipal : Window
    {
        private readonly VentanaPrincipalVistaModelo _vistaModelo;

        public VentanaPrincipal()
        {
            InitializeComponent();

            _vistaModelo = new VentanaPrincipalVistaModelo
            {
                AbrirPerfil = () => MostrarDialogo(new Perfil()),
                AbrirAjustes = () => MostrarDialogo(new Ajustes()),
                AbrirComoJugar = () => MostrarDialogo(new ComoJugar()),
                AbrirClasificacion = () => MostrarDialogo(new Clasificacion()),
                AbrirBuscarAmigo = () => MostrarDialogo(new BuscarAmigo()),
                AbrirSolicitudes = () => MostrarDialogo(new Solicitudes()),
                AbrirEliminarAmigo = nombre => MostrarDialogoEliminarAmigo(nombre),
                AbrirInvitaciones = () => MostrarDialogo(new Invitaciones()),
                IniciarJuego = _ => MostrarVentanaJuego(),
                UnirseSala = _ => AvisoHelper.Mostrar(Lang.errorTextoNoEncuentraPartida)
            };

            _vistaModelo.MostrarMensaje = AvisoHelper.Mostrar;

            DataContext = _vistaModelo;
        }

        private async void VentanaPrincipal_Loaded(object sender, RoutedEventArgs e)
        {
            if (_vistaModelo != null)
            {
                await _vistaModelo.CargarAmigosAsync().ConfigureAwait(true);
            }
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

        private void MostrarDialogoEliminarAmigo(string nombreAmigo)
        {
            if (string.IsNullOrWhiteSpace(nombreAmigo))
            {
                AvisoHelper.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            var ventana = new EliminarAmigo(
                nombreAmigo,
                eliminado => _vistaModelo?.RemoverAmigo(eliminado));

            MostrarDialogo(ventana);
        }
    }
}