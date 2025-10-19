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
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Idiomas;
using PictionaryMusicalCliente.Servicios;
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
        private readonly IListaAmigosService _listaAmigosService;

        public VentanaPrincipalVistaModelo()
            : this(LocalizacionService.Instancia, ListaAmigosService.Instancia)
        {
        }

        public VentanaPrincipalVistaModelo(
            ILocalizacionService localizacionService,
            IListaAmigosService listaAmigosService)
        {
            _localizacionService = localizacionService ?? throw new ArgumentNullException(nameof(localizacionService));
            _listaAmigosService = listaAmigosService ?? throw new ArgumentNullException(nameof(listaAmigosService));

            CargarDatosUsuario();
            CargarOpcionesPartida();
            CargarIdiomas();

            AbrirPerfilCommand = new ComandoDelegado(_ => AbrirPerfil?.Invoke());
            AbrirAjustesCommand = new ComandoDelegado(_ => AbrirAjustes?.Invoke());
            AbrirComoJugarCommand = new ComandoDelegado(_ => AbrirComoJugar?.Invoke());
            AbrirClasificacionCommand = new ComandoDelegado(_ => AbrirClasificacion?.Invoke());
            AbrirBuscarAmigoCommand = new ComandoDelegado(_ => AbrirBuscarAmigo?.Invoke());
            AbrirSolicitudesCommand = new ComandoDelegado(_ => AbrirSolicitudes?.Invoke());
            AbrirEliminarAmigoCommand = new ComandoDelegado(_ => EjecutarAbrirEliminarAmigo());
            AbrirInvitacionesCommand = new ComandoDelegado(_ => AbrirInvitaciones?.Invoke());
            UnirseSalaCommand = new ComandoDelegado(_ => UnirseSalaInterno());
            IniciarJuegoCommand = new ComandoDelegado(_ => IniciarJuegoInterno(), _ => PuedeIniciarJuego());

            _listaAmigosService.ListaActualizada += ListaAmigosService_ListaActualizada;

            _ = CargarListaAmigosAsync();
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
            set => EstablecerPropiedad(ref _amigoSeleccionado, value);
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

        private async Task CargarListaAmigosAsync()
        {
            try
            {
                ListaAmigosResultado resultado = await _listaAmigosService.ObtenerListaAmigosAsync();

                if (resultado?.Resultado == null)
                {
                    return;
                }

                if (resultado.Resultado.Exito)
                {
                    ActualizarAmigos(resultado.Amigos);
                }
                else if (!string.IsNullOrWhiteSpace(resultado.Resultado.Mensaje))
                {
                    MostrarMensaje?.Invoke(resultado.Resultado.Mensaje);
                }
            }
            catch (ServicioException ex)
            {
                string mensaje = ex.Message;
                if (string.IsNullOrWhiteSpace(mensaje))
                {
                    mensaje = Lang.errorTextoServidorNoDisponible;
                }
                MostrarMensaje?.Invoke(mensaje);
            }
            catch (Exception)
            {
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
            }
        }

        private void ListaAmigosService_ListaActualizada(object sender, ListaAmigosActualizadaEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(NombreUsuario)
                && !string.Equals(NombreUsuario, e.Jugador, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            ActualizarAmigos(e.Amigos);
        }

        private void ActualizarAmigos(System.Collections.Generic.IReadOnlyList<string> amigos)
        {
            if (Amigos == null)
            {
                Amigos = new ObservableCollection<string>();
            }

            void Actualizar()
            {
                Amigos.Clear();
                if (amigos != null)
                {
                    foreach (string amigo in amigos)
                    {
                        if (!string.IsNullOrWhiteSpace(amigo))
                        {
                            Amigos.Add(amigo);
                        }
                    }
                }
            }

            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(Actualizar);
            }
            else
            {
                Actualizar();
            }
        }

        private void EjecutarAbrirEliminarAmigo()
        {
            string amigo = AmigoSeleccionado;

            if (string.IsNullOrWhiteSpace(amigo))
            {
                MostrarMensaje?.Invoke(Lang.amigosTextoSeleccioneAmigo);
                return;
            }

            AbrirEliminarAmigo?.Invoke(amigo);
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