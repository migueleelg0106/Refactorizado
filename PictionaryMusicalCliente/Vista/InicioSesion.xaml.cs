using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Dialogos;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Implementacion;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.InicioSesion;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using log4net;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana principal de acceso a la aplicacion.
    /// </summary>
    public partial class InicioSesion : Window
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IMusicaManejador _musica;
        private readonly IUsuarioAutenticado _usuarioSesion;
        private readonly ISonidoManejador _sonidos;
        private readonly ILocalizadorServicio _traductor;
        private readonly IAvisoServicio _aviso;

        private readonly IInicioSesionServicio _inicioSesion;
        private readonly ICambioContrasenaServicio _cambioPass;
        private readonly IRecuperacionCuentaServicio _recuperacion;
        private readonly ILocalizacionServicio _idioma;
        private readonly INombreInvitadoGenerador _generadorNombres;

        private readonly IWcfClienteEjecutor _ejecutor;
        private readonly IWcfClienteFabrica _fabrica;
        private readonly IManejadorErrorServicio _manejadorError;
        private readonly ICatalogoAvatares _avatares;
        private readonly IValidadorEntrada _validador;
        private readonly ICatalogoImagenesPerfil _imagenesPerfil;
        private readonly IVerificacionCodigoDialogoServicio _verifCodigo;

        private readonly IPerfilServicio _perfil;
        private readonly IClasificacionServicio _clasif;
        private readonly IInvitacionesServicio _invitaciones;
        private readonly IReportesServicio _reportes;
        private readonly IListaAmigosServicio _listaAmigos;
        private readonly IAmigosServicio _amigos;
        private readonly Func<ISalasServicio> _fabricaSalas;

        private bool _navegandoVentanaPrincipal;

        /// <summary>
        /// Inicializa la ventana recibiendo todas las dependencias del sistema.
        /// </summary>
        public InicioSesion(
            IMusicaManejador musica,
            IInicioSesionServicio inicioSesion,
            ICambioContrasenaServicio cambioPass,
            IRecuperacionCuentaServicio recuperacion,
            ILocalizacionServicio idioma,
            Func<ISalasServicio> fabricaSalas,
            IWcfClienteFabrica fabricaWcf,
            IWcfClienteEjecutor ejecutorWcf,
            IManejadorErrorServicio errorWcf,
            ILocalizadorServicio traductor,
            IAvisoServicio aviso,
            ICatalogoAvatares avatares,
            ISonidoManejador sonidos,
            IValidadorEntrada validador,
            IUsuarioAutenticado usuarioSesion,
            ICatalogoImagenesPerfil imagenesPerfil,
            IVerificacionCodigoDialogoServicio verifCodigo,
            INombreInvitadoGenerador generadorNombres,
            IPerfilServicio perfil,
            IClasificacionServicio clasif,
            IInvitacionesServicio invitaciones,
            IReportesServicio reportes,
            IListaAmigosServicio listaAmigos,
            IAmigosServicio amigos)
        {
            InitializeComponent();

            _musica = musica ??
                throw new ArgumentNullException(nameof(musica));
            _ejecutor = ejecutorWcf ??
                throw new ArgumentNullException(nameof(ejecutorWcf));
            _fabrica = fabricaWcf ??
                throw new ArgumentNullException(nameof(fabricaWcf));
            _manejadorError = errorWcf ??
                throw new ArgumentNullException(nameof(errorWcf));
            _traductor = traductor ??
                throw new ArgumentNullException(nameof(traductor));
            _aviso = aviso ??
                throw new ArgumentNullException(nameof(aviso));
            _avatares = avatares ??
                throw new ArgumentNullException(nameof(avatares));
            _sonidos = sonidos ??
                throw new ArgumentNullException(nameof(sonidos));
            _validador = validador ??
                throw new ArgumentNullException(nameof(validador));
            _usuarioSesion = usuarioSesion ??
                throw new ArgumentNullException(nameof(usuarioSesion));
            _idioma = idioma ??
                throw new ArgumentNullException(nameof(idioma));
            _imagenesPerfil = imagenesPerfil ??
                throw new ArgumentNullException(nameof(imagenesPerfil));
            _verifCodigo = verifCodigo ??
                throw new ArgumentNullException(nameof(verifCodigo));

            _perfil = perfil ??
                throw new ArgumentNullException(nameof(perfil));
            _clasif = clasif ??
                throw new ArgumentNullException(nameof(clasif));
            _invitaciones = invitaciones ??
                throw new ArgumentNullException(nameof(invitaciones));
            _reportes = reportes ??
                throw new ArgumentNullException(nameof(reportes));
            _listaAmigos = listaAmigos ??
                throw new ArgumentNullException(nameof(listaAmigos));
            _amigos = amigos ??
                throw new ArgumentNullException(nameof(amigos));
            _cambioPass = cambioPass ??
                throw new ArgumentNullException(nameof(cambioPass));
            _recuperacion = recuperacion ??
                throw new ArgumentNullException(nameof(recuperacion));
            _generadorNombres = generadorNombres ??
                throw new ArgumentNullException(nameof(generadorNombres));
            _inicioSesion = inicioSesion ??
                throw new ArgumentNullException(nameof(inicioSesion));
            _fabricaSalas = fabricaSalas ??
                throw new ArgumentNullException(nameof(fabricaSalas));

            _logger.InfoFormat("InicioSesion constructor - _musica es null: {0}", _musica == null);
            if (_musica != null)
            {
                _logger.InfoFormat("InicioSesion constructor - Tipo de _musica: {0}", _musica.GetType().FullName);
            }

            _musica.ReproducirEnBucle("inicio_sesion_musica.mp3");

            var vm = new InicioSesionVistaModelo(
                _inicioSesion,
                _cambioPass,
                _recuperacion,
                _idioma,
                _traductor,
                _aviso,
                _sonidos,
                _generadorNombres,
                _usuarioSesion,
                _fabricaSalas);

            ConfigurarNavegacion(vm);
            vm.MostrarCamposInvalidos = MarcarCamposInvalidos;

            DataContext = vm;
        }

        private void ConfigurarNavegacion(InicioSesionVistaModelo vm)
        {
            vm.CerrarAccion = Close;

            vm.AbrirCrearCuenta = () =>
            {
                var codigoServ = new VerificacionCodigoServicio(
                    _ejecutor, _fabrica, _traductor, _manejadorError);
                var cuentaServ = new CuentaServicio(_ejecutor, _fabrica, _manejadorError);
                var selectAvatar = new SeleccionAvatarDialogoServicio(
                    _aviso, _avatares, _sonidos); 
                var verifCodigo = new VerificacionCodigoDialogoServicio();

                var vmCrear = new CreacionCuentaVistaModelo(
                    codigoServ, cuentaServ, selectAvatar, verifCodigo,
                    _sonidos, _validador, _avatares, _aviso, _traductor,
                    _idioma);

                var ventana = new CreacionCuenta(vmCrear) { Owner = this };
                ventana.ShowDialog();
            };

            vm.InicioSesionCompletado = _ => NavegarAVentanaPrincipal();

            vm.MostrarIngresoInvitado = vmInvitado =>
            {
                if (vmInvitado == null) return;
                var ventana = new IngresoPartidaInvitado(vmInvitado) { Owner = this };
                ventana.ShowDialog();
            };

            vm.AbrirVentanaJuegoInvitado = (sala, servicio, nombre) =>
            {
                if (sala == null || servicio == null) return;
                NavegarAVentanaJuego(sala, servicio, nombre, true);
            };
        }

        private void NavegarAVentanaPrincipal()
        {
            _navegandoVentanaPrincipal = true;
            _musica.Detener();

            var invitacionSalaServicio = new InvitacionSalaServicio(
                _invitaciones, _listaAmigos, _perfil, _validador, _sonidos, _traductor);

            var principal = new VentanaPrincipal(
                _musica, _listaAmigos, _amigos,
                new SalasServicio(_fabrica, _manejadorError),
                _idioma, _aviso, _perfil, _cambioPass, _recuperacion,
                new SeleccionAvatarDialogoServicio(_aviso, _avatares, _sonidos),
                _avatares, _clasif, _imagenesPerfil, _usuarioSesion,
                _invitaciones, _reportes, _sonidos, _validador, _traductor,
                _fabrica,
                invitacionSalaServicio,
                CrearVentanaInicioSesion
            );

            principal.Show();
            Close();
        }

        private void CrearVentanaInicioSesion()
        {
            _usuarioSesion.Limpiar();

            var ventanaInicio = new InicioSesion(
                _musica,
                _inicioSesion,
                _cambioPass,
                _recuperacion,
                _idioma,
                _fabricaSalas,
                _fabrica,
                _ejecutor,
                _manejadorError,
                _traductor,
                _aviso,
                _avatares,
                _sonidos,
                _validador,
                _usuarioSesion,
                _imagenesPerfil,
                _verifCodigo,
                _generadorNombres,
                _perfil,
                _clasif,
                _invitaciones,
                _reportes,
                _listaAmigos,
                _amigos);

            ventanaInicio.Show();
        }

        private void NavegarAVentanaJuego(
            PictionaryMusicalServidor.Servicios.Contratos.DTOs.SalaDTO sala,
            ISalasServicio servicio,
            string nombre,
            bool esInvitado)
        {
            _logger.InfoFormat("NavegarAVentanaJuego - Deteniendo musica, _musica es null: {0}", _musica == null);
            _musica.Detener();

            var cancionManejador = new CancionManejador();
            var invitacionSalaServicio = new InvitacionSalaServicio(
                _invitaciones, _listaAmigos, _perfil, _validador, _sonidos, _traductor);

            Action irInicioSesion = () =>
            {
                _logger.InfoFormat("irInicioSesion action (invitado) - _musica es null: {0}", _musica == null);
                if (_musica != null)
                {
                    _logger.InfoFormat("irInicioSesion action (invitado) - Tipo de _musica: {0}", _musica.GetType().FullName);
                }
                _usuarioSesion.Limpiar();
                CrearVentanaInicioSesion();
            };

            var ventanaJuego = new Sala(
                sala, servicio, _invitaciones, _reportes, _perfil, _listaAmigos,
                _sonidos, _traductor, _aviso, _usuarioSesion, _validador,
                _fabrica,
                cancionManejador,
                invitacionSalaServicio,
                esInvitado, nombre, irInicioSesion, irInicioSesion);

            ventanaJuego.Show();
            Close();
        }

        private void PasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is InicioSesionVistaModelo vm && sender is PasswordBox pb)
            {
                vm.EstablecerContrasena(pb.Password);
            }
        }

        private void MarcarCamposInvalidos(IList<string> campos)
        {
            ControlVisual.RestablecerEstadoCampo(campoTextoUsuario);
            ControlVisual.RestablecerEstadoCampo(campoContrasenaContrasena);

            if (campos == null) return;

            foreach (var campo in campos)
            {
                if (campo == nameof(InicioSesionVistaModelo.Identificador))
                    ControlVisual.MarcarCampoInvalido(campoTextoUsuario);
                else if (campo == InicioSesionVistaModelo.CampoContrasena)
                    ControlVisual.MarcarCampoInvalido(campoContrasenaContrasena);
            }
        }

        private void InicioSesion_Cerrado(object sender, EventArgs e)
        {
            if (_navegandoVentanaPrincipal)
            {
                return;
            }

            _musica.Detener();
            _musica.Dispose();
        }

        private void BotonAudio_Click(object sender, RoutedEventArgs e)
        {
            bool silenciado = _musica.AlternarSilencio();
            string ruta = silenciado ? "Audio_Apagado.png" : "Audio_Encendido.png";
            imagenBotonAudio.Source = new BitmapImage(
                new Uri($"/Recursos/{ruta}", UriKind.Relative));
        }
    }
}