using log4net;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.PictionaryServidorServicioCursoPartida;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Ajustes;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using PictionaryMusicalCliente.VistaModelo.InicioSesion;
using PictionaryMusicalCliente.VistaModelo.VentanaPrincipal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Salas
{
    public class SalaVistaModelo : BaseVistaModelo, ICursoPartidaManejadorCallback
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int MaximoJugadoresSala = 4;
        private static readonly StringComparer ComparadorJugadores =
            StringComparer.OrdinalIgnoreCase;

        private ISalasServicio _salasServicio;
        private IInvitacionSalaServicio _invitacionSalaServicio;
        private IReportesServicio _reportesServicio;
        private SonidoManejador _sonidoManejador;
        private IAvisoServicio _avisoServicio;
        private IUsuarioAutenticado _usuarioSesion;
        private IWcfClienteFabrica _fabricaClientes;
        private CancionManejador _cancionManejador;

        private readonly DTOs.SalaDTO _sala;
        private readonly string _nombreUsuarioSesion;
        private readonly bool _esInvitado;
        private HashSet<int> _amigosInvitados;
        private readonly bool _esHost;
        private readonly string _idJugador;

        private PartidaIniciadaVistaModelo _partidaVistaModelo;
        private ChatVistaModelo _chatVistaModelo;

        private ICursoPartidaManejador _proxyJuego;

        private string _textoBotonIniciarPartida;
        private bool _botonIniciarPartidaHabilitado;
        private bool _mostrarBotonIniciarPartida;
        private string _codigoSala;
        private ObservableCollection<JugadorElemento> _jugadores;
        private string _correoInvitacion;
        private bool _puedeInvitarPorCorreo;
        private bool _puedeInvitarAmigos;
        private bool _aplicacionCerrando;
        private HashSet<string> _adivinadoresQuienYaAcertaron;
        private string _nombreDibujanteActual;
        private bool _rondaTerminadaTemprano;
        private string _mensajeChat;
        private bool _salaCancelada;

        private const int LimiteCaracteresChat = 150;
        private const double PorcentajePuntosDibujante = 0.2;

        /// <summary>
        /// Define los destinos posibles al salir de la partida.
        /// </summary>
        public enum DestinoNavegacion
        {
            InicioSesion,
            VentanaPrincipal
        }

        /// <param name="sala">Datos de la sala actual.</param>
        /// <param name="salasServicio">Servicio de comunicacion de salas.</param>
        /// <param name="invitacionesServicio">Servicio para invitar usuarios.</param>
        /// <param name="listaAmigosServicio">Servicio para obtener amigos.</param>
        /// <param name="perfilServicio">Servicio de perfil de usuario.</param>
        /// <param name="nombreJugador">Nombre del jugador actual (opcional).</param>
        /// <param name="esInvitado">Indica si el usuario es invitado.</param>
        public SalaVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            DTOs.SalaDTO sala,
            ISalasServicio salasServicio,
            IInvitacionesServicio invitacionesServicio,
            IListaAmigosServicio listaAmigosServicio,
            IPerfilServicio perfilServicio,
            IReportesServicio reportesServicio,
            SonidoManejador sonidoManejador,
            IAvisoServicio avisoServicio,
            IUsuarioAutenticado usuarioSesion,
            IInvitacionSalaServicio invitacionSalaServicio,
            IWcfClienteFabrica fabricaClientes,
            CancionManejador cancionManejador,
            string nombreJugador = null,
            bool esInvitado = false)
            : base(ventana, localizador)
        {
            ValidarDependencias(sala, salasServicio, reportesServicio, sonidoManejador,
                avisoServicio, invitacionSalaServicio, usuarioSesion, fabricaClientes,
                cancionManejador);

            AsignarServicios(salasServicio, reportesServicio, sonidoManejador, avisoServicio,
                invitacionSalaServicio, usuarioSesion, fabricaClientes, cancionManejador);

            _sala = sala;
            _esInvitado = esInvitado;
            _nombreUsuarioSesion = !string.IsNullOrWhiteSpace(nombreJugador)
                ? nombreJugador
                : _usuarioSesion.NombreUsuario ?? string.Empty;
            _esHost = string.Equals(_sala.Creador, _nombreUsuarioSesion,
                StringComparison.OrdinalIgnoreCase);
            _idJugador = ObtenerIdentificadorJugador();

            InicializarColecciones();
            InicializarViewModelsHijos();
            ConfigurarEventosViewModels();
            InicializarEstadoInicial();
            SuscribirEventosServicio();
            InicializarComandos();
            ConfigurarPermisos();
            InicializarProxyPartida();
        }

        private void ValidarDependencias(DTOs.SalaDTO sala, ISalasServicio salasServicio,
            IReportesServicio reportesServicio, SonidoManejador sonidoManejador,
            IAvisoServicio avisoServicio, IInvitacionSalaServicio invitacionSalaServicio,
            IUsuarioAutenticado usuarioSesion, IWcfClienteFabrica fabricaClientes,
            CancionManejador cancionManejador)
        {
            if (sala == null) throw new ArgumentNullException(nameof(sala));
            if (salasServicio == null) throw new ArgumentNullException(nameof(salasServicio));
            if (reportesServicio == null)
                throw new ArgumentNullException(nameof(reportesServicio));
            if (sonidoManejador == null)
                throw new ArgumentNullException(nameof(sonidoManejador));
            if (avisoServicio == null) throw new ArgumentNullException(nameof(avisoServicio));
            if (invitacionSalaServicio == null)
                throw new ArgumentNullException(nameof(invitacionSalaServicio));
            if (usuarioSesion == null)
                throw new ArgumentNullException(nameof(usuarioSesion));
            if (fabricaClientes == null)
                throw new ArgumentNullException(nameof(fabricaClientes));
            if (cancionManejador == null)
                throw new ArgumentNullException(nameof(cancionManejador));
        }

        private void AsignarServicios(ISalasServicio salasServicio,
            IReportesServicio reportesServicio, SonidoManejador sonidoManejador,
            IAvisoServicio avisoServicio, IInvitacionSalaServicio invitacionSalaServicio,
            IUsuarioAutenticado usuarioSesion, IWcfClienteFabrica fabricaClientes,
            CancionManejador cancionManejador)
        {
            _salasServicio = salasServicio;
            _reportesServicio = reportesServicio;
            _sonidoManejador = sonidoManejador;
            _avisoServicio = avisoServicio;
            _invitacionSalaServicio = invitacionSalaServicio;
            _usuarioSesion = usuarioSesion;
            _fabricaClientes = fabricaClientes;
            _cancionManejador = cancionManejador;
        }

        private void InicializarColecciones()
        {
            _amigosInvitados = new HashSet<int>();
            _adivinadoresQuienYaAcertaron =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _nombreDibujanteActual = string.Empty;
            _rondaTerminadaTemprano = false;
        }

        private void InicializarViewModelsHijos()
        {
            _partidaVistaModelo = new PartidaIniciadaVistaModelo(
                _ventana,
                _localizador,
                _sonidoManejador,
                _cancionManejador);
            _chatVistaModelo = CrearChatVistaModelo();
        }

        private void ConfigurarEventosViewModels()
        {
            _chatVistaModelo.PropertyChanged += ChatVistaModelo_PropertyChanged;
            _partidaVistaModelo.PropertyChanged += PartidaIniciadaVistaModelo_PropertyChanged;
            ConfigurarPartidaVistaModelo();
        }

        private void InicializarEstadoInicial()
        {
            _textoBotonIniciarPartida = Lang.partidaAdminTextoIniciarPartida;
            _botonIniciarPartidaHabilitado = _esHost;
            _mostrarBotonIniciarPartida = _esHost;
            _codigoSala = _sala.Codigo;
            _jugadores = new ObservableCollection<JugadorElemento>();
            ActualizarJugadores(_sala.Jugadores);
            _puedeInvitarPorCorreo = true;
        }

        private void SuscribirEventosServicio()
        {
            _salasServicio.JugadorSeUnio += SalasServicio_JugadorSeUnio;
            _salasServicio.JugadorSalio += SalasServicio_JugadorSalio;
            _salasServicio.JugadorExpulsado += SalasServicio_JugadorExpulsado;
            _salasServicio.SalaActualizada += SalasServicio_SalaActualizada;
            _salasServicio.SalaCancelada += SalasServicio_SalaCancelada;
        }

        private void ConfigurarPermisos()
        {
            PuedeInvitarPorCorreo = !_esInvitado;
            PuedeInvitarAmigos = !_esInvitado;
            _chatVistaModelo.PuedeEscribir = true;
        }

        private void ConfigurarPartidaVistaModelo()
        {
            _partidaVistaModelo.JuegoIniciadoCambiado += OnJuegoIniciadoCambiado;
            _partidaVistaModelo.PuedeEscribirCambiado += valor =>
                _chatVistaModelo.PuedeEscribir = valor;
            _partidaVistaModelo.EsDibujanteCambiado += valor =>
                _chatVistaModelo.EsDibujante = valor;
            _partidaVistaModelo.NombreCancionCambiado += valor =>
                _chatVistaModelo.NombreCancionCorrecta = valor;
            _partidaVistaModelo.TiempoRestanteCambiado += valor =>
                _chatVistaModelo.TiempoRestante = valor;
            _partidaVistaModelo.EnviarTrazoAlServidor = EnviarTrazoAlServidor;
            _partidaVistaModelo.CelebracionFinRondaTerminada += OnCelebracionFinRondaTerminada;
        }

        private void OnCelebracionFinRondaTerminada()
        {
            _rondaTerminadaTemprano = false;
        }

        private ChatVistaModelo CrearChatVistaModelo()
        {
            var chatMensajeria = new ChatMensajeriaDelegado(EjecutarEnviarMensaje);
            var chatAciertos = new ChatAciertosDelegado(
                () => _nombreUsuarioSesion,
                EjecutarRegistrarAciertoAsync);
            var chatReglas = new ChatReglasPartida(chatAciertos);

            return new ChatVistaModelo(_ventana, _localizador, chatMensajeria, chatReglas);
        }


        private void ChatVistaModelo_PropertyChanged(object remitente, PropertyChangedEventArgs argumentosEvento)
        {
            if (string.Equals(
                argumentosEvento.PropertyName,
                nameof(ChatVistaModelo.PuedeEscribir),
                StringComparison.Ordinal))
            {
                NotificarCambio(nameof(PuedeEscribir));
            }
        }

        private void PartidaIniciadaVistaModelo_PropertyChanged(object remitente, System.ComponentModel.PropertyChangedEventArgs argumentosEvento)
        {
            NotificarCambio(argumentosEvento.PropertyName);
        }

        private void OnJuegoIniciadoCambiado(bool juegoIniciado)
        {
            MostrarBotonIniciarPartida = _esHost && !juegoIniciado;
            ActualizarVisibilidadBotonesExpulsion();
            ActualizarVisibilidadBotonesReporte();
            _chatVistaModelo.EsPartidaIniciada = juegoIniciado;
        }

        public PartidaIniciadaVistaModelo PartidaVistaModelo => _partidaVistaModelo;

        public ChatVistaModelo ChatVistaModelo => _chatVistaModelo;

        public bool JuegoIniciado => _partidaVistaModelo.JuegoIniciado;

        public bool EsHost => _esHost;

        public int NumeroRondaActual => _partidaVistaModelo.NumeroRondaActual;

        public double Grosor
        {
            get => _partidaVistaModelo.Grosor;
            set => _partidaVistaModelo.Grosor = value;
        }

        public Color Color
        {
            get => _partidaVistaModelo.Color;
            set => _partidaVistaModelo.Color = value;
        }

        public string TextoContador => _partidaVistaModelo.TextoContador;

        public Brush ColorContador => _partidaVistaModelo.ColorContador;

        public bool MostrarEstadoRonda => _partidaVistaModelo.MostrarEstadoRonda;

        public bool EsHerramientaLapiz
        {
            get => _partidaVistaModelo.EsHerramientaLapiz;
            set => _partidaVistaModelo.EsHerramientaLapiz = value;
        }

        public bool EsHerramientaBorrador
        {
            get => _partidaVistaModelo.EsHerramientaBorrador;
            set => _partidaVistaModelo.EsHerramientaBorrador = value;
        }

        public Visibility VisibilidadCuadriculaDibujo
        {
            get => _partidaVistaModelo.VisibilidadCuadriculaDibujo;
            set => _partidaVistaModelo.VisibilidadCuadriculaDibujo = value;
        }

        public Visibility VisibilidadOverlayDibujante
        {
            get => _partidaVistaModelo.VisibilidadOverlayDibujante;
            set => _partidaVistaModelo.VisibilidadOverlayDibujante = value;
        }

        public Visibility VisibilidadOverlayAdivinador
        {
            get => _partidaVistaModelo.VisibilidadOverlayAdivinador;
            set => _partidaVistaModelo.VisibilidadOverlayAdivinador = value;
        }

        public Visibility VisibilidadOverlayAlarma
        {
            get => _partidaVistaModelo.VisibilidadOverlayAlarma;
            set => _partidaVistaModelo.VisibilidadOverlayAlarma = value;
        }

        public Visibility VisibilidadPalabraAdivinar
        {
            get => _partidaVistaModelo.VisibilidadPalabraAdivinar;
            set => _partidaVistaModelo.VisibilidadPalabraAdivinar = value;
        }

        public Visibility VisibilidadInfoCancion
        {
            get => _partidaVistaModelo.VisibilidadInfoCancion;
            set => _partidaVistaModelo.VisibilidadInfoCancion = value;
        }

        public Visibility VisibilidadArtista
        {
            get => _partidaVistaModelo.VisibilidadArtista;
            set => _partidaVistaModelo.VisibilidadArtista = value;
        }

        public Visibility VisibilidadGenero
        {
            get => _partidaVistaModelo.VisibilidadGenero;
            set => _partidaVistaModelo.VisibilidadGenero = value;
        }

        public string PalabraAdivinar
        {
            get => _partidaVistaModelo.PalabraAdivinar;
            set => _partidaVistaModelo.PalabraAdivinar = value;
        }

        public Brush ColorPalabraAdivinar
        {
            get => _partidaVistaModelo.ColorPalabraAdivinar;
            set => _partidaVistaModelo.ColorPalabraAdivinar = value;
        }

        public string TextoArtista
        {
            get => _partidaVistaModelo.TextoArtista;
            set => _partidaVistaModelo.TextoArtista = value;
        }

        public string TextoGenero
        {
            get => _partidaVistaModelo.TextoGenero;
            set => _partidaVistaModelo.TextoGenero = value;
        }

        public string TextoDibujoDe => _partidaVistaModelo.TextoDibujoDe;

        public string TextoBotonIniciarPartida
        {
            get => _textoBotonIniciarPartida;
            set => EstablecerPropiedad(ref _textoBotonIniciarPartida, value);
        }

        public bool BotonIniciarPartidaHabilitado
        {
            get => _botonIniciarPartidaHabilitado;
            set => EstablecerPropiedad(ref _botonIniciarPartidaHabilitado, value);
        }

        public bool MostrarBotonIniciarPartida
        {
            get => _mostrarBotonIniciarPartida;
            private set => EstablecerPropiedad(ref _mostrarBotonIniciarPartida, value);
        }

        public string CodigoSala
        {
            get => _codigoSala;
            set => EstablecerPropiedad(ref _codigoSala, value);
        }

        public ObservableCollection<JugadorElemento> Jugadores
        {
            get => _jugadores;
            set => EstablecerPropiedad(ref _jugadores, value);
        }

        public string CorreoInvitacion
        {
            get => _correoInvitacion;
            set => EstablecerPropiedad(ref _correoInvitacion, value);
        }

        public bool PuedeInvitarPorCorreo
        {
            get => _puedeInvitarPorCorreo;
            private set
            {
                if (EstablecerPropiedad(ref _puedeInvitarPorCorreo, value))
                {
                    NotificarComando(InvitarCorreoComando);
                    NotificarComando(InvitarAmigosComando);
                }
            }
        }

        public bool PuedeInvitarAmigos
        {
            get => _puedeInvitarAmigos;
            private set
            {
                if (EstablecerPropiedad(ref _puedeInvitarAmigos, value))
                {
                    NotificarComando(InvitarAmigosComando);
                }
            }
        }

        public bool EsInvitado => _esInvitado;

        public bool PuedeEscribir
        {
            get => _chatVistaModelo.PuedeEscribir;
            private set => _chatVistaModelo.PuedeEscribir = value;
        }

        public bool EsDibujante => _partidaVistaModelo.EsDibujante;

        public string MensajeChat
        {
            get => _mensajeChat;
            set => EstablecerPropiedad(ref _mensajeChat, LimitarMensajePorCaracteres(value));
        }

        public ICommand InvitarCorreoComando { get; private set; }

        public IComandoAsincrono InvitarAmigosComando { get; private set; }

        public ICommand AbrirAjustesComando { get; private set; }

        public ICommand IniciarPartidaComando { get; private set; }

        public ICommand SeleccionarLapizComando => _partidaVistaModelo.SeleccionarLapizComando;

        public ICommand SeleccionarBorradorComando => _partidaVistaModelo.SeleccionarBorradorComando;

        public ICommand CambiarGrosorComando => _partidaVistaModelo.CambiarGrosorComando;

        public ICommand CambiarColorComando => _partidaVistaModelo.CambiarColorComando;

        public ICommand LimpiarDibujoComando => _partidaVistaModelo.LimpiarDibujoComando;

        public ICommand OcultarOverlayAlarmaComando => _partidaVistaModelo.OcultarOverlayAlarmaComando;

        public ICommand CerrarVentanaComando { get; private set; }

        public ICommand EnviarMensajeChatComando { get; private set; }

        public Action<bool> NotificarCambioHerramienta
        {
            get => _partidaVistaModelo.NotificarCambioHerramienta;
            set => _partidaVistaModelo.NotificarCambioHerramienta = value;
        }

        public Action AplicarEstiloLapiz
        {
            get => _partidaVistaModelo.AplicarEstiloLapiz;
            set => _partidaVistaModelo.AplicarEstiloLapiz = value;
        }

        public Action ActualizarFormaGoma
        {
            get => _partidaVistaModelo.ActualizarFormaGoma;
            set => _partidaVistaModelo.ActualizarFormaGoma = value;
        }

        public Action LimpiarTrazos
        {
            get => _partidaVistaModelo.LimpiarTrazos;
            set => _partidaVistaModelo.LimpiarTrazos = value;
        }

        public event Action<DTOs.TrazoDTO> TrazoRecibidoServidor
        {
            add => _partidaVistaModelo.TrazoRecibidoServidor += value;
            remove => _partidaVistaModelo.TrazoRecibidoServidor -= value;
        }

        public event Action<string, string> MensajeChatRecibido
        {
            add => _chatVistaModelo.MensajeChatRecibido += value;
            remove => _chatVistaModelo.MensajeChatRecibido -= value;
        }

        public event Action<string, string> MensajeDoradoRecibido
        {
            add => _chatVistaModelo.MensajeDoradoRecibido += value;
            remove => _chatVistaModelo.MensajeDoradoRecibido -= value;
        }

        public Func<string, bool> MostrarConfirmacion { get; set; }

        public Func<string, ResultadoReporteJugador> SolicitarDatosReporte { get; set; }

        public Action CerrarVentana { get; set; }

        public Func<InvitarAmigosVistaModelo, Task> MostrarInvitarAmigos { get; set; }

        public Func<bool> ChequearCierreAplicacionGlobal { get; set; }

        private DestinoNavegacion ObtenerDestinoSegunSesion()
        {
            bool sesionActiva = _usuarioSesion?.EstaAutenticado == true && !_esInvitado;
            return sesionActiva
                ? DestinoNavegacion.VentanaPrincipal
                : DestinoNavegacion.InicioSesion;
        }

        private static void NotificarComando(ICommand comando)
        {
            if (comando is IComandoNotificable notificable)
            {
                notificable.NotificarPuedeEjecutar();
            }
        }

        private void InicializarComandos()
        {
            InvitarCorreoComando = new ComandoAsincrono(
                async _ => await EjecutarInvitarCorreoAsync(),
                _ => PuedeInvitarPorCorreo);
            InvitarAmigosComando = new ComandoAsincrono(
                async () => await EjecutarInvitarAmigosAsync(),
                () => PuedeInvitarAmigos);
            AbrirAjustesComando = new ComandoDelegado(_ => EjecutarAbrirAjustes());
            IniciarPartidaComando = new ComandoAsincrono(async _ => await EjecutarIniciarPartidaAsync());
            CerrarVentanaComando = new ComandoDelegado(_ => EjecutarCerrarVentana());
            EnviarMensajeChatComando = new ComandoDelegado(_ => EjecutarEnviarMensajeChat());
        }

        private void InicializarProxyPartida()
        {
            try
            {
                var contexto = new InstanceContext(this);
                _proxyJuego = _fabricaClientes.CrearClienteCursoPartida(contexto);

                _proxyJuego.SuscribirJugador(
                    _codigoSala,
                    _idJugador,
                    _nombreUsuarioSesion,
                    _esHost);

                _logger.Info("Cliente WCF de partida inicializado y jugador suscrito.");
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicación al suscribir al jugador en la partida.", ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Se agotó el tiempo para inicializar el proxy de partida.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al inicializar el proxy de partida.", ex);
            }
        }

        private string ObtenerIdentificadorJugador()
        {
            if (_usuarioSesion.JugadorId > 0)
            {
                return _usuarioSesion.JugadorId.ToString();
            }

            if (!string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                return _nombreUsuarioSesion;
            }

            return Guid.NewGuid().ToString();
        }

        private void AplicarInicioVisualPartida()
        {
            _logger.Info("Iniciando partida...");
            _partidaVistaModelo.AplicarInicioVisualPartida(Jugadores?.Count ?? 0);
            ReiniciarPuntajesJugadores();
            BotonIniciarPartidaHabilitado = false;
            TextoBotonIniciarPartida = Lang.partidaTextoPartidaEnCurso;
        }

        private async Task EjecutarInvitarCorreoAsync()
        {
            var resultado = await _invitacionSalaServicio
                .InvitarPorCorreoAsync(_codigoSala, CorreoInvitacion)
                .ConfigureAwait(true);

            if (resultado.Exitoso)
            {
                _sonidoManejador.ReproducirNotificacion();
                _avisoServicio.Mostrar(resultado.Mensaje);
                CorreoInvitacion = string.Empty;
                return;
            }

            _sonidoManejador.ReproducirError();
            _avisoServicio.Mostrar(resultado.Mensaje);
        }

        private async Task EjecutarInvitarAmigosAsync()
        {
            var resultado = await _invitacionSalaServicio
                .ObtenerInvitacionAmigosAsync(
                    _codigoSala,
                    _nombreUsuarioSesion,
                    _amigosInvitados,
                    mensaje => _avisoServicio.Mostrar(mensaje))
                .ConfigureAwait(true);

            if (!resultado.Exitoso)
            {
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(resultado.Mensaje);
                return;
            }

            if (MostrarInvitarAmigos != null && resultado.VistaModelo != null)
            {
                await MostrarInvitarAmigos(resultado.VistaModelo).ConfigureAwait(true);
            }
        }

        private void EjecutarAbrirAjustes()
        {
            AbrirAjustesPartida();
        }

        private static string LimitarMensajePorCaracteres(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return mensaje;
            }

            if (mensaje.Length <= LimiteCaracteresChat)
            {
                return mensaje;
            }

            return mensaje.Substring(0, LimiteCaracteresChat);
        }

        private void EjecutarEnviarMensajeChat()
        {
            if (string.IsNullOrWhiteSpace(MensajeChat))
            {
                return;
            }

            _ = _chatVistaModelo.EnviarMensaje(MensajeChat);
            MensajeChat = string.Empty;
        }

        private void EjecutarEnviarMensaje(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return;
            }

            _ = EnviarMensajeAsync(mensaje);
        }

        private async Task EnviarMensajeAsync(string mensaje)
        {
            try
            {
                if (_proxyJuego == null)
                {
                    _logger.Warn("Proxy de juego no disponible para enviar mensaje.");
                    return;
                }

                await _proxyJuego.EnviarMensajeJuegoAsync(mensaje, _codigoSala, _idJugador)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException)
            {
                _logger.Error("No se pudo enviar el mensaje de juego.", ex);
                _sonidoManejador.ReproducirError();
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al enviar mensaje de juego.", ex);
                _sonidoManejador.ReproducirError();
            }
        }

        private async Task EjecutarRegistrarAciertoAsync(string nombreJugador, int puntosAdivinador, int puntosDibujante)
        {
            if (string.IsNullOrWhiteSpace(nombreJugador))
            {
                return;
            }

            _logger.InfoFormat(
                "Registrando acierto. Jugador: {0}, Puntos adivinador: {1}, Puntos dibujante: {2}",
                nombreJugador,
                puntosAdivinador,
                puntosDibujante);

            try
            {
                string mensajeAcierto = string.Format(
                    "ACIERTO:{0}:{1}:{2}",
                    nombreJugador,
                    puntosAdivinador,
                    puntosDibujante);

                if (_proxyJuego != null)
                {
                    await _proxyJuego.EnviarMensajeJuegoAsync(mensajeAcierto, _codigoSala, _idJugador)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException)
            {
                _logger.Error("No se pudo registrar el acierto en el servidor.", ex);
                _sonidoManejador.ReproducirError();
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al registrar acierto.", ex);
                _sonidoManejador.ReproducirError();
            }
        }

        /// <param name="trazo">Datos del trazo a enviar.</param>
        public void EnviarTrazoAlServidor(DTOs.TrazoDTO trazo)
        {
            if (trazo == null)
            {
                return;
            }

            try
            {
                _proxyJuego?.EnviarTrazo(trazo, _codigoSala, _idJugador);
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException)
            {
                _logger.Error("No se pudo enviar el trazo al servidor.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al enviar trazo al servidor.", ex);
            }
        }

        private async Task EjecutarIniciarPartidaAsync()
        {
            if (JuegoIniciado)
            {
                return;
            }

            if (Jugadores.Count < 2)
            {
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.errorTextoPartidaUnJugador);
                return;
            }

            try
            {
                BotonIniciarPartidaHabilitado = false;

                if (_proxyJuego != null)
                {
                    await _proxyJuego.IniciarPartidaAsync(_codigoSala, _idJugador)
                        .ConfigureAwait(true);
                }
                else
                {
                    AplicarInicioVisualPartida();
                }
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException)
            {
                _logger.Error("No se pudo solicitar el inicio de la partida.", ex);
                _sonidoManejador.ReproducirError();
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al iniciar la partida.", ex);
                _sonidoManejador.ReproducirError();
                BotonIniciarPartidaHabilitado = true;
            }
        }

        private void EjecutarCerrarVentana()
        {
            bool cerrandoPorVentana = ChequearCierreAplicacionGlobal?.Invoke() ?? true;

            if (cerrandoPorVentana)
            {
                NotificarCierreAplicacionCompleta();
            }
        }

        public void NotificarPartidaIniciada()
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.Invoke(() =>
            {
                AplicarInicioVisualPartida();
                _partidaVistaModelo.NotificarPartidaIniciada();
                BotonIniciarPartidaHabilitado = false;
                TextoBotonIniciarPartida = string.Empty;
                PuedeInvitarAmigos = false;
                PuedeInvitarPorCorreo = false;
            });
        }

        /// <param name="ronda">Datos de la ronda.</param>
        public void NotificarInicioRonda(DTOs.RondaDTO ronda)
        {
            _adivinadoresQuienYaAcertaron.Clear();
            _rondaTerminadaTemprano = false;

            _nombreDibujanteActual = ronda.NombreDibujante ?? string.Empty;

            _partidaVistaModelo.NotificarInicioRonda(ronda, Jugadores?.Count ?? 0);
        }

        /// <param name="nombreJugador">Nombre del jugador que adivino.</param>
        /// <param name="puntos">Puntos obtenidos.</param>
        public void NotificarJugadorAdivino(string nombreJugador, int puntos)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.Invoke(() =>
            {
                if (Jugadores != null && puntos > 0)
                {
                    var jugador = Jugadores.FirstOrDefault(j => string.Equals(
                        j.Nombre,
                        nombreJugador,
                        StringComparison.OrdinalIgnoreCase));

                    if (jugador != null)
                    {
                        jugador.Puntos += puntos;
                    }

                    if (!_adivinadoresQuienYaAcertaron.Contains(nombreJugador))
                    {
                        _adivinadoresQuienYaAcertaron.Add(nombreJugador);

                        int puntosBonusDibujante = (int)(puntos * PorcentajePuntosDibujante);

                        AgregarPuntosAlDibujante(puntosBonusDibujante);

                        if (TodosLosAdivinadoresAcertaron() && !_rondaTerminadaTemprano)
                        {
                            _rondaTerminadaTemprano = true;
                            _partidaVistaModelo.NotificarFinRondaTemprano();
                        }
                    }
                }

                _chatVistaModelo.NotificarJugadorAdivinoEnChat(nombreJugador);
                _partidaVistaModelo.NotificarJugadorAdivino(nombreJugador, puntos, _nombreUsuarioSesion);
            });
        }

        private void AgregarPuntosAlDibujante(int puntosBonusDibujante)
        {
            if (puntosBonusDibujante <= 0 || Jugadores == null)
            {
                return;
            }

            JugadorElemento dibujante = null;

            if (!string.IsNullOrWhiteSpace(_nombreDibujanteActual))
            {
                dibujante = Jugadores.FirstOrDefault(j => string.Equals(
                    j.Nombre,
                    _nombreDibujanteActual,
                    StringComparison.OrdinalIgnoreCase));
            }

            if (dibujante != null)
            {
                dibujante.Puntos += puntosBonusDibujante;
            }
        }

        private int ObtenerTotalAdivinadores()
        {
            return Jugadores?.Count > 0 ? Jugadores.Count - 1 : 0;
        }

        private bool TodosLosAdivinadoresAcertaron()
        {
            int totalAdivinadores = ObtenerTotalAdivinadores();
            return totalAdivinadores > 0 && _adivinadoresQuienYaAcertaron.Count >= totalAdivinadores;
        }

        /// <param name="nombreJugador">Nombre del jugador que envio el mensaje.</param>
        /// <param name="mensaje">Contenido del mensaje.</param>
        public void NotificarMensajeChat(string nombreJugador, string mensaje)
        {
            if (EsMensajeAcierto(mensaje) && IntentarProcesarAciertoDesdeMensaje(mensaje))
            {
                return;
            }
            _chatVistaModelo.NotificarMensajeChat(nombreJugador, mensaje);
        }

        /// <param name="trazo">Datos del trazo.</param>
        public void NotificarTrazoRecibido(DTOs.TrazoDTO trazo)
        {
            _partidaVistaModelo.NotificarTrazoRecibido(trazo);
        }

        public void NotificarFinRonda()
        {
            if (_rondaTerminadaTemprano)
            {
                return;
            }

            _partidaVistaModelo.NotificarFinRonda();
        }

        /// <param name="resultado">Resultado de la partida.</param>
        public void NotificarFinPartida(DTOs.ResultadoPartidaDTO resultado)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            string mensajeOriginal = resultado?.Mensaje;
            string mensaje = mensajeOriginal;
            bool esCancelacionPorFaltaDeJugadores = string.Equals(
                mensajeOriginal,
                "Partida cancelada por falta de jugadores.",
                StringComparison.OrdinalIgnoreCase);

            bool esCancelacionPorHost = string.Equals(
                mensajeOriginal,
                "El anfitrion de la sala abandono la partida.",
                StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = _localizador.Localizar(mensaje, mensaje);
            }

            dispatcher.Invoke(() =>
            {
                if (_salaCancelada)
                {
                    return;
                }

                if (esCancelacionPorFaltaDeJugadores && !_esHost)
                {
                    return;
                }

                _partidaVistaModelo.NotificarFinPartida();
                BotonIniciarPartidaHabilitado = false;

                if (esCancelacionPorHost && _esHost)
                {
                    return;
                }

                if (esCancelacionPorFaltaDeJugadores || esCancelacionPorHost)
                {
                    if (!string.IsNullOrWhiteSpace(mensaje))
                    {
                        _avisoServicio.Mostrar(mensaje);
                    }

                    var destino = ObtenerDestinoSegunSesion();

                    if (destino == DestinoNavegacion.InicioSesion)
                    {
                        _aplicacionCerrando = true;
                    }

                    Navegar(destino);
                    return;
                }

                MostrarResultadoFinalPartida(resultado);
            });
        }

        private void MostrarResultadoFinalPartida(DTOs.ResultadoPartidaDTO resultado)
        {
            bool esGanador = DeterminarSiEsGanador(resultado);
            string titulo = esGanador ? Lang.partidaTextoGanasteTitulo : Lang.partidaTextoPerdisteTitulo;
            string mensajeResultado = esGanador ? Lang.partidaTextoGanasteMensaje : Lang.partidaTextoPerdisteMensaje;

            string mensajeFinal = $"{titulo}\n{mensajeResultado}";
            _avisoServicio.Mostrar(mensajeFinal);

            var destino = ObtenerDestinoSegunSesion();

            if (destino == DestinoNavegacion.InicioSesion)
            {
                _aplicacionCerrando = true;
            }

            Navegar(destino);
        }

        private bool DeterminarSiEsGanador(DTOs.ResultadoPartidaDTO resultado)
        {
            if (resultado?.Clasificacion == null || resultado.Clasificacion.Count == 0)
            {
                return false;
            }

            var clasificacion = resultado.Clasificacion;
            var ganador = clasificacion.FirstOrDefault();

            if (ganador == null)
            {
                return false;
            }

            return string.Equals(
                ganador.Usuario,
                _nombreUsuarioSesion,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsMensajeAcierto(string mensaje)
        {
            return !string.IsNullOrWhiteSpace(mensaje) && mensaje.StartsWith("ACIERTO:", StringComparison.OrdinalIgnoreCase);
        }

        private bool IntentarProcesarAciertoDesdeMensaje(string mensaje)
        {
            string[] partes = mensaje?.Split(':');
            if (partes == null || partes.Length < 3)
            {
                return false;
            }

            string nombreJugador = partes[1];
            if (string.IsNullOrWhiteSpace(nombreJugador))
            {
                return false;
            }

            if (!int.TryParse(partes[2], out int puntosAdivinador))
            {
                return false;
            }

            NotificarJugadorAdivino(nombreJugador, puntosAdivinador);
            return true;
        }

        private void SalasServicio_JugadorSeUnio(object remitente, string nombreJugador)
        {
            EjecutarEnDispatcher(() =>
            {
                if (string.IsNullOrWhiteSpace(nombreJugador))
                {
                    return;
                }

                if (Jugadores.Any(j => string.Equals(
                    j.Nombre,
                    nombreJugador,
                    StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                if (Jugadores.Count >= MaximoJugadoresSala)
                {
                    return;
                }

                _logger.InfoFormat("Jugador unido a la sala: {0}",
					nombreJugador);
                AgregarJugador(nombreJugador);
            });
        }

        private void SalasServicio_JugadorSalio(object remitente, string nombreJugador)
        {
            EjecutarEnDispatcher(() =>
            {
                if (string.IsNullOrWhiteSpace(nombreJugador))
                {
                    return;
                }

                if (string.Equals(
                    nombreJugador,
                    _sala.Creador,
                    StringComparison.OrdinalIgnoreCase))
                {
                    CancelarSalaPorAnfitrion();
                    return;
                }

                JugadorElemento jugadorExistente = Jugadores.FirstOrDefault(j => string.Equals(
                    j.Nombre,
                    nombreJugador,
                    StringComparison.OrdinalIgnoreCase));

                if (jugadorExistente != null)
                {
                    _logger.InfoFormat("Jugador salió de la sala: {0}",
                                                nombreJugador);
                    Jugadores.Remove(jugadorExistente);
                    AjustarProgresoRondaTrasCambioJugadores();
                    ActualizarVisibilidadBotonesExpulsion();
                }
            });
        }

        private void SalasServicio_SalaCancelada(object remitente, string codigoSala)
        {
            EjecutarEnDispatcher(() =>
            {
                if (!string.Equals(
                    codigoSala,
                    _codigoSala,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                CancelarSalaPorAnfitrion();
            });
        }

        private void SalasServicio_JugadorExpulsado(object remitente, string nombreJugador)
        {
            EjecutarEnDispatcher(() =>
            {
                if (string.Equals(
                    nombreJugador,
                    _nombreUsuarioSesion,
                    StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Info("Este usuario ha sido expulsado de la sala.");
                    var destino = ObtenerDestinoSegunSesion();

                    if (destino == DestinoNavegacion.InicioSesion)
                    {
                        _aplicacionCerrando = true;
                    }

                    Navegar(destino);

                    _avisoServicio.Mostrar(Lang.expulsarJugadorTextoFuisteExpulsado);
                }
                else
                {
                    _logger.InfoFormat("Jugador expulsado de la sala: {0}",
						nombreJugador);
                    var jugador = Jugadores.FirstOrDefault(j => string.Equals(
                        j.Nombre,
                        nombreJugador,
                        StringComparison.OrdinalIgnoreCase));
                    if (jugador != null) Jugadores.Remove(jugador);
                }
            });
        }

        private void SalasServicio_SalaActualizada(object remitente, DTOs.SalaDTO sala)
        {
            if (sala == null || !string.Equals(
                sala.Codigo,
                _codigoSala,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            EjecutarEnDispatcher(() =>
            {
                ActualizarJugadores(sala.Jugadores);
                bool anfitrionSiguePresente = sala.Jugadores?.Any(jugador =>
                    string.Equals(
                        jugador,
                        _sala.Creador,
                        StringComparison.OrdinalIgnoreCase)) == true;

                if (!anfitrionSiguePresente)
                {
                    CancelarSalaPorAnfitrion();
                }
            });
        }

        private void ActualizarJugadores(IEnumerable<string> jugadores)
        {
            if (Jugadores == null)
            {
                Jugadores = new ObservableCollection<JugadorElemento>();
            }

            Jugadores.Clear();

            if (jugadores == null)
            {
                return;
            }

            var jugadoresUnicos = new HashSet<string>(ComparadorJugadores);

            foreach (string jugador in jugadores)
            {
                if (string.IsNullOrWhiteSpace(jugador))
                {
                    continue;
                }

                if (!jugadoresUnicos.Add(jugador))
                {
                    continue;
                }

                AgregarJugador(jugador);

                if (jugadoresUnicos.Count >= MaximoJugadoresSala)
                {
                    break;
                }
            }

            AjustarProgresoRondaTrasCambioJugadores();
            ActualizarVisibilidadBotonesExpulsion();
            ActualizarVisibilidadBotonesReporte();
        }

        private void AgregarJugador(string nombreJugador)
        {
            var jugadorElemento = new JugadorElemento
            {
                Nombre = nombreJugador,
                MostrarBotonExpulsar = PuedeExpulsarJugador(nombreJugador),
                ExpulsarComando = new ComandoAsincrono(async _ =>
                    await EjecutarExpulsarJugadorAsync(nombreJugador)),
                MostrarBotonReportar = PuedeReportarJugador(nombreJugador),
                ReportarComando = new ComandoAsincrono(async _ =>
                    await EjecutarReportarJugadorAsync(nombreJugador)),
                Puntos = 0
            };

            Jugadores.Add(jugadorElemento);
            AjustarProgresoRondaTrasCambioJugadores();
        }

        private void ReiniciarPuntajesJugadores()
        {
            if (Jugadores == null)
            {
                return;
            }

            foreach (var jugador in Jugadores)
            {
                jugador.Puntos = 0;
            }
        }

        private bool PuedeExpulsarJugador(string nombreJugador)
        {
            if (string.IsNullOrWhiteSpace(nombreJugador))
            {
                return false;
            }

            bool esElMismo = string.Equals(
                nombreJugador,
                _nombreUsuarioSesion,
                StringComparison.OrdinalIgnoreCase);
            bool esCreador = string.Equals(
                nombreJugador,
                _sala.Creador,
                StringComparison.OrdinalIgnoreCase);

            return _esHost && !JuegoIniciado && !esElMismo && !esCreador;
        }

        private bool PuedeReportarJugador(string nombreJugador)
        {
            if (string.IsNullOrWhiteSpace(nombreJugador))
            {
                return false;
            }

            if (_esInvitado)
            {
                return false;
            }

            return !string.Equals(
                nombreJugador,
                _nombreUsuarioSesion,
                StringComparison.OrdinalIgnoreCase);
        }

        private void ActualizarVisibilidadBotonesExpulsion()
        {
            if (Jugadores == null)
            {
                return;
            }

            foreach (var jugador in Jugadores)
            {
                jugador.MostrarBotonExpulsar = PuedeExpulsarJugador(jugador?.Nombre);
            }
        }

        private void ActualizarVisibilidadBotonesReporte()
        {
            if (Jugadores == null)
            {
                return;
            }

            foreach (var jugador in Jugadores)
            {
                jugador.MostrarBotonReportar = PuedeReportarJugador(jugador?.Nombre);
            }
        }

        private void AjustarProgresoRondaTrasCambioJugadores()
        {
            _partidaVistaModelo.AjustarProgresoRondaTrasCambioJugadores(Jugadores?.Count ?? 0);
        }

        private async Task EjecutarExpulsarJugadorAsync(string nombreJugador)
        {
            if (MostrarConfirmacion == null)
            {
                return;
            }

            string mensaje = string.Format(
                Lang.expulsarJugadorTextoConfirmacion,
                nombreJugador);
            bool confirmado = MostrarConfirmacion.Invoke(mensaje);

            if (!confirmado)
            {
                return;
            }

            try
            {
                _logger.InfoFormat("Solicitando expulsión de: {0}",
					nombreJugador);
                await _salasServicio.ExpulsarJugadorAsync(
                    _codigoSala,
                    _nombreUsuarioSesion,
                    nombreJugador).ConfigureAwait(true);

                _sonidoManejador.ReproducirNotificacion();
                _avisoServicio.Mostrar(Lang.expulsarJugadorTextoExito);
            }
            catch (Exception ex) when (ex is ServicioExcepcion || ex is ArgumentException)
            {
                _logger.ErrorFormat("Error al expulsar jugador {0}.",
					nombreJugador, ex);
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(ex.Message ?? Lang.errorTextoExpulsarJugador);
            }
        }

        private async Task EjecutarReportarJugadorAsync(string nombreJugador)
        {
            if (SolicitarDatosReporte == null)
            {
                return;
            }

            if (_esInvitado)
            {
                return;
            }

            var resultado = SolicitarDatosReporte.Invoke(nombreJugador);
            if (resultado == null || !resultado.Confirmado)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(resultado.Motivo))
            {
                _avisoServicio.Mostrar(Lang.reportarJugadorTextoMotivoRequerido);
                return;
            }

            var reporte = new DTOs.ReporteJugadorDTO
            {
                NombreUsuarioReportado = nombreJugador,
                NombreUsuarioReportante = _nombreUsuarioSesion,
                Motivo = resultado.Motivo
            };

            try
            {
                _logger.InfoFormat("Enviando reporte contra: {0}", nombreJugador);
                DTOs.ResultadoOperacionDTO respuesta = await _reportesServicio
                    .ReportarJugadorAsync(reporte)
                    .ConfigureAwait(true);

                if (respuesta?.OperacionExitosa == true)
                {
                    _sonidoManejador.ReproducirNotificacion();
                    _avisoServicio.Mostrar(Lang.reportarJugadorTextoExito);
                }
                else
                {
                    _sonidoManejador.ReproducirError();
                    _avisoServicio.Mostrar(
                        respuesta?.Mensaje ?? Lang.errorTextoReportarJugador);
                }
            }
            catch (Exception ex) when (ex is ServicioExcepcion || ex is ArgumentException)
            {
                _logger.ErrorFormat("Error al reportar jugador {0}.", nombreJugador, ex);
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(ex.Message ?? Lang.errorTextoReportarJugador);
            }
        }

        private void CancelarSalaPorAnfitrion()
        {
            if (_salaCancelada)
            {
                return;
            }

            _salaCancelada = true;
            _logger.Warn("La sala se canceló porque el anfitrión abandonó la partida.");

            _partidaVistaModelo.ReiniciarEstadoVisualSalaCancelada();
            BotonIniciarPartidaHabilitado = false;
            Jugadores.Clear();

            var destino = ObtenerDestinoSegunSesion();

            if (destino == DestinoNavegacion.InicioSesion)
            {
                _aplicacionCerrando = true;
            }

            Navegar(destino);
            _avisoServicio.Mostrar(Lang.partidaTextoHostCanceloSala);
        }

        private static void EjecutarEnDispatcher(Action accion)
        {
            if (accion == null) return;
            var dispatcher = Application.Current?.Dispatcher;

            if (dispatcher == null || dispatcher.CheckAccess())
            {
                accion();
            }
            else
            {
                dispatcher.BeginInvoke(accion);
            }
        }

        public async Task FinalizarAsync()
        {
            _partidaVistaModelo.PropertyChanged -= PartidaIniciadaVistaModelo_PropertyChanged;
            _partidaVistaModelo.CelebracionFinRondaTerminada -= OnCelebracionFinRondaTerminada;

            _partidaVistaModelo.Detener();

            _salasServicio.JugadorSeUnio -= SalasServicio_JugadorSeUnio;
            _salasServicio.JugadorSalio -= SalasServicio_JugadorSalio;
            _salasServicio.JugadorExpulsado -= SalasServicio_JugadorExpulsado;
            _salasServicio.SalaActualizada -= SalasServicio_SalaActualizada;
            _salasServicio.SalaCancelada -= SalasServicio_SalaCancelada;

            try
            {
                if (_proxyJuego is ICommunicationObject canal)
                {
                    if (canal.State == CommunicationState.Faulted)
                    {
                        canal.Abort();
                    }
                    else
                    {
                        canal.Close();
                    }
                }
            }
            catch (CommunicationException ex)
            {
                _logger.Warn("Error de comunicación al cerrar el canal de partida.", ex);
                (_proxyJuego as ICommunicationObject)?.Abort();
            }
            catch (TimeoutException ex)
            {
                _logger.Warn("Timeout al cerrar el canal de partida.", ex);
                (_proxyJuego as ICommunicationObject)?.Abort();
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn("Operación inválida al cerrar el canal de partida.", ex);
                (_proxyJuego as ICommunicationObject)?.Abort();
            }
            finally
            {
                _proxyJuego = null;
            }

            if (_sala != null && !string.IsNullOrWhiteSpace(_sala.Codigo)
                && !string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                try
                {
                    _logger.InfoFormat("Abandonando sala {0} al finalizar vista.",
						_sala.Codigo);
                    await _salasServicio.AbandonarSalaAsync(
                        _sala.Codigo,
                        _nombreUsuarioSesion).ConfigureAwait(false);
                }
                catch (ServicioExcepcion ex)
                {
                    _logger.WarnFormat("Error al abandonar sala en finalización: {0}",
						ex.Message);
                }
            }
        }

        public void NotificarCierreAplicacionCompleta()
        {
            _aplicacionCerrando = true;
        }

        /// <returns>True si se debe ejecutar la accion, false en caso contrario.</returns>
        public bool DebeEjecutarAccionAlCerrar()
        {
            return !_aplicacionCerrando;
        }

        private void Navegar(DestinoNavegacion destino)
        {
            CerrarVentana?.Invoke();

            if (destino == DestinoNavegacion.InicioSesion)
            {
                _usuarioSesion.Limpiar();
                var vmInicio = new InicioSesion.InicioSesionVistaModelo(
                    _ventana,
                    _localizador,
                    App.InicioSesionServicio,
                    App.CambioContrasenaServicio,
                    App.RecuperacionCuentaServicio,
                    App.ServicioIdioma,
                    _sonidoManejador,
                    _avisoServicio,
                    App.GeneradorNombres,
                    _usuarioSesion,
                    App.FabricaSalas);
                _ventana.MostrarVentana(vmInicio);
            }
            else
            {
                var vmPrincipal = new VentanaPrincipal.VentanaPrincipalVistaModelo(
                    _ventana,
                    _localizador,
                    App.ServicioIdioma,
                    App.ListaAmigosServicio,
                    App.AmigosServicio,
                    App.SalasServicio,
                    _sonidoManejador,
                    _usuarioSesion);
                _ventana.MostrarVentana(vmPrincipal);
            }
        }

        private void AbrirAjustesPartida()
        {
            var ajustesVM = new Ajustes.AjustesPartidaVistaModelo(
                _ventana,
                _localizador,
                _cancionManejador,
                _sonidoManejador);
            ajustesVM.SalirPartidaConfirmado = () =>
            {
                var destino = ObtenerDestinoSegunSesion();
                if (destino == DestinoNavegacion.InicioSesion)
                {
                    _aplicacionCerrando = true;
                }
                Navegar(destino);
            };

            _ventana.MostrarVentanaDialogo(ajustesVM);
        }
    }
}