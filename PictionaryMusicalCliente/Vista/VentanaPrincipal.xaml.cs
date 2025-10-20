using System;
using System.Threading.Tasks;
using System.Windows;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Idiomas;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using PictionaryMusicalCliente.Sesiones;

namespace PictionaryMusicalCliente
{
    public partial class VentanaPrincipal : Window
    {
        private readonly IAmigosService _amigosService;

        public VentanaPrincipal()
        {
            InitializeComponent();

            Loaded += VentanaPrincipal_Loaded;

            IAmigosService amigosService = AmigosService.Instancia;
            _amigosService = amigosService;

            var vistaModelo = new VentanaPrincipalVistaModelo(LocalizacionService.Instancia, amigosService)
            {
                AbrirPerfil = () => MostrarDialogo(new Perfil()),
                AbrirAjustes = () => MostrarDialogo(new Ajustes()),
                AbrirComoJugar = () => MostrarDialogo(new ComoJugar()),
                AbrirClasificacion = () => MostrarDialogo(new Clasificacion()),
                AbrirBuscarAmigo = () => MostrarDialogo(new BuscarAmigo(amigosService)),
                AbrirSolicitudes = () => MostrarDialogo(new Solicitudes(amigosService)),
                AbrirEliminarAmigo = amigo => MostrarDialogo(new EliminarAmigo(amigo, amigosService)),
                AbrirInvitaciones = () => MostrarDialogo(new Invitaciones()),
                IniciarJuego = _ => MostrarVentanaJuego(),
                UnirseSala = _ => AvisoHelper.Mostrar(Lang.errorTextoNoEncuentraPartida)
            };

            vistaModelo.MostrarMensaje = AvisoHelper.Mostrar;

            DataContext = vistaModelo;

            Closed += VentanaPrincipal_Closed;
        }

        private async void VentanaPrincipal_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is VentanaPrincipalVistaModelo vistaModelo)
            {
                await vistaModelo.InicializarAsync().ConfigureAwait(true);
            }
        }

        private async void VentanaPrincipal_Closed(object sender, EventArgs e)
        {
            string nombreUsuario = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(nombreUsuario) || _amigosService == null)
            {
                return;
            }

            try
            {
                await _amigosService.DesuscribirseAsync(nombreUsuario).ConfigureAwait(true);
            }
            catch
            {
                // Se ignoran excepciones durante el cierre de la ventana.
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
    }
}