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
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Idiomas;
using PictionaryMusicalCliente.Servicios.Wcf;
using DTOs = global::Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.ClienteServicios.Wcf;

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
        private ObservableCollection<DTOs.AmigoDTO> _amigos;
        private DTOs.AmigoDTO _amigoSeleccionado;

        private readonly string _nombreUsuarioSesion;
        private readonly ILocalizacionServicio _localizacionService;
        private readonly IListaAmigosServicio _listaAmigosService;
        private readonly IAmigosServicio _amigosService;

        private bool _suscripcionActiva;

        public VentanaPrincipalVistaModelo()
            : this(LocalizacionServicio.Instancia, new ListaAmigosServicio(), new AmigosServicio())
        {
        }

        public VentanaPrincipalVistaModelo(
            ILocalizacionServicio localizacionService,
            IListaAmigosServicio listaAmigosService,
            IAmigosServicio amigosService)
        {
            _localizacionService = localizacionService ?? throw new ArgumentNullException(nameof(localizacionService));
            _listaAmigosService = listaAmigosService ?? throw new ArgumentNullException(nameof(listaAmigosService));
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));

            _listaAmigosService.ListaActualizada += ListaAmigosService_ListaActualizada;

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
            EliminarAmigoCommand = new ComandoAsincrono(
                param => EjecutarEliminarAmigoAsync(param as DTOs.AmigoDTO),
                param => param is DTOs.AmigoDTO
            );

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

        public ObservableCollection<DTOs.AmigoDTO> Amigos
        {
            get => _amigos;
            private set => EstablecerPropiedad(ref _amigos, value);
        }

        public DTOs.AmigoDTO AmigoSeleccionado
        {
            get => _amigoSeleccionado;
            set
            {
                EstablecerPropiedad(ref _amigoSeleccionado, value);
            }
        }

        public ICommand AbrirPerfilCommand { get; }
        public ICommand AbrirAjustesCommand { get; }
        public ICommand AbrirComoJugarCommand { get; }
        public ICommand AbrirClasificacionCommand { get; }
        public ICommand AbrirBuscarAmigoCommand { get; }
        public ICommand AbrirSolicitudesCommand { get; }
        public IComandoAsincrono EliminarAmigoCommand { get; }
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

                IReadOnlyList<DTOs.AmigoDTO> listaActual = _listaAmigosService.ListaActual;
                EjecutarEnDispatcher(() => ActualizarAmigos(listaActual));

            }
            catch (ExcepcionServicio ex)
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
            catch (ExcepcionServicio)
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

            Amigos = new ObservableCollection<DTOs.AmigoDTO>();

            NombreUsuario = _nombreUsuarioSesion;
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
            WeakEventManager<ILocalizacionServicio, EventArgs>.AddHandler(
                _localizacionService,
                nameof(ILocalizacionServicio.IdiomaActualizado),
                LocalizacionServiceOnIdiomaActualizado);

            ActualizarIdiomasDisponibles(_localizacionService.CulturaActual?.Name
                ?? CultureInfo.CurrentUICulture?.Name);
        }

        private void LocalizacionServiceOnIdiomaActualizado(object sender, EventArgs e)
        {
            ActualizarIdiomasDisponibles(_localizacionService.CulturaActual?.Name);
        }

        private void ActualizarIdiomasDisponibles(string culturaActual)
        {
            var opciones = new[]
            {
                new IdiomaOpcion("es-MX", Lang.idiomaTextoEspañol),
                new IdiomaOpcion("en-US", Lang.idiomaTextoIngles)
            };

            if (IdiomasDisponibles == null)
            {
                IdiomasDisponibles = new ObservableCollection<IdiomaOpcion>(opciones);
            }
            else
            {
                IdiomasDisponibles.Clear();

                foreach (var opcion in opciones)
                {
                    IdiomasDisponibles.Add(opcion);
                }
            }

            if (string.IsNullOrWhiteSpace(culturaActual))
            {
                IdiomaSeleccionado = IdiomasDisponibles.FirstOrDefault();
                return;
            }

            IdiomaSeleccionado = IdiomasDisponibles
                .FirstOrDefault(i => string.Equals(i.Codigo, culturaActual, StringComparison.OrdinalIgnoreCase))
                ?? IdiomasDisponibles.FirstOrDefault();
        }

        private void ListaAmigosService_ListaActualizada(object sender, IReadOnlyList<DTOs.AmigoDTO> amigos)
        {
            EjecutarEnDispatcher(() => ActualizarAmigos(amigos));
        }

        private void ActualizarAmigos(IReadOnlyList<DTOs.AmigoDTO> amigos)
        {
            if (Amigos == null)
                Amigos = new ObservableCollection<DTOs.AmigoDTO>();

            Amigos.Clear();

            if (amigos != null)
            {
                foreach (var amigo in amigos)
                {
                    if (!string.IsNullOrWhiteSpace(amigo?.NombreUsuario))
                        Amigos.Add(amigo);
                }
            }

            if (AmigoSeleccionado != null
                && (amigos == null || !amigos.Any(a => string.Equals(a.NombreUsuario, AmigoSeleccionado.NombreUsuario, StringComparison.OrdinalIgnoreCase))))
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

        private async Task EjecutarEliminarAmigoAsync(DTOs.AmigoDTO amigo)
        {
            if (amigo == null) return;

            bool? confirmar = ConfirmarEliminarAmigo?.Invoke(amigo.NombreUsuario);
            if (confirmar != true) return;

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            try
            {
                await _amigosService.EliminarAmigoAsync(_nombreUsuarioSesion, amigo.NombreUsuario).ConfigureAwait(true);
                MostrarMensaje?.Invoke(Lang.amigosTextoAmigoEliminado);
            }
            catch (ExcepcionServicio ex)
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