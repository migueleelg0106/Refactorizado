using System;
using System.Threading.Tasks;
using System.Windows;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using PictionaryMusicalCliente.Servicios.Idiomas;

namespace PictionaryMusicalCliente
{
    public partial class VentanaPrincipal : Window
    {
        private readonly IListaAmigosService _listaAmigosService;
        private readonly IAmigosService _amigosService;

        public VentanaPrincipal()
        {
            InitializeComponent();

            _listaAmigosService = new ListaAmigosService();
            _amigosService = new AmigosService();

            var vistaModelo = new VentanaPrincipalVistaModelo(
                LocalizacionService.Instancia,
                _listaAmigosService,
                _amigosService)
            {
                AbrirPerfil = () => MostrarDialogo(new Perfil()),
                AbrirAjustes = () => MostrarDialogo(new Ajustes()),
                AbrirComoJugar = () => MostrarDialogo(new ComoJugar()),
                AbrirClasificacion = () => MostrarDialogo(new Clasificacion()),
                AbrirBuscarAmigo = MostrarBuscarAmigo,
                AbrirSolicitudes = () => MostrarDialogo(new Solicitudes()),
                AbrirEliminarAmigo = MostrarEliminarAmigo,
                AbrirInvitaciones = () => MostrarDialogo(new Invitaciones()),
                IniciarJuego = _ => MostrarVentanaJuego(),
                UnirseSala = _ => AvisoHelper.Mostrar(Lang.errorTextoNoEncuentraPartida)
            };

            vistaModelo.MostrarMensaje = AvisoHelper.Mostrar;

            DataContext = vistaModelo;
        }

        private async void VentanaPrincipal_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is VentanaPrincipalVistaModelo vistaModelo)
            {
                await vistaModelo.CargarAmigosAsync().ConfigureAwait(true);
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

        private void MostrarBuscarAmigo()
        {
            var ventana = new BuscarAmigo();

            var vistaModelo = new BuscarAmigoVistaModelo(_amigosService)
            {
                MostrarMensaje = AvisoHelper.Mostrar
            };

            ventana.ConfigurarVistaModelo(vistaModelo);
            MostrarDialogo(ventana);
        }

        private void MostrarEliminarAmigo(Amigo amigo)
        {
            if (amigo == null)
            {
                AvisoHelper.Mostrar(Lang.errorTextoAmistadNoRegistrada);
                return;
            }

            var ventana = new EliminarAmigo();

            var vistaModelo = new EliminarAmigoVistaModelo(_amigosService, amigo)
            {
                MostrarMensaje = AvisoHelper.Mostrar,
                AmigoEliminado = eliminado =>
                {
                    if (DataContext is VentanaPrincipalVistaModelo principalVistaModelo)
                    {
                        principalVistaModelo.EliminarAmigoLocal(eliminado);
                    }
                }
            };

            ventana.ConfigurarVistaModelo(vistaModelo);
            MostrarDialogo(ventana);
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