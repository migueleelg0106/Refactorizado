using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace PictionaryMusicalCliente.VistaModelo
{
    public class VentanaJuegoVistaModelo : BaseVistaModelo
    {
        private readonly CancionManejador _manejadorCancion;
        private readonly DispatcherTimer _overlayTimer;
        private readonly DispatcherTimer _temporizador;

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

        public VentanaJuegoVistaModelo()
        {
            _manejadorCancion = new CancionManejador();

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

            _overlayTimer = new DispatcherTimer();
            _overlayTimer.Interval = TimeSpan.FromSeconds(5);
            _overlayTimer.Tick += OverlayTimer_Tick;

            _temporizador = new DispatcherTimer();
            _temporizador.Interval = TimeSpan.FromSeconds(1);
            _temporizador.Tick += Temporizador_Tick;

            InicializarComandos();
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

        public ICommand InvitarCorreoComando { get; private set; }
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

        public Action AbrirExpulsionJugador { get; set; }
        public Action AbrirInvitacionAmigos { get; set; }
        public Action<CancionManejador> AbrirAjustesPartida { get; set; }
        public Action<bool> NotificarCambioHerramienta { get; set; }
        public Action AplicarEstiloLapiz { get; set; }
        public Action ActualizarFormaGoma { get; set; }
        public Action LimpiarTrazos { get; set; }
        public Action<string> MostrarMensaje { get; set; }

        private void InicializarComandos()
        {
            InvitarCorreoComando = new ComandoDelegado(_ => EjecutarInvitarCorreo());
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
        }

        private void EjecutarInvitarCorreo()
        {
            AbrirExpulsionJugador?.Invoke();
            AbrirInvitacionAmigos?.Invoke();
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

            if (_contador <= 10)
            {
                ColorContador = Brushes.Red;
            }

            if (_contador <= 0)
            {
                _temporizador.Stop();
                TextoContador = "0";

                VisibilidadPalabraAdivinar = Visibility.Collapsed;
                VisibilidadInfoCancion = Visibility.Collapsed;

                MostrarMensaje?.Invoke("¡Tiempo terminado!");
            }
        }
    }
}
