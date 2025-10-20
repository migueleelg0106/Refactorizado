using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Idiomas;
using PictionaryMusicalCliente.Servicios.Wcf;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class VentanaPrincipalVistaModelo : BaseVistaModelo
    {
        private string _nombreUsuario;
        private string _codigoSala;
        private ObservableCollection<OpcionEntero> _numeroRondasOpciones;
        private OpcionEntero _numeroRondasSeleccionada;
        private ObservableCollection<OpcionEntero> _tiempoRondaOpciones;
        private OpcionEntero _tiempoRondaSeleccionada;
        private ObservableCollection<IdiomaOpcion> _idiomasDisponibles;
        private IdiomaOpcion _idiomaSeleccionado;
        private ObservableCollection<OpcionTexto> _dificultadesDisponibles;
        private OpcionTexto _dificultadSeleccionada;
        private ObservableCollection<string> _amigos;
        private string _amigoSeleccionado;

        private readonly ILocalizacionService _localizacionService;
        private readonly IAmigosService _amigosService;
        private readonly IComandoNotificable _abrirEliminarAmigoCommand;

        public VentanaPrincipalVistaModelo()
            : this(LocalizacionService.Instancia, AmigosService.Instancia)
        {
        }

        public VentanaPrincipalVistaModelo(ILocalizacionService localizacionService, IAmigosService amigosService)
        {
            _localizacionService = localizacionService ?? throw new ArgumentNullException(nameof(localizacionService));
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));

            CargarDatosUsuario();
            CargarOpcionesPartida();
            CargarIdiomas();

            _amigosService.SolicitudRespondida += AmigosService_SolicitudRespondida;
            _amigosService.AmistadEliminada += AmigosService_AmistadEliminada;

            AbrirPerfilCommand = new ComandoDelegado(_ => AbrirPerfil?.Invoke());
            AbrirAjustesCommand = new ComandoDelegado(_ => AbrirAjustes?.Invoke());
            AbrirComoJugarCommand = new ComandoDelegado(_ => AbrirComoJugar?.Invoke());
            AbrirClasificacionCommand = new ComandoDelegado(_ => AbrirClasificacion?.Invoke());
            AbrirBuscarAmigoCommand = new ComandoDelegado(_ => AbrirBuscarAmigo?.Invoke());
            AbrirSolicitudesCommand = new ComandoDelegado(_ => AbrirSolicitudes?.Invoke());
            _abrirEliminarAmigoCommand = new ComandoDelegado(
                _ => AbrirEliminarAmigo?.Invoke(AmigoSeleccionado),
                _ => !string.IsNullOrWhiteSpace(AmigoSeleccionado));
            AbrirEliminarAmigoCommand = _abrirEliminarAmigoCommand;
            AbrirInvitacionesCommand = new ComandoDelegado(_ => AbrirInvitaciones?.Invoke());
            UnirseSalaCommand = new ComandoDelegado(_ => UnirseSalaInterno());
            IniciarJuegoCommand = new ComandoDelegado(_ => IniciarJuegoInterno(), _ => PuedeIniciarJuego());
        }

        public string NombreUsuario
        {
            get => _nombreUsuario;
            private set => EstablecerPropiedad(ref _nombreUsuario, value);
        }

        public string CodigoSala
        {
            get => _codigoSala;
            set => EstablecerPropiedad(ref _codigoSala, value);
        }

        public ObservableCollection<OpcionEntero> NumeroRondasOpciones
        {
            get => _numeroRondasOpciones;
            private set => EstablecerPropiedad(ref _numeroRondasOpciones, value);
        }

        public OpcionEntero NumeroRondasSeleccionada
        {
            get => _numeroRondasSeleccionada;
            set
            {
                if (EstablecerPropiedad(ref _numeroRondasSeleccionada, value))
                {
                    ActualizarEstadoIniciarJuego();
                }
            }
        }

        public ObservableCollection<OpcionEntero> TiempoRondaOpciones
        {
            get => _tiempoRondaOpciones;
            private set => EstablecerPropiedad(ref _tiempoRondaOpciones, value);
        }

        public OpcionEntero TiempoRondaSeleccionada
        {
            get => _tiempoRondaSeleccionada;
            set
            {
                if (EstablecerPropiedad(ref _tiempoRondaSeleccionada, value))
                {
                    ActualizarEstadoIniciarJuego();
                }
            }
        }

        public ObservableCollection<IdiomaOpcion> IdiomasDisponibles
        {
            get => _idiomasDisponibles;
            private set => EstablecerPropiedad(ref _idiomasDisponibles, value);
        }

        public IdiomaOpcion IdiomaSeleccionado
        {
            get => _idiomaSeleccionado;
            set
            {
                if (EstablecerPropiedad(ref _idiomaSeleccionado, value) && value != null)
                {
                    _localizacionService.EstablecerIdioma(value.Codigo);
                    ActualizarEstadoIniciarJuego();
                }
            }
        }

        public ObservableCollection<OpcionTexto> DificultadesDisponibles
        {
            get => _dificultadesDisponibles;
            private set => EstablecerPropiedad(ref _dificultadesDisponibles, value);
        }

        public OpcionTexto DificultadSeleccionada
        {
            get => _dificultadSeleccionada;
            set
            {
                if (EstablecerPropiedad(ref _dificultadSeleccionada, value))
                {
                    ActualizarEstadoIniciarJuego();
                }
            }
        }

        public ObservableCollection<string> Amigos
        {
            get => _amigos;
            private set => EstablecerPropiedad(ref _amigos, value);
        }

        public string AmigoSeleccionado
        {
            get => _amigoSeleccionado;
            set
            {
                if (EstablecerPropiedad(ref _amigoSeleccionado, value))
                {
                    _abrirEliminarAmigoCommand?.NotificarPuedeEjecutar();
                }
            }
        }

        public ICommand AbrirPerfilCommand { get; }

        public ICommand AbrirAjustesCommand { get; }

        public ICommand AbrirComoJugarCommand { get; }

        public ICommand AbrirClasificacionCommand { get; }

        public ICommand AbrirBuscarAmigoCommand { get; }

        public ICommand AbrirSolicitudesCommand { get; }

        public IComandoNotificable AbrirEliminarAmigoCommand { get; }

        public ICommand AbrirInvitacionesCommand { get; }

        public ICommand UnirseSalaCommand { get; }

        public IComandoNotificable IniciarJuegoCommand { get; }

        public Action AbrirPerfil { get; set; }

        public Action AbrirAjustes { get; set; }

        public Action AbrirComoJugar { get; set; }

        public Action AbrirClasificacion { get; set; }

        public Action AbrirBuscarAmigo { get; set; }

        public Action AbrirSolicitudes { get; set; }

        public Action<string> AbrirEliminarAmigo { get; set; }

        public Action AbrirInvitaciones { get; set; }

        public Action<string> UnirseSala { get; set; }

        public Action<ConfiguracionPartida> IniciarJuego { get; set; }

        public Action<string> MostrarMensaje { get; set; }

        public async Task InicializarAsync()
        {
            if (_amigosService == null)
            {
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                return;
            }

            try
            {
                await _amigosService.SuscribirseAsync(usuarioActual).ConfigureAwait(false);
                ActualizarAmigosDesdeServicio();
            }
            catch (ServicioException ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoAmigosOperacion);
            }
        }

        private void CargarDatosUsuario()
        {
            CodigoSala = string.Empty;
            Amigos = new ObservableCollection<string>();
            NombreUsuario = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario ?? string.Empty;
            AmigoSeleccionado = null;
        }

        private void ActualizarAmigosDesdeServicio()
        {
            if (_amigosService == null)
            {
                return;
            }

            var amigos = _amigosService.ObtenerAmigos() ?? Array.Empty<string>();

            EjecutarEnDispatcher(() =>
            {
                Amigos = new ObservableCollection<string>(amigos);
                AmigoSeleccionado = null;
            });
        }

        private void AmigosService_SolicitudRespondida(object sender, RespuestaSolicitudAmistadEventArgs e)
        {
            if (e?.Respuesta == null)
            {
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual) || !e.Respuesta.SolicitudAceptada)
            {
                return;
            }

            if (!e.Respuesta.InvolucraUsuario(usuarioActual))
            {
                return;
            }

            string amigo = e.Respuesta.ObtenerOtroUsuario(usuarioActual);
            AgregarAmigo(amigo);
        }

        private void AmigosService_AmistadEliminada(object sender, AmistadEliminadaEventArgs e)
        {
            if (e?.Amistad == null)
            {
                return;
            }

            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;
            if (string.IsNullOrWhiteSpace(usuarioActual) || !e.Amistad.InvolucraUsuario(usuarioActual))
            {
                return;
            }

            string amigo = e.Amistad.ObtenerOtroUsuario(usuarioActual);
            RemoverAmigo(amigo);
        }

        private void AgregarAmigo(string nombreAmigo)
        {
            if (string.IsNullOrWhiteSpace(nombreAmigo))
            {
                return;
            }

            EjecutarEnDispatcher(() =>
            {
                if (Amigos == null)
                {
                    Amigos = new ObservableCollection<string>();
                }

                if (!Amigos.Any(a => string.Equals(a, nombreAmigo, StringComparison.OrdinalIgnoreCase)))
                {
                    Amigos.Add(nombreAmigo);
                }
            });
        }

        private void RemoverAmigo(string nombreAmigo)
        {
            if (string.IsNullOrWhiteSpace(nombreAmigo))
            {
                return;
            }

            EjecutarEnDispatcher(() =>
            {
                if (Amigos == null)
                {
                    return;
                }

                string existente = Amigos.FirstOrDefault(a => string.Equals(a, nombreAmigo, StringComparison.OrdinalIgnoreCase));
                if (existente != null)
                {
                    Amigos.Remove(existente);
                }

                if (string.Equals(AmigoSeleccionado, nombreAmigo, StringComparison.OrdinalIgnoreCase))
                {
                    AmigoSeleccionado = null;
                }
            });
        }

        private static void EjecutarEnDispatcher(Action accion)
        {
            if (accion == null)
            {
                return;
            }

            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(accion);
                return;
            }

            accion();
        }

        private void CargarOpcionesPartida()
        {
            NumeroRondasOpciones = new ObservableCollection<OpcionEntero>(
                new[]
                {
                    new OpcionEntero(3),
                    new OpcionEntero(5),
                    new OpcionEntero(7)
                });

            NumeroRondasSeleccionada = NumeroRondasOpciones.FirstOrDefault();

            TiempoRondaOpciones = new ObservableCollection<OpcionEntero>(
                new[]
                {
                    new OpcionEntero(60),
                    new OpcionEntero(90),
                    new OpcionEntero(120)
                });

            TiempoRondaSeleccionada = TiempoRondaOpciones.FirstOrDefault();

            DificultadesDisponibles = new ObservableCollection<OpcionTexto>(
                new[]
                {
                    new OpcionTexto("facil", Lang.principalTextoFacil),
                    new OpcionTexto("media", Lang.principalTextoMedia),
                    new OpcionTexto("dificil", Lang.principalTextoDificil),
                    new OpcionTexto("mixto", Lang.principalTextoMixto)
                });

            DificultadSeleccionada = DificultadesDisponibles.FirstOrDefault();
        }

        private void CargarIdiomas()
        {
            IdiomasDisponibles = new ObservableCollection<IdiomaOpcion>
            {
                new IdiomaOpcion("es-MX", "Español"),
                new IdiomaOpcion("en-US", "English")
            };

            string culturaActual = _localizacionService.CulturaActual?.Name
                ?? CultureInfo.CurrentUICulture?.Name;

            IdiomaSeleccionado = IdiomasDisponibles
                .FirstOrDefault(i => string.Equals(i.Codigo, culturaActual, StringComparison.OrdinalIgnoreCase))
                ?? IdiomasDisponibles.FirstOrDefault();
        }

        private void UnirseSalaInterno()
        {
            string codigo = CodigoSala?.Trim();

            if (string.IsNullOrWhiteSpace(codigo))
            {
                MostrarMensaje?.Invoke(Lang.globalTextoIngreseCodigoPartida);
                return;
            }

            if (UnirseSala != null)
            {
                UnirseSala.Invoke(codigo);
            }
            else
            {
                MostrarMensaje?.Invoke(Lang.errorTextoNoEncuentraPartida);
            }
        }

        private void IniciarJuegoInterno()
        {
            if (!PuedeIniciarJuego())
            {
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            var configuracion = new ConfiguracionPartida
            {
                NumeroRondas = NumeroRondasSeleccionada?.Valor ?? 0,
                TiempoPorRondaSegundos = TiempoRondaSeleccionada?.Valor ?? 0,
                IdiomaCanciones = IdiomaSeleccionado?.Codigo,
                Dificultad = DificultadSeleccionada?.Clave
            };

            if (IniciarJuego != null)
            {
                IniciarJuego.Invoke(configuracion);
            }
            else
            {
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
            }
        }

        private bool PuedeIniciarJuego()
        {
            return NumeroRondasSeleccionada != null
                && TiempoRondaSeleccionada != null
                && IdiomaSeleccionado != null
                && DificultadSeleccionada != null;
        }

        private void ActualizarEstadoIniciarJuego()
        {
            IniciarJuegoCommand?.NotificarPuedeEjecutar();
        }

        public class OpcionEntero
        {
            public OpcionEntero(int valor)
            {
                Valor = valor;
                Descripcion = valor.ToString(CultureInfo.CurrentCulture);
            }

            public int Valor { get; }

            public string Descripcion { get; }

            public override string ToString()
            {
                return Descripcion;
            }
        }

        public class OpcionTexto
        {
            public OpcionTexto(string clave, string descripcion)
            {
                Clave = clave;
                Descripcion = descripcion;
            }

            public string Clave { get; }

            public string Descripcion { get; }

            public override string ToString()
            {
                return Descripcion;
            }
        }
    }

    public class ConfiguracionPartida
    {
        public int NumeroRondas { get; set; }

        public int TiempoPorRondaSegundos { get; set; }

        public string IdiomaCanciones { get; set; }

        public string Dificultad { get; set; }
    }
}