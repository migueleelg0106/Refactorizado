using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Modelo; 
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Utilidades; 
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using PictionaryMusicalCliente.VistaModelo.VentanaPrincipal;
using System;
using System.Windows;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using log4net;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana principal (Lobby) que gestiona la creacion de partidas y navegacion.
    /// </summary>
    public partial class VentanaPrincipal : Window
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IMusicaManejador _musica;
        private readonly IListaAmigosServicio _listaAmigos;
        private readonly IAmigosServicio _amigos;
        private readonly ISalasServicio _salas;
        private readonly ILocalizacionServicio _idioma;
        private readonly IAvisoServicio _aviso;

        private readonly IPerfilServicio _perfilServicio;
        private readonly ICambioContrasenaServicio _cambioPass;
        private readonly IRecuperacionCuentaServicio _recuperacion;
        private readonly ISeleccionarAvatarServicio _selectAvatar;
        private readonly ICatalogoAvatares _avatares;
        private readonly IClasificacionServicio _clasificacion;
        private readonly ICatalogoImagenesPerfil _imagenesPerfil;
        private readonly IUsuarioAutenticado _usuarioSesion;
        private readonly IInvitacionesServicio _invitaciones;
        private readonly IReportesServicio _reportes;
        private readonly ISonidoManejador _sonidos;
        private readonly IValidadorEntrada _validador;
        private readonly ILocalizadorServicio _traductor;
        private readonly IWcfClienteFabrica _fabricaWcf;
        private readonly IInvitacionSalaServicio _invitacionSalaServicio;

        private readonly Action _navegarInicioSesion;

        private readonly VentanaPrincipalVistaModelo _vistaModelo;

        /// <summary>
        /// Constructor por defecto, solo para uso del disenador/XAML. 
        /// La aplicacion debe usar el constructor que recibe dependencias.
        /// </summary>
        public VentanaPrincipal()
        {
        }

        /// <summary>
        /// Inicializa el Lobby con todas las dependencias requeridas.
        /// </summary>
        public VentanaPrincipal(
            IMusicaManejador musica,
            IListaAmigosServicio listaAmigos,
            IAmigosServicio amigos,
            ISalasServicio salas,
            ILocalizacionServicio idioma,
            IAvisoServicio aviso,
            IPerfilServicio perfilServicio,
            ICambioContrasenaServicio cambioPass,
            IRecuperacionCuentaServicio recuperacion,
            ISeleccionarAvatarServicio selectAvatar,
            ICatalogoAvatares avatares,
            IClasificacionServicio clasificacion,
            ICatalogoImagenesPerfil imagenesPerfil,
            IUsuarioAutenticado usuarioSesion,
            IInvitacionesServicio invitaciones,
            IReportesServicio reportes,
            ISonidoManejador sonidos,
            IValidadorEntrada validador,
            ILocalizadorServicio traductor,
            IWcfClienteFabrica fabricaWcf,
            IInvitacionSalaServicio invitacionSalaServicio,
            Action navegarInicioSesion)
        {
            InitializeComponent();

            _musica = musica ??
                throw new ArgumentNullException(nameof(musica));
            _listaAmigos = listaAmigos ??
                throw new ArgumentNullException(nameof(listaAmigos));
            _amigos = amigos ??
                throw new ArgumentNullException(nameof(amigos));
            _salas = salas ??
                throw new ArgumentNullException(nameof(salas));
            _idioma = idioma ??
                throw new ArgumentNullException(nameof(idioma));
            _aviso = aviso ??
                throw new ArgumentNullException(nameof(aviso));
            _perfilServicio = perfilServicio ??
                throw new ArgumentNullException(nameof(perfilServicio));
            _cambioPass = cambioPass ??
                throw new ArgumentNullException(nameof(cambioPass));
            _recuperacion = recuperacion ??
                throw new ArgumentNullException(nameof(recuperacion));
            _selectAvatar = selectAvatar ??
                throw new ArgumentNullException(nameof(selectAvatar));
            _avatares = avatares ??
                throw new ArgumentNullException(nameof(avatares));
            _clasificacion = clasificacion ??
                throw new ArgumentNullException(nameof(clasificacion));
            _imagenesPerfil = imagenesPerfil ??
                throw new ArgumentNullException(nameof(imagenesPerfil));
            _usuarioSesion = usuarioSesion ??
                throw new ArgumentNullException(nameof(usuarioSesion));
            _invitaciones = invitaciones ??
                throw new ArgumentNullException(nameof(invitaciones));
            _reportes = reportes ??
                throw new ArgumentNullException(nameof(reportes));
            _sonidos = sonidos ??
                throw new ArgumentNullException(nameof(sonidos));
            _validador = validador ??
                throw new ArgumentNullException(nameof(validador));
            _traductor = traductor ??
                throw new ArgumentNullException(nameof(traductor));
            _fabricaWcf = fabricaWcf ??
                throw new ArgumentNullException(nameof(fabricaWcf));
            _invitacionSalaServicio = invitacionSalaServicio ??
                throw new ArgumentNullException(nameof(invitacionSalaServicio));
            _navegarInicioSesion = navegarInicioSesion ??
                throw new ArgumentNullException(nameof(navegarInicioSesion));

            _logger.InfoFormat("VentanaPrincipal constructor - _musica es null: {0}", _musica == null);
            if (_musica != null)
            {
                _logger.InfoFormat("VentanaPrincipal constructor - Tipo de _musica: {0}", _musica.GetType().FullName);
            }
            _musica.ReproducirEnBucle("ventana_principal_musica.mp3");

            _vistaModelo = new VentanaPrincipalVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                _idioma,
                _listaAmigos,
                _amigos,
                _salas,
                _sonidos,
                _usuarioSesion);

            ConfigurarNavegacion();
            _vistaModelo.MostrarMensaje = _aviso.Mostrar;

            DataContext = _vistaModelo;

            Loaded += VentanaPrincipal_LoadedAsync;
            Closed += VentanaPrincipal_ClosedAsync;
        }

        private void ConfigurarNavegacion()
        {
            _vistaModelo.AbrirPerfil = AbrirPerfil;
            _vistaModelo.AbrirAjustes = AbrirAjustes;
            _vistaModelo.AbrirComoJugar = () => MostrarDialogo(new ComoJugar());
            _vistaModelo.AbrirClasificacion = AbrirClasificacion;
            _vistaModelo.AbrirBuscarAmigo = AbrirBuscarAmigo;
            _vistaModelo.AbrirSolicitudes = AbrirSolicitudes;
            _vistaModelo.ConfirmarEliminarAmigo = MostrarConfirmacionEliminar;
            _vistaModelo.IniciarJuego = sala => MostrarVentanaJuego(sala);
            _vistaModelo.UnirseSala = sala => MostrarVentanaJuego(sala);
        }

        private void AbrirPerfil()
        {
            var vmPerfil = new PerfilVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                _perfilServicio,
                _selectAvatar,
                _cambioPass,
                _recuperacion,
                _aviso,
                _sonidos,
                _usuarioSesion,
                _avatares,
                _validador,
                _imagenesPerfil);

            vmPerfil.SolicitarReinicioSesion = ReiniciarAplicacion;
            App.VentanaServicio.MostrarVentanaDialogo(vmPerfil);
        }

        private void AbrirAjustes()
        {
            var ajustesVM = new VistaModelo.Ajustes.AjustesVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                _musica,
                _sonidos);
            App.VentanaServicio.MostrarVentanaDialogo(ajustesVM);
        }

        private void AbrirClasificacion()
        {
            var clasificacionVM = new ClasificacionVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                _clasificacion,
                _aviso,
                _sonidos);
            App.VentanaServicio.MostrarVentanaDialogo(clasificacionVM);
        }

        private void AbrirBuscarAmigo()
        {
            var busquedaAmigoVM = new VistaModelo.Amigos.BusquedaAmigoVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                _amigos,
                _sonidos,
                _aviso,
                _usuarioSesion);
            App.VentanaServicio.MostrarVentanaDialogo(busquedaAmigoVM);
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

            _musica.Detener();
        }

        private bool? MostrarConfirmacionEliminar(string amigo)
        {
            var eliminacionVM = new VistaModelo.Amigos.EliminacionAmigoVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                _sonidos,
                amigo);
            App.VentanaServicio.MostrarVentanaDialogo(eliminacionVM);
            return eliminacionVM.DialogResult;
        }

        private void AbrirSolicitudes()
        {
            var solicitudesVM = new VistaModelo.Amigos.SolicitudesVistaModelo(
                App.VentanaServicio,
                App.Localizador,
                _amigos,
                _sonidos,
                _aviso,
                _usuarioSesion);
            App.VentanaServicio.MostrarVentanaDialogo(solicitudesVM);
        }

        private void MostrarDialogo(Window ventana)
        {
            if (ventana == null) return;
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void MostrarVentanaJuego(DTOs.SalaDTO sala)
        {
            _logger.InfoFormat("MostrarVentanaJuego - Deteniendo musica, _musica es null: {0}", _musica == null);
            _musica.Detener();

            Action irMenu = () =>
            {
                _logger.InfoFormat("irMenu action - Creando nueva VentanaPrincipal, _musica es null: {0}", _musica == null);
                if (_musica != null)
                {
                    _logger.InfoFormat("irMenu action - Tipo de _musica: {0}", _musica.GetType().FullName);
                }
                var nuevaPrincipal = new VentanaPrincipal(
                    _musica, _listaAmigos, _amigos, _salas,
                    _idioma, _aviso, _perfilServicio, _cambioPass,
                    _recuperacion, _selectAvatar, _avatares,
                    _clasificacion, _imagenesPerfil, _usuarioSesion,
                    _invitaciones, _reportes, _sonidos,
                    _validador, _traductor, _fabricaWcf, _invitacionSalaServicio,
                    _navegarInicioSesion);

                nuevaPrincipal.Show();
            };

            Action irInicioSesion = () =>
            {
                _logger.InfoFormat("irInicioSesion action - Navegando a inicio sesion, _musica es null: {0}", _musica == null);
                NavegarAInicioSesion();
            };

            var ventanaJuego = new Sala(
                sala, _salas, _invitaciones, _reportes, _perfilServicio,
                _listaAmigos, _sonidos, _traductor, _aviso, _usuarioSesion,
                _validador, _fabricaWcf, new CancionManejador(), _invitacionSalaServicio,
                false,
                _usuarioSesion.NombreUsuario,
                irMenu,
                irInicioSesion
            );

            ventanaJuego.Show();
            Close();
        }

        private void ReiniciarAplicacion()
        {
            CerrarVentanasSecundarias();
            NavegarAInicioSesion();
            Close();
        }

        private void NavegarAInicioSesion()
        {
            _usuarioSesion.Limpiar();
            _navegarInicioSesion?.Invoke();
        }

        private void CerrarVentanasSecundarias()
        {
            foreach (Window ventana in Application.Current.Windows)
            {
                if (!ReferenceEquals(ventana, this))
                {
                    ventana.Close();
                }
            }
        }
    }
}