using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Idiomas;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Sesiones;

namespace PictionaryMusicalCliente
{
    public partial class VentanaPrincipal : Window
    {
        private readonly IListaAmigosService _listaAmigosService;
        private readonly IAmigosService _amigosService;
        private readonly VentanaPrincipalVistaModelo _vistaModelo;

        public VentanaPrincipal()
        {
            InitializeComponent();

            ILocalizacionService localizacionService = LocalizacionService.Instancia;
            _listaAmigosService = new ListaAmigosService();
            _amigosService = new AmigosService();

            _amigosService.SolicitudAmistadNotificada += AmigosService_SolicitudAmistadNotificada;
            _amigosService.SolicitudAmistadRespondidaNotificada += AmigosService_SolicitudAmistadRespondidaNotificada;
            _amigosService.AmistadEliminadaNotificada += AmigosService_AmistadEliminadaNotificada;

            _vistaModelo = new VentanaPrincipalVistaModelo(localizacionService, _listaAmigosService)
            {
                AbrirPerfil = () => MostrarDialogo(new Perfil()),
                AbrirAjustes = () => MostrarDialogo(new Ajustes()),
                AbrirComoJugar = () => MostrarDialogo(new ComoJugar()),
                AbrirClasificacion = () => MostrarDialogo(new Clasificacion()),
                AbrirBuscarAmigo = () =>
                {
                    var ventana = new BuscarAmigo(_amigosService)
                    {
                        VistaModeloPrincipal = _vistaModelo
                    };

                    MostrarDialogo(ventana);
                },
                AbrirSolicitudes = () => MostrarDialogo(new Solicitudes()),
                AbrirEliminarAmigo = amigo =>
                {
                    if (string.IsNullOrWhiteSpace(amigo))
                    {
                        AvisoHelper.Mostrar(Lang.amigosTextoNoSeleccionado);
                        return;
                    }

                    var ventana = new EliminarAmigo(amigo, _amigosService)
                    {
                        VistaModeloPrincipal = _vistaModelo
                    };

                    MostrarDialogo(ventana);
                },
                AbrirInvitaciones = () => MostrarDialogo(new Invitaciones()),
                IniciarJuego = _ => MostrarVentanaJuego(),
                UnirseSala = _ => AvisoHelper.Mostrar(Lang.errorTextoNoEncuentraPartida)
            };

            _vistaModelo.MostrarMensaje = AvisoHelper.Mostrar;

            DataContext = _vistaModelo;

            Closed += VentanaPrincipalClosed;
        }

        private void VentanaPrincipalClosed(object sender, EventArgs e)
        {
            Closed -= VentanaPrincipalClosed;
            if (_amigosService != null)
            {
                _amigosService.SolicitudAmistadNotificada -= AmigosService_SolicitudAmistadNotificada;
                _amigosService.SolicitudAmistadRespondidaNotificada -= AmigosService_SolicitudAmistadRespondidaNotificada;
                _amigosService.AmistadEliminadaNotificada -= AmigosService_AmistadEliminadaNotificada;
            }

            _vistaModelo?.Dispose();
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

        private async void AmigosService_SolicitudAmistadNotificada(object sender, SolicitudAmistadNotificacion e)
        {
            if (e == null)
            {
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return;
            }

            if (string.Equals(usuarioActual, e.Receptor, StringComparison.OrdinalIgnoreCase))
            {
                string mensaje = string.Format(
                    CultureInfo.CurrentUICulture,
                    Lang.amigosTextoNotificacionSolicitudRecibida,
                    e.Remitente);
                AvisoHelper.Mostrar(mensaje);
            }

            await RecargarAmigosAsync().ConfigureAwait(true);
        }

        private async void AmigosService_SolicitudAmistadRespondidaNotificada(
            object sender,
            RespuestaSolicitudAmistadNotificacion e)
        {
            if (e == null)
            {
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return;
            }

            if (string.Equals(usuarioActual, e.Remitente, StringComparison.OrdinalIgnoreCase))
            {
                string plantilla = e.Aceptada
                    ? Lang.amigosTextoNotificacionSolicitudAceptada
                    : Lang.amigosTextoNotificacionSolicitudRechazada;

                string mensaje = string.Format(CultureInfo.CurrentUICulture, plantilla, e.Receptor);
                AvisoHelper.Mostrar(mensaje);

                if (e.Aceptada)
                {
                    await RecargarAmigosAsync().ConfigureAwait(true);
                }

                return;
            }

            if (e.Aceptada && string.Equals(usuarioActual, e.Receptor, StringComparison.OrdinalIgnoreCase))
            {
                AvisoHelper.Mostrar(Lang.amigosTextoListaActualizada);
                await RecargarAmigosAsync().ConfigureAwait(true);
            }
        }

        private async void AmigosService_AmistadEliminadaNotificada(object sender, AmistadEliminadaNotificacion e)
        {
            if (e == null)
            {
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return;
            }

            if (!e.OperacionLocal && string.Equals(usuarioActual, e.Jugador, StringComparison.OrdinalIgnoreCase))
            {
                AvisoHelper.Mostrar(Lang.amigosTextoListaActualizada);
            }

            await RecargarAmigosAsync().ConfigureAwait(true);
        }

        private Task RecargarAmigosAsync()
        {
            return _vistaModelo?.RecargarAmigosAsync() ?? Task.CompletedTask;
        }
    }
}
