using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Amigos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Idiomas;
using PictionaryMusicalCliente.Servicios.Wcf;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class VentanaPrincipalVistaModelo : BaseVistaModelo, IDisposable
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
        private readonly IListaAmigosService _listaAmigosService;

        public VentanaPrincipalVistaModelo()
            : this(LocalizacionService.Instancia, new ListaAmigosService())
        {
        }

        public VentanaPrincipalVistaModelo(
            ILocalizacionService localizacionService,
            IListaAmigosService listaAmigosService)
        {
            _localizacionService = localizacionService ?? throw new ArgumentNullException(nameof(localizacionService));
            _listaAmigosService = listaAmigosService ?? throw new ArgumentNullException(nameof(listaAmigosService));

            _listaAmigosService.ListaActualizada += ListaAmigosService_ListaActualizada;

            CargarDatosUsuario();
            CargarOpcionesPartida();
            CargarIdiomas();

            AbrirPerfilCommand = new ComandoDelegado(_ => AbrirPerfil?.Invoke());
            AbrirAjustesCommand = new ComandoDelegado(_ => AbrirAjustes?.Invoke());
            AbrirComoJugarCommand = new ComandoDelegado(_ => AbrirComoJugar?.Invoke());
            AbrirClasificacionCommand = new ComandoDelegado(_ => AbrirClasificacion?.Invoke());
            AbrirBuscarAmigoCommand = new ComandoDelegado(_ => AbrirBuscarAmigo?.Invoke());
            AbrirSolicitudesCommand = new ComandoDelegado(_ => AbrirSolicitudes?.Invoke());
            AbrirEliminarAmigoCommand = new ComandoDelegado(
                _ => AbrirEliminarAmigo?.Invoke(AmigoSeleccionado),
                _ => !string.IsNullOrWhiteSpace(AmigoSeleccionado));
            AbrirInvitacionesCommand = new ComandoDelegado(_ => AbrirInvitaciones?.Invoke());
            UnirseSalaCommand = new ComandoDelegado(_ => UnirseSalaInterno());
            IniciarJuegoCommand = new ComandoDelegado(_ => IniciarJuegoInterno(), _ => PuedeIniciarJuego());

            _ = RecargarAmigosAsync();
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
                if (EstablecerPropiedad(ref _amigoSeleccionado, value)
                    && AbrirEliminarAmigoCommand is IComandoNotificable notificable)
                {
                    notificable.NotificarPuedeEjecutar();
                }
            }
        }

        public ICommand AbrirPerfilCommand { get; }

        public ICommand AbrirAjustesCommand { get; }

        public ICommand AbrirComoJugarCommand { get; }

        public ICommand AbrirClasificacionCommand { get; }

        public ICommand AbrirBuscarAmigoCommand { get; }

        public ICommand AbrirSolicitudesCommand { get; }

        public ICommand AbrirEliminarAmigoCommand { get; }

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

        private void CargarDatosUsuario()
        {
            CodigoSala = string.Empty;
            Amigos = new ObservableCollection<string>();
            NombreUsuario = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario ?? string.Empty;
        }

        public async Task RecargarAmigosAsync()
        {
            string nombreUsuario = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                ActualizarListaAmigos(Array.Empty<string>());
                return;
            }

            try
            {
                ListaAmigosResultado resultado = await _listaAmigosService
                    .ObtenerListaAmigosAsync(nombreUsuario)
                    .ConfigureAwait(true);

                if (resultado == null)
                {
                    MostrarMensaje?.Invoke(Lang.amigosTextoListaNoDisponible);
                    ActualizarListaAmigos(Array.Empty<string>());
                    return;
                }

                if (!resultado.Exito)
                {
                    if (!string.IsNullOrWhiteSpace(resultado.Mensaje))
                    {
                        MostrarMensaje?.Invoke(resultado.Mensaje);
                    }

                    ActualizarListaAmigos(resultado.Amigos);
                    return;
                }

                ActualizarListaAmigos(resultado.Amigos);
            }
            catch (ServicioException ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.amigosTextoListaNoDisponible);
                ActualizarListaAmigos(Array.Empty<string>());
            }
            catch (ArgumentException)
            {
                ActualizarListaAmigos(Array.Empty<string>());
            }
        }

        private void ListaAmigosService_ListaActualizada(object sender, ListaAmigosResultado resultado)
        {
            if (resultado == null)
            {
                return;
            }

            if (!resultado.Exito && !string.IsNullOrWhiteSpace(resultado.Mensaje))
            {
                MostrarMensaje?.Invoke(resultado.Mensaje);
            }

            ActualizarListaAmigos(resultado.Amigos);
        }

        private void ActualizarListaAmigos(IEnumerable<string> amigos)
        {
            IEnumerable<string> lista = amigos ?? Array.Empty<string>();
            Amigos = new ObservableCollection<string>(lista);

            if (string.IsNullOrWhiteSpace(AmigoSeleccionado))
            {
                return;
            }

            bool existe = lista.Any(a => string.Equals(a, AmigoSeleccionado, StringComparison.OrdinalIgnoreCase));

            if (!existe)
            {
                AmigoSeleccionado = null;
            }
        }

        public void Dispose()
        {
            if (_listaAmigosService != null)
            {
                _listaAmigosService.ListaActualizada -= ListaAmigosService_ListaActualizada;
                _listaAmigosService.Dispose();
            }
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