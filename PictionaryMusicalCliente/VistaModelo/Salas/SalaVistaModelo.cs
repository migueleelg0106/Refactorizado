using log4net;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.PictionaryServidorServicioCursoPartida;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Amigos;
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
    /// <summary>
    /// Coordina la logica de la sala anterior al inicio de la partida.
    /// Gestiona jugadores, invitaciones, codigo de sala y eventos de la UI.
    /// </summary>
    public class SalaVistaModelo : BaseVistaModelo, ICursoPartidaManejadorCallback
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int MaximoJugadoresSala = 4;
        private const string CursoPartidaEndpoint = "NetTcpBinding_ICursoPartidaManejador";
        private static readonly StringComparer ComparadorJugadores =
            StringComparer.OrdinalIgnoreCase;

        private readonly ISalasServicio _salasServicio;
        private readonly IInvitacionesServicio _invitacionesServicio;
        private readonly IListaAmigosServicio _listaAmigosServicio;
        private readonly IPerfilServicio _perfilServicio;
        private readonly DTOs.SalaDTO _sala;
        private readonly string _nombreUsuarioSesion;
        private readonly bool _esInvitado;
        private readonly HashSet<int> _amigosInvitados;
        private readonly bool _esHost;
        private readonly string _idJugador;
        private readonly PartidaIniciadaVistaModelo _partidaVistaModelo;
        private readonly ChatVistaModelo _chatVistaModelo;
        private CursoPartidaManejadorClient _proxyJuego;

        private string _textoBotonIniciarPartida;
        private bool _botonIniciarPartidaHabilitado;
        private bool _mostrarBotonIniciarPartida;
        private string _codigoSala;
        private ObservableCollection<JugadorElemento> _jugadores;
        private string _correoInvitacion;
        private bool _puedeInvitarPorCorreo;
        private bool _puedeInvitarAmigos;
        private bool _salaCancelada;
        private bool _aplicacionCerrando;
        private HashSet<string> _adivinadoresQuienYaAcertaron;
        private string _nombreDibujanteActual;
        private bool _rondaTerminadaTemprano;
        private string _mensajeChat;

        private const int LimitePalabrasChat = 150;
        private const double PorcentajePuntosDibujante = 0.2;

        /// <summary>
        /// Define los destinos posibles al salir de la partida.
        /// </summary>
        public enum DestinoNavegacion
        {
            /// <summary>Regresa al inicio de sesion (para invitados).</summary>
            InicioSesion,
            /// <summary>Regresa al menu principal (para usuarios registrados).</summary>
            VentanaPrincipal
        }

        /// <summary>
        /// Inicializa la VistaModelo con todos los servicios necesarios para el juego.
        /// </summary>
        /// <param name="sala">Datos de la sala actual.</param>
        /// <param name="salasServicio">Servicio de comunicacion de salas.</param>
        /// <param name="invitacionesServicio">Servicio para invitar usuarios.</param>
        /// <param name="listaAmigosServicio">Servicio para obtener amigos.</param>
        /// <param name="perfilServicio">Servicio de perfil de usuario.</param>
        /// <param name="nombreJugador">Nombre del jugador actual (opcional).</param>
        /// <param name="esInvitado">Indica si el usuario es invitado.</param>
        public SalaVistaModelo(
            DTOs.SalaDTO sala,
            ISalasServicio salasServicio,
            IInvitacionesServicio invitacionesServicio,
            IListaAmigosServicio listaAmigosServicio,
            IPerfilServicio perfilServicio,
            string nombreJugador = null,
            bool esInvitado = false)
        {
            _sala = sala ?? throw new ArgumentNullException(nameof(sala));
            _salasServicio = salasServicio ??
                throw new ArgumentNullException(nameof(salasServicio));
            _invitacionesServicio = invitacionesServicio ??
                throw new ArgumentNullException(nameof(invitacionesServicio));
            _listaAmigosServicio = listaAmigosServicio ??
                throw new ArgumentNullException(nameof(listaAmigosServicio));
            _perfilServicio = perfilServicio ??
                throw new ArgumentNullException(nameof(perfilServicio));

            _esInvitado = esInvitado;
            _nombreUsuarioSesion = !string.IsNullOrWhiteSpace(nombreJugador)
                ? nombreJugador
                : SesionUsuarioActual.Usuario?.NombreUsuario ?? string.Empty;
            _esHost = string.Equals(
                _sala.Creador,
                _nombreUsuarioSesion,
                StringComparison.OrdinalIgnoreCase);
            _idJugador = ObtenerIdentificadorJugador();

            _amigosInvitados = new HashSet<int>();
            _adivinadoresQuienYaAcertaron = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _nombreDibujanteActual = string.Empty;
            _rondaTerminadaTemprano = false;

            _partidaVistaModelo = new PartidaIniciadaVistaModelo();
            _chatVistaModelo = new ChatVistaModelo();
            _chatVistaModelo.PropertyChanged += ChatVistaModelo_PropertyChanged;

            _partidaVistaModelo.PropertyChanged += PartidaIniciadaVistaModelo_PropertyChanged;

            ConfigurarPartidaVistaModelo();
            ConfigurarChatVistaModelo();

            _textoBotonIniciarPartida = Lang.partidaAdminTextoIniciarPartida;
            _botonIniciarPartidaHabilitado = _esHost;
            _mostrarBotonIniciarPartida = _esHost;

            _codigoSala = _sala.Codigo;
            _jugadores = new ObservableCollection<JugadorElemento>();
            ActualizarJugadores(_sala.Jugadores);
            _puedeInvitarPorCorreo = true;

            _salasServicio.JugadorSeUnio += SalasServicio_JugadorSeUnio;
            _salasServicio.JugadorSalio += SalasServicio_JugadorSalio;
            _salasServicio.JugadorExpulsado += SalasServicio_JugadorExpulsado;
            _salasServicio.SalaActualizada += SalasServicio_SalaActualizada;
            _salasServicio.SalaCancelada += SalasServicio_SalaCancelada;

            InicializarComandos();

            PuedeInvitarPorCorreo = !_esInvitado;
            PuedeInvitarAmigos = !_esInvitado;
            _chatVistaModelo.PuedeEscribir = true;

            InicializarProxyPartida();
        }

        /// <summary>
        /// Constructor de conveniencia que inicializa servicios por defecto.
        /// </summary>
        public SalaVistaModelo(
            DTOs.SalaDTO sala,
            ISalasServicio salasServicio,
            string nombreJugador = null,
            bool esInvitado = false)
            : this(
                sala,
                salasServicio,
                new InvitacionesServicio(),
                new ListaAmigosServicio(),
                new PerfilServicio(),
                nombreJugador,
                esInvitado)
        {
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

        private void ConfigurarChatVistaModelo()
        {
            _chatVistaModelo.EnviarMensajeAlServidor = EjecutarEnviarMensaje;
            _chatVistaModelo.ObtenerNombreJugadorActual = () => _nombreUsuarioSesion;
            _chatVistaModelo.RegistrarAciertoEnServidor = EjecutarRegistrarAciertoAsync;
        }

        private void ChatVistaModelo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(
                e.PropertyName,
                nameof(ChatVistaModelo.PuedeEscribir),
                StringComparison.Ordinal))
            {
                NotificarCambio(nameof(PuedeEscribir));
            }
        }

        private void PartidaIniciadaVistaModelo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotificarCambio(e.PropertyName);
        }

        private void OnJuegoIniciadoCambiado(bool juegoIniciado)
        {
            MostrarBotonIniciarPartida = _esHost && !juegoIniciado;
            ActualizarVisibilidadBotonesExpulsion();
            _chatVistaModelo.EsPartidaIniciada = juegoIniciado;
        }

        /// <summary>
        /// VistaModelo que coordina la logica de la partida iniciada.
        /// </summary>
        public PartidaIniciadaVistaModelo PartidaVistaModelo => _partidaVistaModelo;

        /// <summary>
        /// VistaModelo que coordina la logica del chat.
        /// </summary>
        public ChatVistaModelo ChatVistaModelo => _chatVistaModelo;

        /// <summary>
        /// Indica si la partida ha comenzado.
        /// </summary>
        public bool JuegoIniciado => _partidaVistaModelo.JuegoIniciado;

        /// <summary>
        /// Indica si el usuario actual es el anfitrion de la sala.
        /// </summary>
        public bool EsHost => _esHost;

        /// <summary>
        /// Numero de la ronda actual.
        /// </summary>
        public int NumeroRondaActual => _partidaVistaModelo.NumeroRondaActual;

        /// <summary>
        /// Grosor del trazo del pincel.
        /// </summary>
        public double Grosor
        {
            get => _partidaVistaModelo.Grosor;
            set => _partidaVistaModelo.Grosor = value;
        }

        /// <summary>
        /// Color actual del pincel.
        /// </summary>
        public Color Color
        {
            get => _partidaVistaModelo.Color;
            set => _partidaVistaModelo.Color = value;
        }

        /// <summary>
        /// Texto a mostrar en el temporizador.
        /// </summary>
        public string TextoContador => _partidaVistaModelo.TextoContador;

        /// <summary>
        /// Color del texto del temporizador (para alertas).
        /// </summary>
        public Brush ColorContador => _partidaVistaModelo.ColorContador;

        /// <summary>
        /// Indica si se debe mostrar la informacion de la ronda y el temporizador.
        /// </summary>
        public bool MostrarEstadoRonda => _partidaVistaModelo.MostrarEstadoRonda;

        /// <summary>
        /// Indica si la herramienta seleccionada es el lapiz.
        /// </summary>
        public bool EsHerramientaLapiz
        {
            get => _partidaVistaModelo.EsHerramientaLapiz;
            set => _partidaVistaModelo.EsHerramientaLapiz = value;
        }

        /// <summary>
        /// Indica si la herramienta seleccionada es el borrador.
        /// </summary>
        public bool EsHerramientaBorrador
        {
            get => _partidaVistaModelo.EsHerramientaBorrador;
            set => _partidaVistaModelo.EsHerramientaBorrador = value;
        }

        /// <summary>
        /// Visibilidad del area de dibujo.
        /// </summary>
        public Visibility VisibilidadCuadriculaDibujo
        {
            get => _partidaVistaModelo.VisibilidadCuadriculaDibujo;
            set => _partidaVistaModelo.VisibilidadCuadriculaDibujo = value;
        }

        /// <summary>
        /// Visibilidad del overlay para quien dibuja.
        /// </summary>
        public Visibility VisibilidadOverlayDibujante
        {
            get => _partidaVistaModelo.VisibilidadOverlayDibujante;
            set => _partidaVistaModelo.VisibilidadOverlayDibujante = value;
        }

        /// <summary>
        /// Visibilidad del overlay para quien adivina.
        /// </summary>
        public Visibility VisibilidadOverlayAdivinador
        {
            get => _partidaVistaModelo.VisibilidadOverlayAdivinador;
            set => _partidaVistaModelo.VisibilidadOverlayAdivinador = value;
        }

        /// <summary>
        /// Visibilidad del overlay de tiempo terminado.
        /// </summary>
        public Visibility VisibilidadOverlayAlarma
        {
            get => _partidaVistaModelo.VisibilidadOverlayAlarma;
            set => _partidaVistaModelo.VisibilidadOverlayAlarma = value;
        }

        /// <summary>
        /// Visibilidad de la palabra a adivinar en la interfaz.
        /// </summary>
        public Visibility VisibilidadPalabraAdivinar
        {
            get => _partidaVistaModelo.VisibilidadPalabraAdivinar;
            set => _partidaVistaModelo.VisibilidadPalabraAdivinar = value;
        }

        /// <summary>
        /// Visibilidad de la informacion de la cancion.
        /// </summary>
        public Visibility VisibilidadInfoCancion
        {
            get => _partidaVistaModelo.VisibilidadInfoCancion;
            set => _partidaVistaModelo.VisibilidadInfoCancion = value;
        }

        /// <summary>
        /// Visibilidad del texto de artista.
        /// </summary>
        public Visibility VisibilidadArtista
        {
            get => _partidaVistaModelo.VisibilidadArtista;
            set => _partidaVistaModelo.VisibilidadArtista = value;
        }

        /// <summary>
        /// Visibilidad del texto de genero musical.
        /// </summary>
        public Visibility VisibilidadGenero
        {
            get => _partidaVistaModelo.VisibilidadGenero;
            set => _partidaVistaModelo.VisibilidadGenero = value;
        }

        /// <summary>
        /// Palabra que se debe dibujar o adivinar.
        /// </summary>
        public string PalabraAdivinar
        {
            get => _partidaVistaModelo.PalabraAdivinar;
            set => _partidaVistaModelo.PalabraAdivinar = value;
        }

        /// <summary>
        /// Color del texto de la palabra a adivinar.
        /// </summary>
        public Brush ColorPalabraAdivinar
        {
            get => _partidaVistaModelo.ColorPalabraAdivinar;
            set => _partidaVistaModelo.ColorPalabraAdivinar = value;
        }

        /// <summary>
        /// Nombre del artista de la cancion actual.
        /// </summary>
        public string TextoArtista
        {
            get => _partidaVistaModelo.TextoArtista;
            set => _partidaVistaModelo.TextoArtista = value;
        }

        /// <summary>
        /// Genero musical de la cancion actual.
        /// </summary>
        public string TextoGenero
        {
            get => _partidaVistaModelo.TextoGenero;
            set => _partidaVistaModelo.TextoGenero = value;
        }

        /// <summary>
        /// Texto que muestra quien es el dibujante actual.
        /// </summary>
        public string TextoDibujoDe => _partidaVistaModelo.TextoDibujoDe;

        /// <summary>
        /// Texto del boton de inicio de partida.
        /// </summary>
        public string TextoBotonIniciarPartida
        {
            get => _textoBotonIniciarPartida;
            set => EstablecerPropiedad(ref _textoBotonIniciarPartida, value);
        }

        /// <summary>
        /// Estado de habilitacion del boton de inicio.
        /// </summary>
        public bool BotonIniciarPartidaHabilitado
        {
            get => _botonIniciarPartidaHabilitado;
            set => EstablecerPropiedad(ref _botonIniciarPartidaHabilitado, value);
        }

        /// <summary>
        /// Controla la visibilidad del boton de inicio de partida.
        /// </summary>
        public bool MostrarBotonIniciarPartida
        {
            get => _mostrarBotonIniciarPartida;
            private set => EstablecerPropiedad(ref _mostrarBotonIniciarPartida, value);
        }

        /// <summary>
        /// Codigo unico de la sala actual.
        /// </summary>
        public string CodigoSala
        {
            get => _codigoSala;
            set => EstablecerPropiedad(ref _codigoSala, value);
        }

        /// <summary>
        /// Coleccion de jugadores presentes en la sala.
        /// </summary>
        public ObservableCollection<JugadorElemento> Jugadores
        {
            get => _jugadores;
            set => EstablecerPropiedad(ref _jugadores, value);
        }

        /// <summary>
        /// Correo electronico ingresado para invitar.
        /// </summary>
        public string CorreoInvitacion
        {
            get => _correoInvitacion;
            set => EstablecerPropiedad(ref _correoInvitacion, value);
        }

        /// <summary>
        /// Indica si el usuario tiene permisos para invitar por correo.
        /// </summary>
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

        /// <summary>
        /// Indica si el usuario tiene permisos para invitar amigos.
        /// </summary>
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

        /// <summary>
        /// Indica si el usuario actual es un invitado.
        /// </summary>
        public bool EsInvitado => _esInvitado;

        /// <summary>
        /// Indica si el jugador actual puede escribir en el chat.
        /// </summary>
        public bool PuedeEscribir
        {
            get => _chatVistaModelo.PuedeEscribir;
            private set => _chatVistaModelo.PuedeEscribir = value;
        }

        /// <summary>
        /// Indica si el usuario es el dibujante de la ronda.
        /// </summary>
        public bool EsDibujante => _partidaVistaModelo.EsDibujante;

        /// <summary>
        /// Texto del mensaje de chat a enviar.
        /// </summary>
        public string MensajeChat
        {
            get => _mensajeChat;
            set => EstablecerPropiedad(ref _mensajeChat, LimitarMensajePorPalabras(value));
        }

        /// <summary>
        /// Comando para invitar a un usuario por correo.
        /// </summary>
        public ICommand InvitarCorreoComando { get; private set; }

        /// <summary>
        /// Comando para abrir la lista de amigos a invitar.
        /// </summary>
        public IComandoAsincrono InvitarAmigosComando { get; private set; }

        /// <summary>
        /// Comando para abrir los ajustes de la partida.
        /// </summary>
        public ICommand AbrirAjustesComando { get; private set; }

        /// <summary>
        /// Comando para iniciar el juego.
        /// </summary>
        public ICommand IniciarPartidaComando { get; private set; }

        /// <summary>
        /// Comando para seleccionar el lapiz como herramienta.
        /// </summary>
        public ICommand SeleccionarLapizComando => _partidaVistaModelo.SeleccionarLapizComando;

        /// <summary>
        /// Comando para seleccionar el borrador como herramienta.
        /// </summary>
        public ICommand SeleccionarBorradorComando => _partidaVistaModelo.SeleccionarBorradorComando;

        /// <summary>
        /// Comando para cambiar el grosor del trazo.
        /// </summary>
        public ICommand CambiarGrosorComando => _partidaVistaModelo.CambiarGrosorComando;

        /// <summary>
        /// Comando para cambiar el color del trazo.
        /// </summary>
        public ICommand CambiarColorComando => _partidaVistaModelo.CambiarColorComando;

        /// <summary>
        /// Comando para limpiar el lienzo de dibujo.
        /// </summary>
        public ICommand LimpiarDibujoComando => _partidaVistaModelo.LimpiarDibujoComando;

        /// <summary>
        /// Comando para ocultar el overlay de tiempo terminado.
        /// </summary>
        public ICommand OcultarOverlayAlarmaComando => _partidaVistaModelo.OcultarOverlayAlarmaComando;

        /// <summary>
        /// Comando para cerrar la ventana de juego.
        /// </summary>
        public ICommand CerrarVentanaComando { get; private set; }

        /// <summary>
        /// Comando para enviar un mensaje de chat.
        /// </summary>
        public ICommand EnviarMensajeChatComando { get; private set; }

        /// <summary>
        /// Accion para abrir la ventana de ajustes.
        /// </summary>
        public Action<CancionManejador> AbrirAjustesPartida { get; set; }

        /// <summary>
        /// Accion notificar cambio de herramienta a la vista.
        /// </summary>
        public Action<bool> NotificarCambioHerramienta
        {
            get => _partidaVistaModelo.NotificarCambioHerramienta;
            set => _partidaVistaModelo.NotificarCambioHerramienta = value;
        }

        /// <summary>
        /// Accion para aplicar estilo visual de lapiz.
        /// </summary>
        public Action AplicarEstiloLapiz
        {
            get => _partidaVistaModelo.AplicarEstiloLapiz;
            set => _partidaVistaModelo.AplicarEstiloLapiz = value;
        }

        /// <summary>
        /// Accion para actualizar cursor de goma.
        /// </summary>
        public Action ActualizarFormaGoma
        {
            get => _partidaVistaModelo.ActualizarFormaGoma;
            set => _partidaVistaModelo.ActualizarFormaGoma = value;
        }

        /// <summary>
        /// Accion para limpiar el Canvas.
        /// </summary>
        public Action LimpiarTrazos
        {
            get => _partidaVistaModelo.LimpiarTrazos;
            set => _partidaVistaModelo.LimpiarTrazos = value;
        }

        /// <summary>
        /// Evento para trazo recibido desde el servidor.
        /// </summary>
        public event Action<DTOs.TrazoDTO> TrazoRecibidoServidor
        {
            add => _partidaVistaModelo.TrazoRecibidoServidor += value;
            remove => _partidaVistaModelo.TrazoRecibidoServidor -= value;
        }

        /// <summary>
        /// Evento para notificar mensajes de chat entrantes.
        /// </summary>
        public event Action<string, string> MensajeChatRecibido
        {
            add => _chatVistaModelo.MensajeChatRecibido += value;
            remove => _chatVistaModelo.MensajeChatRecibido -= value;
        }

        /// <summary>
        /// Evento para notificar mensajes dorados (aciertos) al chat.
        /// </summary>
        public event Action<string, string> MensajeDoradoRecibido
        {
            add => _chatVistaModelo.MensajeDoradoRecibido += value;
            remove => _chatVistaModelo.MensajeDoradoRecibido -= value;
        }

        /// <summary>
        /// Accion para mostrar mensajes al usuario.
        /// </summary>
        public Action<string> MostrarMensaje { get; set; }

        /// <summary>
        /// Funcion para solicitar confirmacion al usuario.
        /// </summary>
        public Func<string, bool> MostrarConfirmacion { get; set; }

        /// <summary>
        /// Accion para cerrar la ventana fisica.
        /// </summary>
        public Action CerrarVentana { get; set; }

        /// <summary>
        /// Funcion para mostrar el dialogo de invitar amigos.
        /// </summary>
        public Func<InvitarAmigosVistaModelo, Task> MostrarInvitarAmigos { get; set; }

        /// <summary>
        /// Accion para navegar a otra vista.
        /// </summary>
        public Action<DestinoNavegacion> ManejarNavegacion { get; set; }

        /// <summary>
        /// Funcion para verificar cierre global.
        /// </summary>
        public Func<bool> ChequearCierreAplicacionGlobal { get; set; }

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
                _proxyJuego = new CursoPartidaManejadorClient(contexto, CursoPartidaEndpoint);

                _proxyJuego.SuscribirJugador(
                    _codigoSala,
                    _idJugador,
                    _nombreUsuarioSesion,
                    _esHost);

                _logger.Info("Cliente WCF de partida inicializado y jugador suscrito.");
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException)
            {
                _logger.Error("Error de comunicación al suscribir al jugador en la partida.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al inicializar el proxy de partida.", ex);
            }
        }

        private string ObtenerIdentificadorJugador()
        {
            if (SesionUsuarioActual.Usuario?.JugadorId > 0)
            {
                return SesionUsuarioActual.Usuario.JugadorId.ToString();
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
            string correo = CorreoInvitacion?.Trim();

            if (string.IsNullOrWhiteSpace(correo))
            {
                SonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(Lang.errorTextoCorreoInvalido);
                return;
            }

            var resultadoValidacion = ValidacionEntrada.ValidarCorreo(correo);
            if (!resultadoValidacion.OperacionExitosa)
            {
                SonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(
                    resultadoValidacion.Mensaje ?? Lang.errorTextoCorreoInvalido);
                return;
            }

            try
            {
				_logger.InfoFormat("Enviando invitación por correo a: {0}",
					correo);
                var resultado = await _invitacionesServicio
                    .EnviarInvitacionAsync(_codigoSala, correo)
                    .ConfigureAwait(true);

                if (resultado != null && resultado.OperacionExitosa)
                {
                    SonidoManejador.ReproducirExito();
                    MostrarMensaje?.Invoke(Lang.invitarCorreoTextoEnviado);
                    CorreoInvitacion = string.Empty;
                }
                else
                {
                    _logger.WarnFormat("Fallo al enviar invitación: {0}",
						resultado?.Mensaje);
                    SonidoManejador.ReproducirError();
					MostrarMensaje?.Invoke(resultado?.Mensaje ?? Lang.errorTextoEnviarCorreo);
				}
			}
			catch (ServicioExcepcion ex)
			{
				_logger.Error("Excepción de servicio al enviar invitación.", ex);
				SonidoManejador.ReproducirError();
				MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoEnviarCorreo);
			}
			catch (ArgumentException ex)
			{
				_logger.Error("Error de argumento al enviar invitación.", ex);
				SonidoManejador.ReproducirError();
				MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
			}
                        catch (Exception ex)
                        {
                                _logger.Error("Error inesperado al invitar.", ex);
                                SonidoManejador.ReproducirError();
                                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                        }
                }

        private async Task EjecutarInvitarAmigosAsync()
        {
            SonidoManejador.ReproducirClick();

            if (_listaAmigosServicio == null ||
                _invitacionesServicio == null ||
                _perfilServicio == null)
            {
                _logger.Warn("Servicios de invitación no inicializados.");
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                _logger.Warn("Intento de invitar amigos sin usuario de sesión.");
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            IReadOnlyList<DTOs.AmigoDTO> amigos;

            try
            {
                amigos = await _listaAmigosServicio
                    .ObtenerAmigosAsync(_nombreUsuarioSesion)
                    .ConfigureAwait(true);
            }
            catch (Exception ex) when (ex is ServicioExcepcion || ex is ArgumentException)
            {
                _logger.Error("Error al obtener lista de amigos para invitar.", ex);
                SonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            if (amigos == null || amigos.Count == 0)
            {
                SonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(Lang.invitarAmigosTextoSinAmigos);
                return;
            }

            var vistaModelo = new InvitarAmigosVistaModelo(
                amigos,
                _invitacionesServicio,
                _perfilServicio,
                _codigoSala,
                id => _amigosInvitados.Contains(id),
                id =>
                {
                    if (!_amigosInvitados.Contains(id))
                    {
                        _amigosInvitados.Add(id);
                    }
                },
                mensaje => MostrarMensaje?.Invoke(mensaje));

            if (MostrarInvitarAmigos != null)
            {
                await MostrarInvitarAmigos(vistaModelo).ConfigureAwait(true);
            }
        }

        private void EjecutarAbrirAjustes()
        {
            AbrirAjustesPartida?.Invoke(_partidaVistaModelo.ManejadorCancion);
        }

        private static string LimitarMensajePorPalabras(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return mensaje;
            }

            var palabras = mensaje.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            if (palabras.Length <= LimitePalabrasChat)
            {
                return mensaje;
            }

            return string.Join(" ", palabras.Take(LimitePalabrasChat));
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
                SonidoManejador.ReproducirError();
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al enviar mensaje de juego.", ex);
                SonidoManejador.ReproducirError();
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
                SonidoManejador.ReproducirError();
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al registrar acierto.", ex);
                SonidoManejador.ReproducirError();
            }
        }

        /// <summary>
        /// Envia un trazo al servidor.
        /// </summary>
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
                SonidoManejador.ReproducirError();
                AvisoAyudante.Mostrar(Lang.errorTextoPartidaUnJugador);
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
                SonidoManejador.ReproducirError();
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al iniciar la partida.", ex);
                SonidoManejador.ReproducirError();
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

        /// <summary>
        /// Procesa la notificacion de que la partida ha iniciado desde el servidor.
        /// </summary>
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

        /// <summary>
        /// Procesa la notificacion de inicio de una nueva ronda desde el servidor.
        /// </summary>
        /// <param name="ronda">Datos de la ronda.</param>
        public void NotificarInicioRonda(DTOs.RondaDTO ronda)
        {
            _adivinadoresQuienYaAcertaron.Clear();
            _rondaTerminadaTemprano = false;

            _nombreDibujanteActual = ronda.NombreDibujante ?? string.Empty;

            _partidaVistaModelo.NotificarInicioRonda(ronda, Jugadores?.Count ?? 0);
        }

        /// <summary>
        /// Procesa la notificacion de que un jugador adivino la cancion.
        /// </summary>
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

        /// <summary>
        /// Procesa la notificacion de un mensaje de chat desde el servidor.
        /// </summary>
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

        /// <summary>
        /// Procesa la recepcion de un trazo desde el servidor.
        /// </summary>
        /// <param name="trazo">Datos del trazo.</param>
        public void NotificarTrazoRecibido(DTOs.TrazoDTO trazo)
        {
            _partidaVistaModelo.NotificarTrazoRecibido(trazo);
        }

        /// <summary>
        /// Procesa la notificacion de fin de ronda desde el servidor.
        /// </summary>
        public void NotificarFinRonda()
        {
            if (_rondaTerminadaTemprano)
            {
                return;
            }

            _partidaVistaModelo.NotificarFinRonda();
        }

        /// <summary>
        /// Procesa la notificacion de fin de partida desde el servidor.
        /// </summary>
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
            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = MensajeServidorAyudante.Localizar(mensaje, mensaje);
            }

            dispatcher.Invoke(() =>
            {
                _partidaVistaModelo.NotificarFinPartida();
                BotonIniciarPartidaHabilitado = false;

                bool esCancelacionPorFaltaDeJugadores = string.Equals(
                    mensajeOriginal,
                    "No hay suficientes jugadores para seguir jugando, se canceló la partida.",
                    StringComparison.Ordinal);

                if (esCancelacionPorFaltaDeJugadores)
                {
                    if (!string.IsNullOrWhiteSpace(mensaje))
                    {
                        MostrarMensaje?.Invoke(mensaje);
                    }

                    ManejarNavegacion?.Invoke(DestinoNavegacion.VentanaPrincipal);
                }
                else
                {
                    MostrarResultadoFinalPartida(resultado);
                }
            });
        }

        private void MostrarResultadoFinalPartida(DTOs.ResultadoPartidaDTO resultado)
        {
            bool esGanador = DeterminarSiEsGanador(resultado);
            string titulo = esGanador ? Lang.partidaTextoGanasteTitulo : Lang.partidaTextoPerdisteTitulo;
            string mensajeResultado = esGanador ? Lang.partidaTextoGanasteMensaje : Lang.partidaTextoPerdisteMensaje;

            string mensajeFinal = $"{titulo}\n{mensajeResultado}";
            MostrarMensaje?.Invoke(mensajeFinal);

            DestinoNavegacion destino = _esInvitado
                ? DestinoNavegacion.InicioSesion
                : DestinoNavegacion.VentanaPrincipal;

            if (destino == DestinoNavegacion.InicioSesion)
            {
                _aplicacionCerrando = true;
            }

            ManejarNavegacion?.Invoke(destino);
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

        private void SalasServicio_JugadorSeUnio(object sender, string nombreJugador)
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

        private void SalasServicio_JugadorSalio(object sender, string nombreJugador)
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

        private void SalasServicio_SalaCancelada(object sender, string codigoSala)
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

        private void SalasServicio_JugadorExpulsado(object sender, string nombreJugador)
        {
            EjecutarEnDispatcher(() =>
            {
                if (string.Equals(
                    nombreJugador,
                    _nombreUsuarioSesion,
                    StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Info("Este usuario ha sido expulsado de la sala.");
                    DestinoNavegacion destino = _esInvitado
                        ? DestinoNavegacion.InicioSesion
                        : DestinoNavegacion.VentanaPrincipal;

                    if (destino == DestinoNavegacion.InicioSesion)
                    {
                        _aplicacionCerrando = true;
                    }

                    ManejarNavegacion?.Invoke(destino);

                    MostrarMensaje?.Invoke(Lang.expulsarJugadorTextoFuisteExpulsado);
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

        private void SalasServicio_SalaActualizada(object sender, DTOs.SalaDTO sala)
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
        }

        private void AgregarJugador(string nombreJugador)
        {
            var jugadorElemento = new JugadorElemento
            {
                Nombre = nombreJugador,
                MostrarBotonExpulsar = PuedeExpulsarJugador(nombreJugador),
                ExpulsarComando = new ComandoAsincrono(async _ =>
                    await EjecutarExpulsarJugadorAsync(nombreJugador)),
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

                SonidoManejador.ReproducirExito();
                MostrarMensaje?.Invoke(Lang.expulsarJugadorTextoExito);
            }
            catch (Exception ex) when (ex is ServicioExcepcion || ex is ArgumentException)
            {
                _logger.ErrorFormat("Error al expulsar jugador {0}.",
					nombreJugador, ex);
                SonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoExpulsarJugador);
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

            DestinoNavegacion destino = _esInvitado
                ? DestinoNavegacion.InicioSesion
                : DestinoNavegacion.VentanaPrincipal;

            if (destino == DestinoNavegacion.InicioSesion)
            {
                _aplicacionCerrando = true;
            }

            ManejarNavegacion?.Invoke(destino);
            MostrarMensaje?.Invoke(Lang.partidaTextoHostCanceloSala);
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

        /// <summary>
        /// Libera los recursos y cierra la conexion con la sala al terminar la partida.
        /// </summary>
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

            (_salasServicio as IDisposable)?.Dispose();
            (_listaAmigosServicio as IDisposable)?.Dispose();
            (_invitacionesServicio as IDisposable)?.Dispose();
            (_perfilServicio as IDisposable)?.Dispose();
        }

        /// <summary>
        /// Marca la aplicacion como cerrandose para evitar dialogos de confirmacion adicionales.
        /// </summary>
        public void NotificarCierreAplicacionCompleta()
        {
            _aplicacionCerrando = true;
        }

        /// <summary>
        /// Determina si se deben ejecutar acciones de limpieza al cerrar la ventana.
        /// </summary>
        /// <returns>True si se debe ejecutar la accion, false en caso contrario.</returns>
        public bool DebeEjecutarAccionAlCerrar()
        {
            return !_aplicacionCerrando;
        }
    }
}