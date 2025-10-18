using System;
using System.Windows;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class VentanaPrincipal : Window
    {
        private readonly IDialogService _dialogService;

        public VentanaPrincipal()
        {
            InitializeComponent();

            _dialogService = new DialogService();

            var vistaModelo = new VentanaPrincipalVistaModelo(_dialogService)
            {
                AbrirPerfil = () => MostrarDialogo(new Perfil()),
                AbrirAjustes = () => MostrarDialogo(new Ajustes()),
                AbrirComoJugar = () => MostrarDialogo(new ComoJugar()),
                AbrirClasificacion = () => MostrarDialogo(new Clasificacion()),
                AbrirBuscarAmigo = () => MostrarDialogo(new BuscarAmigo()),
                AbrirSolicitudes = () => MostrarDialogo(new Solicitudes()),
                AbrirEliminarAmigo = () => MostrarDialogo(new EliminarAmigo()),
                AbrirInvitaciones = () => MostrarDialogo(new Invitaciones()),
                IniciarJuego = _ => MostrarVentanaJuego(),
                UnirseSala = _ => _dialogService.Aviso(Lang.errorTextoNoEncuentraPartida)
            };

            DataContext = vistaModelo;
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