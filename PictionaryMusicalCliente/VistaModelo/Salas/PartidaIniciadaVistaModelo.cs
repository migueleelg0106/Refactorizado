using log4net;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Salas
{
    /// <summary>
    /// Coordina la logica de la partida iniciada, incluyendo dibujo, rondas y temporizadores.
    /// Gestiona los eventos de la UI relacionados con el juego en curso.
    /// </summary>
    public class PartidaIniciadaVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISonidoManejador _sonidoManejador;

        private readonly ICancionManejador _cancionManejador;
        private readonly DispatcherTimer _overlayTimer;
        private readonly DispatcherTimer _temporizadorAlarma;
        private readonly DispatcherTimer _temporizador;
        private readonly Dictionary<int, CancionCatalogo> _catalogoAudio;

        private bool _juegoIniciado;
        private int _numeroRondaActual;
        private double _grosor;
        private Color _color;
        private int _contador;
        private int _tiempoRondaSegundos;
        private readonly Stopwatch _cronometroRonda;
        private string _textoContador;
        private Brush _colorContador;
        private bool _esHerramientaLapiz;
        private bool _esHerramientaBorrador;
        private Visibility _visibilidadCuadriculaDibujo;
        private Visibility _visibilidadOverlayDibujante;
        private Visibility _visibilidadOverlayAdivinador;
        private Visibility _visibilidadOverlayAlarma;
        private Visibility _visibilidadPalabraAdivinar;
        private Visibility _visibilidadInfoCancion;
        private string _palabraAdivinar;
        private string _textoArtista;
        private string _textoGenero;
        private Visibility _visibilidadArtista;
        private Visibility _visibilidadGenero;
        private bool _mostrarEstadoRonda;
        private int _turnosCompletadosEnCiclo;
        private bool _esDibujante;
        private bool _alarmaActiva;
        private DTOs.RondaDTO _rondaPendiente;
        private int _totalJugadoresPendiente;
        private string _nombreCancionActual;
        private string _archivoCancionActual;
        private Brush _colorPalabraAdivinar;
        private string _textoDibujoDe;

        /// <summary>
        /// Inicializa la VistaModelo con los componentes necesarios para el juego.
        /// </summary>
        public PartidaIniciadaVistaModelo(ISonidoManejador sonidoManejador,
            ICancionManejador cancionManejador)
        {
            _cancionManejador = cancionManejador ??
                throw new ArgumentNullException(nameof(cancionManejador));
            _catalogoAudio = InicializarCatalogoAudio();
            _cronometroRonda = new Stopwatch();

            _numeroRondaActual = 0;
            _grosor = 6;
            _color = Colors.Black;
            _contador = 0;
            _textoContador = string.Empty;
            _colorContador = Brushes.Black;
            _esHerramientaLapiz = true;
            _esHerramientaBorrador = false;
            _visibilidadCuadriculaDibujo = Visibility.Collapsed;
            _visibilidadOverlayDibujante = Visibility.Collapsed;
            _visibilidadOverlayAdivinador = Visibility.Collapsed;
            _visibilidadOverlayAlarma = Visibility.Collapsed;
            _visibilidadPalabraAdivinar = Visibility.Collapsed;
            _visibilidadInfoCancion = Visibility.Collapsed;
            _colorPalabraAdivinar = Brushes.Black;
            _visibilidadArtista = Visibility.Collapsed;
            _visibilidadGenero = Visibility.Collapsed;
            _mostrarEstadoRonda = false;
            _turnosCompletadosEnCiclo = 0;
            _nombreCancionActual = string.Empty;
            _archivoCancionActual = string.Empty;
            _textoDibujoDe = string.Empty;

            _overlayTimer = new DispatcherTimer();
            _overlayTimer.Interval = TimeSpan.FromSeconds(5);
            _overlayTimer.Tick += OverlayTimer_Tick;

            _temporizadorAlarma = new DispatcherTimer();
            _temporizadorAlarma.Interval = TimeSpan.FromSeconds(5);
            _temporizadorAlarma.Tick += TemporizadorAlarma_Tick;

            _temporizador = new DispatcherTimer();
            _temporizador.Interval = TimeSpan.FromSeconds(1);
            _temporizador.Tick += Temporizador_Tick;

            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));

            InicializarComandos();
        }

        /// <summary>
        /// Indica si la partida ha comenzado.
        /// </summary>
        public bool JuegoIniciado
        {
            get => _juegoIniciado;
            private set
            {
                if (EstablecerPropiedad(ref _juegoIniciado, value))
                {
                    JuegoIniciadoCambiado?.Invoke(value);
                    if (!value)
                    {
                        _turnosCompletadosEnCiclo = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Numero de la ronda actual.
        /// </summary>
        public int NumeroRondaActual
        {
            get => _numeroRondaActual;
            private set => EstablecerPropiedad(ref _numeroRondaActual, value);
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
        /// Indica si se debe mostrar la informacion de la ronda y el temporizador.
        /// </summary>
        public bool MostrarEstadoRonda
        {
            get => _mostrarEstadoRonda;
            private set => EstablecerPropiedad(ref _mostrarEstadoRonda, value);
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
        /// Visibilidad del overlay de tiempo terminado.
        /// </summary>
        public Visibility VisibilidadOverlayAlarma
        {
            get => _visibilidadOverlayAlarma;
            set => EstablecerPropiedad(ref _visibilidadOverlayAlarma, value);
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
        /// Visibilidad del texto de artista.
        /// </summary>
        public Visibility VisibilidadArtista
        {
            get => _visibilidadArtista;
            set => EstablecerPropiedad(ref _visibilidadArtista, value);
        }

        /// <summary>
        /// Visibilidad del texto de genero musical.
        /// </summary>
        public Visibility VisibilidadGenero
        {
            get => _visibilidadGenero;
            set => EstablecerPropiedad(ref _visibilidadGenero, value);
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
        /// Color del texto de la palabra a adivinar.
        /// </summary>
        public Brush ColorPalabraAdivinar
        {
            get => _colorPalabraAdivinar;
            set => EstablecerPropiedad(ref _colorPalabraAdivinar, value);
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
        /// Texto que muestra quien es el dibujante actual.
        /// </summary>
        public string TextoDibujoDe
        {
            get => _textoDibujoDe;
            set => EstablecerPropiedad(ref _textoDibujoDe, value);
        }

        /// <summary>
        /// Indica si el usuario es el dibujante de la ronda.
        /// </summary>
        public bool EsDibujante
        {
            get => _esDibujante;
            private set => EstablecerPropiedad(ref _esDibujante, value);
        }

        /// <summary>
        /// Manejador de canciones para ajustes de volumen.
        /// </summary>
        public ICancionManejador CancionManejador => _cancionManejador;

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
        /// Comando para ocultar el overlay de tiempo terminado.
        /// </summary>
        public ICommand OcultarOverlayAlarmaComando { get; private set; }

        /// <summary>
        /// Accion para notificar cambio de herramienta a la vista.
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
        /// Evento para trazo recibido desde el servidor.
        /// </summary>
        public event Action<DTOs.TrazoDTO> TrazoRecibidoServidor;

        /// <summary>
        /// Evento que notifica cuando cambia el estado de juego iniciado.
        /// </summary>
        public event Action<bool> JuegoIniciadoCambiado;

        /// <summary>
        /// Accion para enviar trazo al servidor.
        /// </summary>
        public Action<DTOs.TrazoDTO> EnviarTrazoAlServidor { get; set; }

        /// <summary>
        /// Evento que notifica cuando cambia el estado de poder escribir del jugador.
        /// </summary>
        public event Action<bool> PuedeEscribirCambiado;

        /// <summary>
        /// Evento que notifica cuando cambia el rol de dibujante.
        /// </summary>
        public event Action<bool> EsDibujanteCambiado;

        /// <summary>
        /// Evento que notifica cuando cambia el nombre de la cancion correcta.
        /// </summary>
        public event Action<string> NombreCancionCambiado;

        /// <summary>
        /// Evento que notifica cuando cambia el tiempo restante.
        /// </summary>
        public event Action<int> TiempoRestanteCambiado;

        /// <summary>
        /// Evento que notifica cuando la celebracion de fin de ronda temprano ha terminado.
        /// </summary>
        public event Action CelebracionFinRondaTerminada;

        private void InicializarComandos()
        {
            SeleccionarLapizComando = new ComandoDelegado(_ => EjecutarSeleccionarLapiz());
            SeleccionarBorradorComando = new ComandoDelegado(_ => EjecutarSeleccionarBorrador());
            CambiarGrosorComando = new ComandoDelegado(p => EjecutarCambiarGrosor(p));
            CambiarColorComando = new ComandoDelegado(p => EjecutarCambiarColor(p));
            LimpiarDibujoComando = new ComandoDelegado(_ => EjecutarLimpiarDibujo());
            OcultarOverlayAlarmaComando = new ComandoDelegado(_ => OcultarOverlayAlarma());
        }

        private static Dictionary<int, CancionCatalogo> InicializarCatalogoAudio()
        {
            return new Dictionary<int, CancionCatalogo>
            {
                { 1, new CancionCatalogo("Gasolina", "Gasolina_Daddy_Yankee.mp3", "Español") },
                { 2, new CancionCatalogo("Bocanada", "Bocanada_Gustavo_Cerati.mp3", "Español") },
                { 3, new CancionCatalogo("La Nave Del Olvido", "La_Nave_Del_Olvido_Jose_Jose.mp3", "Español") },
                { 4, new CancionCatalogo("Tiburón", "Tiburon_Proyecto_Uno.mp3", "Español") },
                { 5, new CancionCatalogo("Pupilas De Gato", "Pupilas_De_Gato_Luis_Miguel.mp3", "Español") },
                { 6, new CancionCatalogo("El Triste", "El_Triste_Jose_Jose.mp3", "Español") },
                { 7, new CancionCatalogo("El Reloj", "El_Reloj_Luis_Miguel.mp3", "Español") },
                { 8, new CancionCatalogo("La Camisa Negra", "La_Camisa_Negra_Juanes.mp3", "Español") },
                { 9, new CancionCatalogo("Rosas", "Rosas_La_Oreja_de_Van_Gogh.mp3", "Español") },
                { 10, new CancionCatalogo("La Bicicleta", "La_Bicicleta_Shakira.mp3", "Español") },
                { 11, new CancionCatalogo("El Taxi", "El_Taxi_Pitbull.mp3", "Español") },
                { 12, new CancionCatalogo("La Puerta Negra", "La_Puerta_Negra_Los_Tigres_del_Norte.mp3", "Español") },
                { 13, new CancionCatalogo("Baraja de Oro", "Baraja_de_Oro_Chalino_Sanchez.mp3", "Español") },
                { 14, new CancionCatalogo("Los Luchadores", "Los_Luchadores_La_Sonora_Santanera.mp3", "Español") },
                { 15, new CancionCatalogo("El Oso Polar", "El_Oso_Polar_Nelson_Kanzela.mp3", "Español") },
                { 16, new CancionCatalogo("El Teléfono", "El_Telefono_Wisin_&_Yandel.mp3", "Español") },
                { 17, new CancionCatalogo("La Planta", "La_Planta_Caos.mp3", "Español") },
                { 18, new CancionCatalogo("Lluvia", "Lluvia_Eddie_Santiago.mp3", "Español") },
                { 19, new CancionCatalogo("Pose", "Pose_Daddy_Yankee.mp3", "Español") },
                { 20, new CancionCatalogo("Cama y Mesa", "Cama_y_Mesa_Roberto_Carlos.mp3", "Español") },

                { 21, new CancionCatalogo("Black Or White", "Black_Or_White_Michael_Jackson.mp3", "Ingles") },
                { 22, new CancionCatalogo("Don't Stop The Music", "Dont_Stop_The_Music_Rihanna.mp3", "Ingles") },
                { 23, new CancionCatalogo("Man In The Mirror", "Man_In_The_Mirror_Michael_Jackson.mp3", "Ingles") },
                { 24, new CancionCatalogo("Earth Song", "Earth_Song_Michael_Jackson.mp3", "Ingles") },
                { 25, new CancionCatalogo("Redbone", "Redbone_Childish_Gambino.mp3", "Ingles") },
                { 26, new CancionCatalogo("The Chain", "The_Chain_Fleetwood_Mac.mp3", "Ingles") },
                { 27, new CancionCatalogo("Umbrella", "Umbrella_Rihanna.mp3", "Ingles") },
                { 28, new CancionCatalogo("Yellow Submarine", "Yellow_Submarine_The_Beatles.mp3", "Ingles") },
                { 29, new CancionCatalogo("Money", "Money_Pink_Floyd.mp3", "Ingles") },
                { 30, new CancionCatalogo("Diamonds", "Diamonds_Rihanna.mp3", "Ingles") },
                { 31, new CancionCatalogo("Grenade", "Grenade_Bruno_Mars.mp3", "Ingles") },
                { 32, new CancionCatalogo("Scarface", "Scarface_Paul_Engemann.mp3", "Ingles") },
                { 33, new CancionCatalogo("Animals", "Animals_Martin_Garrix.mp3", "Ingles") },
                { 34, new CancionCatalogo("Hotel California", "Hotel_California_Eagles.mp3", "Ingles") },
                { 35, new CancionCatalogo("67", "67_Skrilla.mp3", "Ingles") },
                { 36, new CancionCatalogo("Blackbird", "Blackbird_The_Beatles.mp3", "Ingles") },
                { 37, new CancionCatalogo("Pony", "Pony_Ginuwine.mp3", "Ingles") },
                { 38, new CancionCatalogo("Rocket Man", "Rocket_Man_Elton_John.mp3", "Ingles") },
                { 39, new CancionCatalogo("Starman", "Starman_David_Bowie.mp3", "Ingles") },
                { 40, new CancionCatalogo("Time In A Bottle", "Time_In_A_Bottle_Jim_Croce.mp3", "Ingles") }
            };
        }

        private CancionCatalogo ObtenerCancion(int idCancion)
        {
            if (_catalogoAudio.TryGetValue(idCancion, out var cancion))
            {
                return cancion;
            }

            _logger.WarnFormat("No se encontró la canción con id {0} en el catálogo local.", idCancion);
            return null;
        }

        private static Visibility DeterminarVisibilidadPista(string textoPista)
        {
            return string.IsNullOrWhiteSpace(textoPista) ? Visibility.Collapsed : Visibility.Visible;
        }

        private class CancionCatalogo
        {
            public CancionCatalogo(string nombre, string archivo, string idioma)
            {
                Nombre = nombre;
                Archivo = archivo;
                Idioma = idioma;
            }

            public string Nombre { get; }
            public string Archivo { get; }
            public string Idioma { get; }
        }

        /// <summary>
        /// Aplica los cambios visuales al iniciar la partida.
        /// </summary>
        /// <param name="totalJugadores">Numero total de jugadores en la sala.</param>
        public void AplicarInicioVisualPartida(int totalJugadores)
        {
            _logger.Info("Iniciando partida...");
            JuegoIniciado = true;
            NumeroRondaActual = 0;
            _turnosCompletadosEnCiclo = 0;
            MostrarEstadoRonda = false;
            VisibilidadCuadriculaDibujo = Visibility.Visible;
            EsHerramientaLapiz = true;
            AplicarEstiloLapiz?.Invoke();
            ActualizarFormaGoma?.Invoke();
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

            var trazoLimpiar = new DTOs.TrazoDTO
            {
                PuntosX = Array.Empty<double>(),
                PuntosY = Array.Empty<double>(),
                ColorHex = string.Empty,
                Grosor = 0,
                EsBorrado = true,
                EsLimpiarTodo = true
            };

            EnviarTrazoAlServidor?.Invoke(trazoLimpiar);
        }

        private void EjecutarMostrarOverlayDibujante()
        {
            VisibilidadOverlayAdivinador = Visibility.Collapsed;
            VisibilidadOverlayDibujante = Visibility.Visible;
            VisibilidadPalabraAdivinar = Visibility.Visible;

            _overlayTimer.Stop();
            _overlayTimer.Start();
        }

        private void EjecutarMostrarOverlayAdivinador()
        {
            VisibilidadOverlayDibujante = Visibility.Collapsed;
            VisibilidadOverlayAdivinador = Visibility.Visible;
            VisibilidadPalabraAdivinar = Visibility.Collapsed;

            _overlayTimer.Stop();
            _overlayTimer.Start();
        }

        private void MostrarOverlayAlarma()
        {
            _alarmaActiva = true;

            if (!string.IsNullOrWhiteSpace(_nombreCancionActual))
            {
                PalabraAdivinar = _nombreCancionActual;
                _cancionManejador.Reproducir(_archivoCancionActual);
            }

            VisibilidadPalabraAdivinar = Visibility.Visible;
            ColorPalabraAdivinar = Brushes.Blue;
            VisibilidadOverlayAlarma = Visibility.Visible;

            _temporizadorAlarma.Stop();
            _temporizadorAlarma.Start();
        }

        private void OcultarOverlayAlarma()
        {
            FinalizarAlarma();
        }

        private void OverlayTimer_Tick(object sender, EventArgs e)
        {
            _overlayTimer.Stop();
            VisibilidadOverlayDibujante = Visibility.Collapsed;
            VisibilidadOverlayAdivinador = Visibility.Collapsed;
            HabilitarEscrituraTrasOverlay();
            IniciarTemporizador();
        }

        private void HabilitarEscrituraTrasOverlay()
        {
            if (_alarmaActiva)
            {
                return;
            }

            bool puedeEscribir = !EsDibujante;
            PuedeEscribirCambiado?.Invoke(puedeEscribir);
        }

        private void IniciarTemporizador()
        {
            if (_alarmaActiva)
            {
                return;
            }

            OcultarOverlayAlarma();

            _cronometroRonda.Restart();
            _contador = Math.Max(0, _contador);
            TextoContador = _contador.ToString();
            ColorContador = Brushes.Black;

            _temporizador.Start();
        }

        private void Temporizador_Tick(object sender, EventArgs e)
        {
            var tiempoTranscurrido = (int)_cronometroRonda.Elapsed.TotalSeconds;
            _contador = Math.Max(0, _tiempoRondaSegundos - tiempoTranscurrido);
            TextoContador = _contador.ToString();
            TiempoRestanteCambiado?.Invoke(_contador);

            if (_contador <= 0)
            {
                _temporizador.Stop();
                TextoContador = "0";
                _cancionManejador.Detener();

                VisibilidadPalabraAdivinar = Visibility.Collapsed;
                VisibilidadInfoCancion = Visibility.Collapsed;
                VisibilidadArtista = Visibility.Collapsed;
                VisibilidadGenero = Visibility.Collapsed;

                MostrarOverlayAlarma();
            }
        }

        private void TemporizadorAlarma_Tick(object sender, EventArgs e)
        {
            FinalizarAlarma();
        }

        private void FinalizarAlarma()
        {
            if (!_alarmaActiva && VisibilidadOverlayAlarma == Visibility.Collapsed)
            {
                return;
            }

            _temporizadorAlarma.Stop();
            _cancionManejador.Detener();
            VisibilidadOverlayAlarma = Visibility.Collapsed;
            _alarmaActiva = false;

            RestablecerPalabraTrasAlarma();

            if (_rondaPendiente != null)
            {
                var rondaPendiente = _rondaPendiente;
                _rondaPendiente = null;
                ProcesarInicioRonda(rondaPendiente);
            }
            else
            {
                CelebracionFinRondaTerminada?.Invoke();
            }
        }

        private void RestablecerPalabraTrasAlarma()
        {
            ColorPalabraAdivinar = Brushes.Black;
            PalabraAdivinar = string.Empty;
            VisibilidadPalabraAdivinar = Visibility.Collapsed;
            VisibilidadInfoCancion = Visibility.Collapsed;
        }

        /// <summary>
        /// Procesa la notificacion de que la partida ha iniciado.
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
                AplicarInicioVisualPartida(0);
                TextoContador = string.Empty;
                _sonidoManejador.ReproducirExito();
            });
        }

        /// <summary>
        /// Procesa la notificacion de inicio de una nueva ronda.
        /// </summary>
        /// <param name="ronda">Datos de la ronda.</param>
        /// <param name="totalJugadores">Numero total de jugadores.</param>
        public void NotificarInicioRonda(DTOs.RondaDTO ronda, int totalJugadores)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.Invoke(() =>
            {
                if (ronda == null)
                {
                    return;
                }

                _totalJugadoresPendiente = totalJugadores;

                if (_alarmaActiva)
                {
                    _rondaPendiente = ronda;
                    return;
                }

                ProcesarInicioRonda(ronda);
            });
        }

        private void ProcesarInicioRonda(DTOs.RondaDTO ronda)
        {
            _rondaPendiente = null;

            _temporizador.Stop();
            _overlayTimer.Stop();
            _cancionManejador.Detener();
            LimpiarTrazos?.Invoke();

            ActualizarContadorRondas(_totalJugadoresPendiente);
            _tiempoRondaSegundos = ronda.TiempoSegundos;
            _contador = ronda.TiempoSegundos;
            TextoContador = _contador.ToString();
            ColorContador = Brushes.Black;
            VisibilidadCuadriculaDibujo = Visibility.Visible;
            MostrarEstadoRonda = true;

            var cancion = ObtenerCancion(ronda.IdCancion);
            string archivoCancion = cancion?.Archivo ?? string.Empty;
            string nombreCancion = cancion?.Nombre ?? string.Empty;
            _nombreCancionActual = nombreCancion;
            _archivoCancionActual = archivoCancion;
            NombreCancionCambiado?.Invoke(nombreCancion);
            TiempoRestanteCambiado?.Invoke(_contador);
            ColorPalabraAdivinar = Brushes.Black;

            TextoDibujoDe = string.IsNullOrWhiteSpace(ronda.NombreDibujante)
                ? string.Empty
                : string.Format(Lang.partidaTextoDibujoDe, ronda.NombreDibujante);

            if (string.Equals(ronda.Rol, "Dibujante", StringComparison.OrdinalIgnoreCase))
            {
                EsDibujante = true;
                EsDibujanteCambiado?.Invoke(true);
                PuedeEscribirCambiado?.Invoke(false);
                PalabraAdivinar = string.IsNullOrWhiteSpace(nombreCancion)
                    ? PalabraAdivinar
                    : nombreCancion;
                VisibilidadPalabraAdivinar = Visibility.Visible;
                VisibilidadInfoCancion = Visibility.Visible;
                TextoArtista = string.Empty;
                TextoGenero = string.Empty;
                VisibilidadArtista = Visibility.Collapsed;
                VisibilidadGenero = Visibility.Collapsed;

                if (!string.IsNullOrWhiteSpace(archivoCancion))
                {
                    _cancionManejador.Reproducir(archivoCancion);
                }

                TextoArtista = string.IsNullOrWhiteSpace(ronda.PistaArtista)
                    ? string.Empty
                    : string.Format("Artista: {0}", ronda.PistaArtista);
                TextoGenero = string.IsNullOrWhiteSpace(ronda.PistaGenero)
                    ? string.Empty
                    : string.Format("Género: {0}", ronda.PistaGenero);
                VisibilidadArtista = DeterminarVisibilidadPista(TextoArtista);
                VisibilidadGenero = DeterminarVisibilidadPista(TextoGenero);
                VisibilidadInfoCancion = Visibility.Visible;

                EjecutarMostrarOverlayDibujante();
            }
            else
            {
                EsDibujante = false;
                EsDibujanteCambiado?.Invoke(false);
                PuedeEscribirCambiado?.Invoke(false);
                PalabraAdivinar = string.Empty;
                VisibilidadPalabraAdivinar = Visibility.Collapsed;
                TextoArtista = string.IsNullOrWhiteSpace(ronda.PistaArtista)
                    ? string.Empty
                    : string.Format("Artista: {0}", ronda.PistaArtista);
                TextoGenero = string.IsNullOrWhiteSpace(ronda.PistaGenero)
                    ? string.Empty
                    : string.Format("Género: {0}", ronda.PistaGenero);
                VisibilidadArtista = DeterminarVisibilidadPista(TextoArtista);
                VisibilidadGenero = DeterminarVisibilidadPista(TextoGenero);
                VisibilidadInfoCancion = Visibility.Visible;

                _cancionManejador.Detener();
                EjecutarMostrarOverlayAdivinador();
            }
        }

        /// <summary>
        /// Procesa la notificacion de que un jugador adivino la cancion.
        /// </summary>
        /// <param name="nombreJugador">Nombre del jugador que adivino.</param>
        /// <param name="puntos">Puntos obtenidos.</param>
        /// <param name="nombreUsuarioSesion">Nombre del usuario de la sesion actual.</param>
        public void NotificarJugadorAdivino(string nombreJugador, int puntos, string nombreUsuarioSesion)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.Invoke(() =>
            {
                _sonidoManejador.ReproducirExito();

                if (string.Equals(
                    nombreJugador,
                    nombreUsuarioSesion,
                    StringComparison.OrdinalIgnoreCase))
                {
                    PuedeEscribirCambiado?.Invoke(false);
                }
            });
        }

        /// <summary>
        /// Procesa la recepcion de un trazo desde el servidor.
        /// </summary>
        /// <param name="trazo">Datos del trazo.</param>
        public void NotificarTrazoRecibido(DTOs.TrazoDTO trazo)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.Invoke(() => TrazoRecibidoServidor?.Invoke(trazo));
        }

        /// <summary>
        /// Procesa la notificacion de fin de ronda.
        /// </summary>
        public void NotificarFinRonda()
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.Invoke(() =>
            {
                _temporizador.Stop();
                _overlayTimer.Stop();
                _cancionManejador.Detener();
                LimpiarTrazos?.Invoke();
                PuedeEscribirCambiado?.Invoke(false);
                MostrarEstadoRonda = false;
                TextoContador = string.Empty;
                MostrarOverlayAlarma();
            });
        }

        /// <summary>
        /// Procesa el fin de ronda temprano cuando todos los adivinadores acertaron.
        /// Muestra la cancion en azul y la reproduce durante 5 segundos.
        /// </summary>
        public void NotificarFinRondaTemprano()
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.Invoke(() =>
            {
                _temporizador.Stop();
                _overlayTimer.Stop();
                LimpiarTrazos?.Invoke();
                PuedeEscribirCambiado?.Invoke(false);
                MostrarEstadoRonda = false;
                TextoContador = string.Empty;

                if (!string.IsNullOrWhiteSpace(_nombreCancionActual))
                {
                    PalabraAdivinar = _nombreCancionActual;
                }

                VisibilidadPalabraAdivinar = Visibility.Visible;
                ColorPalabraAdivinar = Brushes.Blue;

                if (!string.IsNullOrWhiteSpace(_archivoCancionActual))
                {
                    _cancionManejador.Reproducir(_archivoCancionActual);
                }

                _alarmaActiva = true;
                _temporizadorAlarma.Stop();
                _temporizadorAlarma.Start();
            });
        }

        /// <summary>
        /// Procesa la notificacion de fin de partida.
        /// </summary>
        public void NotificarFinPartida()
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.Invoke(() =>
            {
                _temporizador.Stop();
                _overlayTimer.Stop();
                _temporizadorAlarma.Stop();
                _cancionManejador.Detener();
                JuegoIniciado = false;
                MostrarEstadoRonda = false;
                TextoContador = string.Empty;
                NumeroRondaActual = 0;
                PuedeEscribirCambiado?.Invoke(false);
                _alarmaActiva = false;
                _rondaPendiente = null;
                VisibilidadOverlayAlarma = Visibility.Collapsed;
                RestablecerPalabraTrasAlarma();
            });
        }

        /// <summary>
        /// Ajusta el progreso de ronda despues de un cambio en jugadores.
        /// </summary>
        /// <param name="totalJugadores">Numero total de jugadores.</param>
        public void AjustarProgresoRondaTrasCambioJugadores(int totalJugadores)
        {
            if (totalJugadores <= 0)
            {
                _turnosCompletadosEnCiclo = 0;
                NumeroRondaActual = 0;
                return;
            }

            _turnosCompletadosEnCiclo = Math.Min(_turnosCompletadosEnCiclo, totalJugadores);
        }

        private void ActualizarContadorRondas(int totalJugadores)
        {
            if (totalJugadores <= 0)
            {
                NumeroRondaActual = 0;
                _turnosCompletadosEnCiclo = 0;
                return;
            }

            _turnosCompletadosEnCiclo = (_turnosCompletadosEnCiclo % totalJugadores) + 1;

            if (NumeroRondaActual == 0)
            {
                NumeroRondaActual = 1;
            }
            else if (_turnosCompletadosEnCiclo == 1)
            {
                NumeroRondaActual++;
            }
        }

        /// <summary>
        /// Detiene todos los temporizadores y libera recursos.
        /// </summary>
        public void Detener()
        {
            _overlayTimer.Stop();
            _temporizador.Stop();
            _temporizadorAlarma.Stop();
            _cronometroRonda.Stop();
            _cancionManejador.Detener();
        }

        /// <summary>
        /// Reinicia el estado visual para mostrar la sala cancelada.
        /// </summary>
        public void ReiniciarEstadoVisualSalaCancelada()
        {
            _temporizador.Stop();
            _overlayTimer.Stop();
            _cancionManejador.Detener();

            JuegoIniciado = false;
            MostrarEstadoRonda = false;
            TextoContador = string.Empty;
            NumeroRondaActual = 0;
            VisibilidadCuadriculaDibujo = Visibility.Collapsed;
            VisibilidadOverlayAdivinador = Visibility.Collapsed;
            VisibilidadOverlayDibujante = Visibility.Collapsed;
            VisibilidadOverlayAlarma = Visibility.Collapsed;
        }
    }
}
