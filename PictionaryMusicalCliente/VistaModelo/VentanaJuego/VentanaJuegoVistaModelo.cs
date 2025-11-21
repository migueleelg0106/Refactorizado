using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Modelo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.VistaModelo.Amigos;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.VentanaJuego
{
    /// <summary>
    /// Coordina la logica de la partida, incluyendo dibujo, chat, rondas y gestion de jugadores.
    /// Mantiene el estado sincronizado con el servidor y gestiona los eventos de la UI.
    /// </summary>
    public class VentanaJuegoVistaModelo : BaseVistaModelo
    {
        private static readonly ILog Log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int MaximoJugadoresSala = 4;
        private static readonly StringComparer ComparadorJugadores =
            StringComparer.OrdinalIgnoreCase;
        private readonly CancionManejador _manejadorCancion;
        private readonly DispatcherTimer _overlayTimer;
        private readonly DispatcherTimer _temporizador;
        private readonly ISalasServicio _salasServicio;
        private readonly IInvitacionesServicio _invitacionesServicio;
        private readonly IListaAmigosServicio _listaAmigosServicio;
        private readonly IPerfilServicio _perfilServicio;
        private readonly DTOs.SalaDTO _sala;
        private readonly string _nombreUsuarioSesion;
        private readonly bool _esInvitado;
        private readonly HashSet<int> _amigosInvitados;

        private bool _juegoIniciado;
        private double _grosor;
        private Color _color;
        private int _contador;
        private string _textoContador;
        private Brush _colorContador;
        private bool _esHerramientaLapiz;
        private bool _esHerramientaBorrador;
        private Visibility _visibilidadCuadriculaDibujo;
        private Visibility _visibilidadOverlayDibujante;
        private Visibility _visibilidadOverlayAdivinador;
        private Visibility _visibilidadPalabraAdivinar;
        private Visibility _visibilidadInfoCancion;
        private string _palabraAdivinar;
        private string _textoArtista;
        private string _textoGenero;
        private string _textoBotonIniciarPartida;
        private bool _botonIniciarPartidaHabilitado;
        private string _codigoSala;
        private ObservableCollection<JugadorElemento> _jugadores;
        private string _correoInvitacion;
        private bool _puedeInvitarPorCorreo;
        private bool _puedeInvitarAmigos;
        private bool _aplicacionCerrando;

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
        public VentanaJuegoVistaModelo(
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

            _manejadorCancion = new CancionManejador();
            _amigosInvitados = new HashSet<int>();

            _grosor = 6;
            _color = Colors.Black;
            _contador = 30;
            _textoContador = "30";
            _colorContador = Brushes.Black;
            _esHerramientaLapiz = true;
            _esHerramientaBorrador = false;
            _visibilidadCuadriculaDibujo = Visibility.Collapsed;
            _visibilidadOverlayDibujante = Visibility.Collapsed;
            _visibilidadOverlayAdivinador = Visibility.Collapsed;
            _visibilidadPalabraAdivinar = Visibility.Collapsed;
            _visibilidadInfoCancion = Visibility.Collapsed;
            _textoBotonIniciarPartida = Lang.partidaAdminTextoIniciarPartida;
            _botonIniciarPartidaHabilitado = true;

            _codigoSala = _sala.Codigo;
            _jugadores = new ObservableCollection<JugadorElemento>();
            ActualizarJugadores(_sala.Jugadores);
            _puedeInvitarPorCorreo = true;

            _salasServicio.JugadorSeUnio += SalasServicio_JugadorSeUnio;
            _salasServicio.JugadorSalio += SalasServicio_JugadorSalio;
            _salasServicio.JugadorExpulsado += SalasServicio_JugadorExpulsado;
            _salasServicio.SalaActualizada += SalasServicio_SalaActualizada;

            _overlayTimer = new DispatcherTimer();
            _overlayTimer.Interval = TimeSpan.FromSeconds(5);
            _overlayTimer.Tick += OverlayTimer_Tick;

            _temporizador = new DispatcherTimer();
            _temporizador.Interval = TimeSpan.FromSeconds(1);
            _temporizador.Tick += Temporizador_Tick;

            InicializarComandos();

            PuedeInvitarPorCorreo = !_esInvitado;
            PuedeInvitarAmigos = !_esInvitado;
        }

        /// <summary>
        /// Constructor de conveniencia que inicializa servicios por defecto.
        /// </summary>
        public VentanaJuegoVistaModelo(
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

        /// <summary>
        /// Indica si la partida ha comenzado.
        /// </summary>
        public bool JuegoIniciado
        {
            get => _juegoIniciado;
            private set => EstablecerPropiedad(ref _juegoIniciado, value);
        }

        /// <summary>
        /// Grosor del trazo del pincel.
        /// </summary>
        public double Grosor
        {
            get => _grosor;
            set => EstablecerPropiedad(ref _grosor, value);
        }

        /// <summary>
        /// Color actual del pincel.
        /// </summary>
        public Color Color
        {
            get => _color;
            set => EstablecerPropiedad(ref _color, value);
        }

        /// <summary>
        /// Texto a mostrar en el temporizador.
        /// </summary>
        public string TextoContador
        {
            get => _textoContador;
            set => EstablecerPropiedad(ref _textoContador, value);
        }

        /// <summary>
        /// Color del texto del temporizador (para alertas).
        /// </summary>
        public Brush ColorContador
        {
            get => _colorContador;
            set => EstablecerPropiedad(ref _colorContador, value);
        }

        /// <summary>
        /// Indica si la herramienta seleccionada es el lapiz.
        /// </summary>
        public bool EsHerramientaLapiz
        {
            get => _esHerramientaLapiz;
            set
            {
                if (EstablecerPropiedad(ref _esHerramientaLapiz, value))
                {
                    EsHerramientaBorrador = !value;
                    NotificarCambioHerramienta?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Indica si la herramienta seleccionada es el borrador.
        /// </summary>
        public bool EsHerramientaBorrador
        {
            get => _esHerramientaBorrador;
            set
            {
                if (EstablecerPropiedad(ref _esHerramientaBorrador, value))
                {
                    if (value)
                    {
                        EsHerramientaLapiz = false;
                    }
                    NotificarCambioHerramienta?.Invoke(!value);
                }
            }
        }

        /// <summary>
        /// Visibilidad del area de dibujo.
        /// </summary>
        public Visibility VisibilidadCuadriculaDibujo
        {
            get => _visibilidadCuadriculaDibujo;
            set => EstablecerPropiedad(ref _visibilidadCuadriculaDibujo, value);
        }

        /// <summary>
        /// Visibilidad del overlay para quien dibuja.
        /// </summary>
        public Visibility VisibilidadOverlayDibujante
        {
            get => _visibilidadOverlayDibujante;
            set => EstablecerPropiedad(ref _visibilidadOverlayDibujante, value);
        }

        /// <summary>
        /// Visibilidad del overlay para quien adivina.
        /// </summary>
        public Visibility VisibilidadOverlayAdivinador
        {
            get => _visibilidadOverlayAdivinador;
            set => EstablecerPropiedad(ref _visibilidadOverlayAdivinador, value);
        }

        /// <summary>
        /// Visibilidad de la palabra a adivinar en la interfaz.
        /// </summary>
        public Visibility VisibilidadPalabraAdivinar
        {
            get => _visibilidadPalabraAdivinar;
            set => EstablecerPropiedad(ref _visibilidadPalabraAdivinar, value);
        }

        /// <summary>
        /// Visibilidad de la informacion de la cancion.
        /// </summary>
        public Visibility VisibilidadInfoCancion
        {
            get => _visibilidadInfoCancion;
            set => EstablecerPropiedad(ref _visibilidadInfoCancion, value);
        }

        /// <summary>
        /// Palabra que se debe dibujar o adivinar.
        /// </summary>
        public string PalabraAdivinar
        {
            get => _palabraAdivinar;
            set => EstablecerPropiedad(ref _palabraAdivinar, value);
        }

        /// <summary>
        /// Nombre del artista de la cancion actual.
        /// </summary>
        public string TextoArtista
        {
            get => _textoArtista;
            set => EstablecerPropiedad(ref _textoArtista, value);
        }

        /// <summary>
        /// Genero musical de la cancion actual.
        /// </summary>
        public string TextoGenero
        {
            get => _textoGenero;
            set => EstablecerPropiedad(ref _textoGenero, value);
        }

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
        public ICommand SeleccionarLapizComando { get; private set; }

        /// <summary>
        /// Comando para seleccionar el borrador como herramienta.
        /// </summary>
        public ICommand SeleccionarBorradorComando { get; private set; }

        /// <summary>
        /// Comando para cambiar el grosor del trazo.
        /// </summary>
        public ICommand CambiarGrosorComando { get; private set; }

        /// <summary>
        /// Comando para cambiar el color del trazo.
        /// </summary>
        public ICommand CambiarColorComando { get; private set; }

        /// <summary>
        /// Comando para limpiar el lienzo de dibujo.
        /// </summary>
        public ICommand LimpiarDibujoComando { get; private set; }

        /// <summary>
        /// Comando para mostrar el overlay del dibujante.
        /// </summary>
        public ICommand MostrarOverlayDibujanteComando { get; private set; }

        /// <summary>
        /// Comando para mostrar el overlay del adivinador.
        /// </summary>
        public ICommand MostrarOverlayAdivinadorComando { get; private set; }

        /// <summary>
        /// Comando para cerrar los overlays informativos.
        /// </summary>
        public ICommand CerrarOverlayComando { get; private set; }

        /// <summary>
        /// Comando para cerrar la ventana de juego.
        /// </summary>
        public ICommand CerrarVentanaComando { get; private set; }

        /// <summary>
        /// Accion para abrir la ventana de ajustes.
        /// </summary>
        public Action<CancionManejador> AbrirAjustesPartida { get; set; }

        /// <summary>
        /// Accion notificar cambio de herramienta a la vista.
        /// </summary>
        public Action<bool> NotificarCambioHerramienta { get; set; }

        /// <summary>
        /// Accion para aplicar estilo visual de lapiz.
        /// </summary>
        public Action AplicarEstiloLapiz { get; set; }

        /// <summary>
        /// Accion para actualizar cursor de goma.
        /// </summary>
        public Action ActualizarFormaGoma { get; set; }

        /// <summary>
        /// Accion para limpiar el Canvas.
        /// </summary>
        public Action LimpiarTrazos { get; set; }

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
            IniciarPartidaComando = new ComandoDelegado(_ => EjecutarIniciarPartida());
            SeleccionarLapizComando = new ComandoDelegado(_ => EjecutarSeleccionarLapiz());
            SeleccionarBorradorComando = new ComandoDelegado(_ => EjecutarSeleccionarBorrador());
            CambiarGrosorComando = new ComandoDelegado(p => EjecutarCambiarGrosor(p));
            CambiarColorComando = new ComandoDelegado(p => EjecutarCambiarColor(p));
            LimpiarDibujoComando = new ComandoDelegado(_ => EjecutarLimpiarDibujo());
            MostrarOverlayDibujanteComando = new ComandoDelegado(
                _ => EjecutarMostrarOverlayDibujante());
            MostrarOverlayAdivinadorComando = new ComandoDelegado(
                _ => EjecutarMostrarOverlayAdivinador());
            CerrarOverlayComando = new ComandoDelegado(_ => EjecutarCerrarOverlay());
            CerrarVentanaComando = new ComandoDelegado(_ => EjecutarCerrarVentana());
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
                Log.InfoFormat("Enviando invitación por correo a: {0}",
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
                    Log.WarnFormat("Fallo al enviar invitación: {0}",
						resultado?.Mensaje);
                    SonidoManejador.ReproducirError();
					MostrarMensaje?.Invoke(resultado?.Mensaje ?? Lang.errorTextoEnviarCorreo);
				}
			}
			catch (ServicioExcepcion ex)
			{
				Log.Error("Excepción de servicio al enviar invitación.", ex);
				SonidoManejador.ReproducirError();
				MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoEnviarCorreo);
			}
			catch (ArgumentException ex)
			{
				Log.Error("Error de argumento al enviar invitación.", ex);
				SonidoManejador.ReproducirError();
				MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
			}
			catch (Exception ex)
			{
				Log.Error("Error inesperado al invitar.", ex);
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
                Log.Warn("Servicios de invitación no inicializados.");
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                Log.Warn("Intento de invitar amigos sin usuario de sesión.");
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
                Log.Error("Error al obtener lista de amigos para invitar.", ex);
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
            AbrirAjustesPartida?.Invoke(_manejadorCancion);
        }

        private void EjecutarIniciarPartida()
        {
            if (JuegoIniciado)
            {
                return;
            }

            Log.Info("Iniciando partida...");
            JuegoIniciado = true;
            VisibilidadCuadriculaDibujo = Visibility.Visible;
            EsHerramientaLapiz = true;
            AplicarEstiloLapiz?.Invoke();
            ActualizarFormaGoma?.Invoke();

            BotonIniciarPartidaHabilitado = false;
            TextoBotonIniciarPartida = Lang.partidaTextoPartidaEnCurso;
        }

        private void EjecutarSeleccionarLapiz()
        {
            EsHerramientaLapiz = true;
        }

        private void EjecutarSeleccionarBorrador()
        {
            EsHerramientaBorrador = true;
        }

        private void EjecutarCambiarGrosor(object parametro)
        {
            if (parametro != null &&
                double.TryParse(parametro.ToString(), out var nuevoGrosor))
            {
                Grosor = nuevoGrosor;
                if (EsHerramientaLapiz)
                {
                    AplicarEstiloLapiz?.Invoke();
                }
                else
                {
                    ActualizarFormaGoma?.Invoke();
                }
            }
        }

        private void EjecutarCambiarColor(object parametro)
        {
            if (parametro is string colorName)
            {
                Color = (Color)ColorConverter.ConvertFromString(colorName);
                EsHerramientaLapiz = true;
                AplicarEstiloLapiz?.Invoke();
            }
        }

        private void EjecutarLimpiarDibujo()
        {
            LimpiarTrazos?.Invoke();
        }

        private void EjecutarMostrarOverlayDibujante()
        {
            VisibilidadOverlayAdivinador = Visibility.Collapsed;
            VisibilidadOverlayDibujante = Visibility.Visible;

            _overlayTimer.Stop();
            _overlayTimer.Start();

            _manejadorCancion.Reproducir("Gasolina_Daddy_Yankee.mp3");
        }

        private void EjecutarMostrarOverlayAdivinador()
        {
            VisibilidadOverlayDibujante = Visibility.Collapsed;
            VisibilidadOverlayAdivinador = Visibility.Visible;

            _overlayTimer.Stop();
            _overlayTimer.Start();
        }

        private void EjecutarCerrarOverlay()
        {
            _overlayTimer.Stop();
            VisibilidadOverlayDibujante = Visibility.Collapsed;
            VisibilidadOverlayAdivinador = Visibility.Collapsed;
        }

        private void EjecutarCerrarVentana()
        {
            bool cerrandoPorVentana = ChequearCierreAplicacionGlobal?.Invoke() ?? true;

            if (cerrandoPorVentana)
            {
                NotificarCierreAplicacionCompleta();
            }
        }

        private void OverlayTimer_Tick(object sender, EventArgs e)
        {
            _overlayTimer.Stop();
            VisibilidadOverlayDibujante = Visibility.Collapsed;
            VisibilidadOverlayAdivinador = Visibility.Collapsed;
            IniciarTemporizador();
        }

        private void IniciarTemporizador()
        {
            _contador = 30;
            TextoContador = _contador.ToString();
            ColorContador = Brushes.Black;

            VisibilidadPalabraAdivinar = Visibility.Visible;
            VisibilidadInfoCancion = Visibility.Visible;

            PalabraAdivinar = "Gasolina";
            TextoArtista = "Artista: Daddy Yankee";
            TextoGenero = "Género: Reggaeton";

            _temporizador.Start();
        }

        private void Temporizador_Tick(object sender, EventArgs e)
        {
            _contador--;
            TextoContador = _contador.ToString();

            if (_contador <= 0)
            {
                _temporizador.Stop();
                TextoContador = "0";

                VisibilidadPalabraAdivinar = Visibility.Collapsed;
                VisibilidadInfoCancion = Visibility.Collapsed;

                MostrarMensaje?.Invoke("¡Tiempo terminado!");
            }
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

                Log.InfoFormat("Jugador unido a la sala: {0}",
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

                JugadorElemento jugadorExistente = Jugadores.FirstOrDefault(j => string.Equals(
                    j.Nombre,
                    nombreJugador,
                    StringComparison.OrdinalIgnoreCase));

                if (jugadorExistente != null)
                {
                    Log.InfoFormat("Jugador salió de la sala: {0}",
						nombreJugador);
                    Jugadores.Remove(jugadorExistente);
                }
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
                    Log.Info("Este usuario ha sido expulsado de la sala.");
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
                    Log.InfoFormat("Jugador expulsado de la sala: {0}",
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
        }

        private void AgregarJugador(string nombreJugador)
        {
            bool esHost = string.Equals(
                _sala.Creador,
                _nombreUsuarioSesion,
                StringComparison.OrdinalIgnoreCase);
            bool esElMismo = string.Equals(
                nombreJugador,
                _nombreUsuarioSesion,
                StringComparison.OrdinalIgnoreCase);
            bool esCreador = string.Equals(
                nombreJugador,
                _sala.Creador,
                StringComparison.OrdinalIgnoreCase);

            var jugadorElemento = new JugadorElemento
            {
                Nombre = nombreJugador,
                MostrarBotonExpulsar = esHost && !esElMismo && !esCreador,
                ExpulsarComando = new ComandoAsincrono(async _ =>
                    await EjecutarExpulsarJugadorAsync(nombreJugador))
            };

            Jugadores.Add(jugadorElemento);
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
                Log.InfoFormat("Solicitando expulsión de: {0}",
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
                Log.ErrorFormat("Error al expulsar jugador {0}.",
					nombreJugador, ex);
                SonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoExpulsarJugador);
            }
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
            _overlayTimer.Stop();
            _temporizador.Stop();

            _salasServicio.JugadorSeUnio -= SalasServicio_JugadorSeUnio;
            _salasServicio.JugadorSalio -= SalasServicio_JugadorSalio;
            _salasServicio.JugadorExpulsado -= SalasServicio_JugadorExpulsado;
            _salasServicio.SalaActualizada -= SalasServicio_SalaActualizada;

            if (_sala != null && !string.IsNullOrWhiteSpace(_sala.Codigo)
                && !string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                try
                {
                    Log.InfoFormat("Abandonando sala {0} al finalizar vista.",
						_sala.Codigo);
                    await _salasServicio.AbandonarSalaAsync(
                        _sala.Codigo,
                        _nombreUsuarioSesion).ConfigureAwait(false);
                }
                catch (ServicioExcepcion ex)
                {
                    Log.WarnFormat("Error al abandonar sala en finalización: {0}",
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