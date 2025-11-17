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
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.VentanaJuego
{
    public class VentanaJuegoVistaModelo : BaseVistaModelo
    {
        private const int MaximoJugadoresSala = 4;
        private static readonly StringComparer ComparadorJugadores = StringComparer.OrdinalIgnoreCase;
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

        public enum DestinoNavegacion
        {
            InicioSesion,
            VentanaPrincipal
        }

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
            _salasServicio = salasServicio ?? throw new ArgumentNullException(nameof(salasServicio));
            _invitacionesServicio = invitacionesServicio ?? throw new ArgumentNullException(nameof(invitacionesServicio));
            _listaAmigosServicio = listaAmigosServicio ?? throw new ArgumentNullException(nameof(listaAmigosServicio));
            _perfilServicio = perfilServicio ?? throw new ArgumentNullException(nameof(perfilServicio));

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

        public bool JuegoIniciado
        {
            get => _juegoIniciado;
            private set => EstablecerPropiedad(ref _juegoIniciado, value);
        }

        public double Grosor
        {
            get => _grosor;
            set => EstablecerPropiedad(ref _grosor, value);
        }

        public Color Color
        {
            get => _color;
            set => EstablecerPropiedad(ref _color, value);
        }

        public string TextoContador
        {
            get => _textoContador;
            set => EstablecerPropiedad(ref _textoContador, value);
        }

        public Brush ColorContador
        {
            get => _colorContador;
            set => EstablecerPropiedad(ref _colorContador, value);
        }

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

        public Visibility VisibilidadCuadriculaDibujo
        {
            get => _visibilidadCuadriculaDibujo;
            set => EstablecerPropiedad(ref _visibilidadCuadriculaDibujo, value);
        }

        public Visibility VisibilidadOverlayDibujante
        {
            get => _visibilidadOverlayDibujante;
            set => EstablecerPropiedad(ref _visibilidadOverlayDibujante, value);
        }

        public Visibility VisibilidadOverlayAdivinador
        {
            get => _visibilidadOverlayAdivinador;
            set => EstablecerPropiedad(ref _visibilidadOverlayAdivinador, value);
        }

        public Visibility VisibilidadPalabraAdivinar
        {
            get => _visibilidadPalabraAdivinar;
            set => EstablecerPropiedad(ref _visibilidadPalabraAdivinar, value);
        }

        public Visibility VisibilidadInfoCancion
        {
            get => _visibilidadInfoCancion;
            set => EstablecerPropiedad(ref _visibilidadInfoCancion, value);
        }

        public string PalabraAdivinar
        {
            get => _palabraAdivinar;
            set => EstablecerPropiedad(ref _palabraAdivinar, value);
        }

        public string TextoArtista
        {
            get => _textoArtista;
            set => EstablecerPropiedad(ref _textoArtista, value);
        }

        public string TextoGenero
        {
            get => _textoGenero;
            set => EstablecerPropiedad(ref _textoGenero, value);
        }

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

        public ICommand InvitarCorreoComando { get; private set; }
        public IComandoAsincrono InvitarAmigosComando { get; private set; }
        public ICommand AbrirAjustesComando { get; private set; }
        public ICommand IniciarPartidaComando { get; private set; }
        public ICommand SeleccionarLapizComando { get; private set; }
        public ICommand SeleccionarBorradorComando { get; private set; }
        public ICommand CambiarGrosorComando { get; private set; }
        public ICommand CambiarColorComando { get; private set; }
        public ICommand LimpiarDibujoComando { get; private set; }
        public ICommand MostrarOverlayDibujanteComando { get; private set; }
        public ICommand MostrarOverlayAdivinadorComando { get; private set; }
        public ICommand CerrarOverlayComando { get; private set; }
        public ICommand CerrarVentanaComando { get; private set; }

        public Action<CancionManejador> AbrirAjustesPartida { get; set; }
        public Action<bool> NotificarCambioHerramienta { get; set; }
        public Action AplicarEstiloLapiz { get; set; }
        public Action ActualizarFormaGoma { get; set; }
        public Action LimpiarTrazos { get; set; }
        public Action<string> MostrarMensaje { get; set; }
        public Func<string, bool> MostrarConfirmacion { get; set; }
        public Action CerrarVentana { get; set; }
        public Func<InvitarAmigosVistaModelo, Task> MostrarInvitarAmigos { get; set; }
        public Action<DestinoNavegacion> ManejarNavegacion { get; set; }
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
            InvitarCorreoComando = new ComandoAsincrono(async _ => await EjecutarInvitarCorreoAsync(), _ => PuedeInvitarPorCorreo);
            InvitarAmigosComando = new ComandoAsincrono(async () => await EjecutarInvitarAmigosAsync(), () => PuedeInvitarAmigos);
            AbrirAjustesComando = new ComandoDelegado(_ => EjecutarAbrirAjustes());
            IniciarPartidaComando = new ComandoDelegado(_ => EjecutarIniciarPartida());
            SeleccionarLapizComando = new ComandoDelegado(_ => EjecutarSeleccionarLapiz());
            SeleccionarBorradorComando = new ComandoDelegado(_ => EjecutarSeleccionarBorrador());
            CambiarGrosorComando = new ComandoDelegado(parametro => EjecutarCambiarGrosor(parametro));
            CambiarColorComando = new ComandoDelegado(parametro => EjecutarCambiarColor(parametro));
            LimpiarDibujoComando = new ComandoDelegado(_ => EjecutarLimpiarDibujo());
            MostrarOverlayDibujanteComando = new ComandoDelegado(_ => EjecutarMostrarOverlayDibujante());
            MostrarOverlayAdivinadorComando = new ComandoDelegado(_ => EjecutarMostrarOverlayAdivinador());
            CerrarOverlayComando = new ComandoDelegado(_ => EjecutarCerrarOverlay());
            CerrarVentanaComando = new ComandoDelegado(_ => EjecutarCerrarVentana());
        }

        private async Task EjecutarInvitarCorreoAsync()
        {
            string correo = CorreoInvitacion?.Trim();

            if (string.IsNullOrWhiteSpace(correo))
            {
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(Lang.errorTextoCorreoInvalido);
                return;
            }

            var resultadoValidacion = ValidacionEntrada.ValidarCorreo(correo);
            if (!resultadoValidacion.OperacionExitosa)
            {
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(resultadoValidacion.Mensaje ?? Lang.errorTextoCorreoInvalido);
                return;
            }

            try
            {
                var resultado = await _invitacionesServicio
                    .EnviarInvitacionAsync(_codigoSala, correo)
                    .ConfigureAwait(true);

                if (resultado != null && resultado.OperacionExitosa)
                {
                    ManejadorSonido.ReproducirExito();
                    MostrarMensaje?.Invoke(Lang.invitarCorreoTextoEnviado);
                    CorreoInvitacion = string.Empty;
                }
                else
                {
                    ManejadorSonido.ReproducirError();
                    string mensaje = MensajeServidorAyudante.Localizar(
                        resultado?.Mensaje,
                        Lang.errorTextoEnviarCorreo);
                    MostrarMensaje?.Invoke(mensaje);
                }
            }
            catch (ServicioExcepcion ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error Servicio Invitaciones]: {ex.Message}");
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoEnviarCorreo);
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error Invitaciones - Argumento inválido]: {ex.Message}");
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoEnviarCorreo);
            }
        }

        private async Task EjecutarInvitarAmigosAsync()
        {
            ManejadorSonido.ReproducirClick();

            if (_listaAmigosServicio == null || _invitacionesServicio == null || _perfilServicio == null)
            {
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
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
            catch (ServicioExcepcion ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error Servicio Lista Amigos]: {ex.Message}");
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
                return;
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error Lista Amigos - Argumento inválido]: {ex.Message}");
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            if (amigos == null || amigos.Count == 0)
            {
                ManejadorSonido.ReproducirError();
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
            if (parametro != null && double.TryParse(parametro.ToString(), out var nuevoGrosor))
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

                if (Jugadores.Any(j => string.Equals(j.Nombre, nombreJugador, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                if (Jugadores.Count >= MaximoJugadoresSala)
                {
                    return;
                }

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

                JugadorElemento jugadorExistente = Jugadores
                    .FirstOrDefault(j => string.Equals(j.Nombre, nombreJugador, StringComparison.OrdinalIgnoreCase));

                if (jugadorExistente != null)
                {
                    Jugadores.Remove(jugadorExistente);
                }
            });
        }

        private void SalasServicio_JugadorExpulsado(object sender, string nombreJugador)
        {
            EjecutarEnDispatcher(() =>
            {
                if (string.Equals(nombreJugador, _nombreUsuarioSesion, StringComparison.OrdinalIgnoreCase))
                {
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
                    var jugador = Jugadores.FirstOrDefault(j => string.Equals(j.Nombre, nombreJugador, StringComparison.OrdinalIgnoreCase));
                    if (jugador != null) Jugadores.Remove(jugador);
                }
            });
        }

        private void SalasServicio_SalaActualizada(object sender, DTOs.SalaDTO sala)
        {
            if (sala == null || !string.Equals(sala.Codigo, _codigoSala, StringComparison.OrdinalIgnoreCase))
                return;

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
            bool esHost = string.Equals(_sala.Creador, _nombreUsuarioSesion, StringComparison.OrdinalIgnoreCase);
            bool esElMismo = string.Equals(nombreJugador, _nombreUsuarioSesion, StringComparison.OrdinalIgnoreCase);
            bool esCreador = string.Equals(nombreJugador, _sala.Creador, StringComparison.OrdinalIgnoreCase);

            var jugadorElemento = new JugadorElemento
            {
                Nombre = nombreJugador,
                MostrarBotonExpulsar = esHost && !esElMismo && !esCreador,
                ExpulsarComando = new ComandoAsincrono(async _ => await EjecutarExpulsarJugadorAsync(nombreJugador))
            };

            Jugadores.Add(jugadorElemento);
        }

        private async Task EjecutarExpulsarJugadorAsync(string nombreJugador)
        {
            if (MostrarConfirmacion == null)
            {
                return;
            }

            string mensaje = string.Format(Lang.expulsarJugadorTextoConfirmacion, nombreJugador);
            bool confirmado = MostrarConfirmacion.Invoke(mensaje);

            if (!confirmado)
            {
                return;
            }

            try
            {
                await _salasServicio.ExpulsarJugadorAsync(_codigoSala, _nombreUsuarioSesion, nombreJugador).ConfigureAwait(true);
                ManejadorSonido.ReproducirExito();
                MostrarMensaje?.Invoke(Lang.expulsarJugadorTextoExito);
            }
            catch (ServicioExcepcion ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error Servicio Expulsar]: {ex.Message}");
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoExpulsarJugador);
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error Expulsar - Argumento inválido]: {ex.Message}");
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoExpulsarJugador);
            }
        }

        private static void EjecutarEnDispatcher(Action accion)
        {
            if (accion == null) return;
            var dispatcher = Application.Current?.Dispatcher;

            if (dispatcher == null || dispatcher.CheckAccess())
                accion();
            else
                dispatcher.BeginInvoke(accion);
        }

        public async Task FinalizarAsync()
        {
            _overlayTimer.Stop();
            _temporizador.Stop();

            _salasServicio.JugadorSeUnio -= SalasServicio_JugadorSeUnio;
            _salasServicio.JugadorSalio -= SalasServicio_JugadorSalio;
            _salasServicio.JugadorExpulsado -= SalasServicio_JugadorExpulsado;
            _salasServicio.SalaActualizada -= SalasServicio_SalaActualizada;

            if (_sala != null && !string.IsNullOrWhiteSpace(_sala.Codigo) && !string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                try
                {
                    await _salasServicio.AbandonarSalaAsync(_sala.Codigo, _nombreUsuarioSesion).ConfigureAwait(false);
                }
                catch (ServicioExcepcion ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[FinalizarAsync] Error al abandonar sala ({ex.Tipo}): {ex.Message}");
                }
            }

            (_salasServicio as IDisposable)?.Dispose();
            (_listaAmigosServicio as IDisposable)?.Dispose();
            (_invitacionesServicio as IDisposable)?.Dispose();
            (_perfilServicio as IDisposable)?.Dispose();
        }

        public void NotificarCierreAplicacionCompleta()
        {
            _aplicacionCerrando = true;
        }

        public bool DebeEjecutarAccionAlCerrar()
        {
            return !_aplicacionCerrando;
        }
    }
}