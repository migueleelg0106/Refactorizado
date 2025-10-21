using System;
using System.Collections.Generic;
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

        // Campos readonly (no se modifican fuera del constructor)
        private readonly string _nombreUsuarioSesion;
        private readonly ILocalizacionService _localizacionService;
        private readonly IListaAmigosService _listaAmigosService;
        private readonly IAmigosService _amigosService;

        private bool _suscripcionActiva;

        public VentanaPrincipalVistaModelo()
            : this(LocalizacionService.Instancia, new ListaAmigosService(), new AmigosService())
        {
        }

        public VentanaPrincipalVistaModelo(
            ILocalizacionService localizacionService,
            IListaAmigosService listaAmigosService,
            IAmigosService amigosService)
        {
            _localizacionService = localizacionService ?? throw new ArgumentNullException(nameof(localizacionService));
            _listaAmigosService = listaAmigosService ?? throw new ArgumentNullException(nameof(listaAmigosService));
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));

            _listaAmigosService.ListaActualizada += ListaAmigosService_ListaActualizada;

            // ✅ Asignar el usuario de sesión aquí (donde sí se permite)
            _nombreUsuarioSesion = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario ?? string.Empty;

            CargarDatosUsuario();
            CargarOpcionesPartida();
            CargarIdiomas();

            AbrirPerfilCommand = new ComandoDelegado(_ => AbrirPerfil?.Invoke());
            AbrirAjustesCommand = new ComandoDelegado(_ => AbrirAjustes?.Invoke());
            AbrirComoJugarCommand = new ComandoDelegado(_ => AbrirComoJugar?.Invoke());
            AbrirClasificacionCommand = new ComandoDelegado(_ => AbrirClasificacion?.Invoke());
            AbrirBuscarAmigoCommand = new ComandoDelegado(_ => AbrirBuscarAmigo?.Invoke());
            AbrirSolicitudesCommand = new ComandoDelegado(_ => AbrirSolicitudes?.Invoke());
            AbrirEliminarAmigoCommand = new ComandoAsincrono(_ => EliminarAmigoAsync(), _ => !string.IsNullOrWhiteSpace(AmigoSeleccionado));
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
                    AbrirEliminarAmigoCommand?.NotificarPuedeEjecutar();
                }
            }
        }

        public ICommand AbrirPerfilCommand { get; }
        public ICommand AbrirAjustesCommand { get; }
        public ICommand AbrirComoJugarCommand { get; }
        public ICommand AbrirClasificacionCommand { get; }
        public ICommand AbrirBuscarAmigoCommand { get; }
        public ICommand AbrirSolicitudesCommand { get; }
        public IComandoAsincrono AbrirEliminarAmigoCommand { get; }
        public ICommand AbrirInvitacionesCommand { get; }
        public ICommand UnirseSalaCommand { get; }
        public IComandoNotificable IniciarJuegoCommand { get; }

        public Action AbrirPerfil { get; set; }
        public Action AbrirAjustes { get; set; }
        public Action AbrirComoJugar { get; set; }
        public Action AbrirClasificacion { get; set; }
        public Action AbrirBuscarAmigo { get; set; }
        public Action AbrirSolicitudes { get; set; }
        public Func<string, bool?> ConfirmarEliminarAmigo { get; set; }
        public Action AbrirInvitaciones { get; set; }
        public Action<string> UnirseSala { get; set; }
        public Action<ConfiguracionPartida> IniciarJuego { get; set; }
        public Action<string> MostrarMensaje { get; set; }

        public async Task InicializarAsync()
        {
            if (_suscripcionActiva || string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
                return;

            try
            {
                await _listaAmigosService.SuscribirAsync(_nombreUsuarioSesion).ConfigureAwait(false);
                await _amigosService.SuscribirAsync(_nombreUsuarioSesion).ConfigureAwait(false);
                _suscripcionActiva = true;

                IReadOnlyList<Amigo> listaActual = _listaAmigosService.ListaActual;
                EjecutarEnDispatcher(() => ActualizarAmigos(listaActual));

            }
            catch (ServicioException ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
        }

        public async Task FinalizarAsync()
        {
            _listaAmigosService.ListaActualizada -= ListaAmigosService_ListaActualizada;

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
                return;

            try
            {
                await _listaAmigosService.CancelarSuscripcionAsync(_nombreUsuarioSesion).ConfigureAwait(false);
                await _amigosService.CancelarSuscripcionAsync(_nombreUsuarioSesion).ConfigureAwait(false);
            }
            catch (ServicioException)
            {
                // Ignorado
            }
            finally
            {
                _suscripcionActiva = false;
            }
        }

        private void CargarDatosUsuario()
        {
            CodigoSala = string.Empty;
            Amigos = new ObservableCollection<string>();
            NombreUsuario = _nombreUsuarioSesion; // ✅ ahora solo se usa, no se reasigna
        }

        private void CargarOpcionesPartida()
        {
            NumeroRondasOpciones = new ObservableCollection<OpcionEntero>(
                new[] { new OpcionEntero(3), new OpcionEntero(5), new OpcionEntero(7) });
            NumeroRondasSeleccionada = NumeroRondasOpciones.FirstOrDefault();

            TiempoRondaOpciones = new ObservableCollection<OpcionEntero>(
                new[] { new OpcionEntero(60), new OpcionEntero(90), new OpcionEntero(120) });
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

            string culturaActual = _localizacionService.CulturaActual?.Name ?? CultureInfo.CurrentUICulture?.Name;

            IdiomaSeleccionado = IdiomasDisponibles
                .FirstOrDefault(i => string.Equals(i.Codigo, culturaActual, StringComparison.OrdinalIgnoreCase))
                ?? IdiomasDisponibles.FirstOrDefault();
        }

        private void ListaAmigosService_ListaActualizada(object sender, IReadOnlyList<Amigo> amigos)
        {
            EjecutarEnDispatcher(() => ActualizarAmigos(amigos));
        }

        private void ActualizarAmigos(IReadOnlyList<Amigo> amigos)
        {
            if (Amigos == null)
                Amigos = new ObservableCollection<string>();

            Amigos.Clear();

            if (amigos != null)
            {
                foreach (var amigo in amigos)
                {
                    if (!string.IsNullOrWhiteSpace(amigo?.NombreUsuario))
                        Amigos.Add(amigo.NombreUsuario);
                }
            }

            if (!string.IsNullOrWhiteSpace(AmigoSeleccionado)
                && (amigos == null || !amigos.Any(a => string.Equals(a.NombreUsuario, AmigoSeleccionado, StringComparison.OrdinalIgnoreCase))))
            {
                AmigoSeleccionado = null;
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

        private async Task EliminarAmigoAsync()
        {
            string amigo = AmigoSeleccionado;

            if (string.IsNullOrWhiteSpace(amigo)) return;

            bool? confirmar = ConfirmarEliminarAmigo?.Invoke(amigo);
            if (confirmar != true) return;

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            try
            {
                await _amigosService.EliminarAmigoAsync(_nombreUsuarioSesion, amigo).ConfigureAwait(true);
                MostrarMensaje?.Invoke(Lang.amigosTextoAmigoEliminado);
            }
            catch (ServicioException ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
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
                UnirseSala.Invoke(codigo);
            else
                MostrarMensaje?.Invoke(Lang.errorTextoNoEncuentraPartida);
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
                IniciarJuego.Invoke(configuracion);
            else
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
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
            public override string ToString() => Descripcion;
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
            public override string ToString() => Descripcion;
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
