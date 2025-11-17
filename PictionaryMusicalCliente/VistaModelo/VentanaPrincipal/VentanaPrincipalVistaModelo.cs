using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Idiomas;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.VentanaPrincipal
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
        private readonly ILocalizacionServicio _localizacionServicio;
        private readonly IListaAmigosServicio _listaAmigosServicio;
        private readonly IAmigosServicio _amigosServicio;
        private readonly ISalasServicio _salasServicio;

        private bool _suscripcionActiva;

        public VentanaPrincipalVistaModelo()
            : this(LocalizacionServicio.Instancia, new ListaAmigosServicio(), new AmigosServicio(), new SalasServicio())
        {
        }

        public VentanaPrincipalVistaModelo(
            ILocalizacionServicio localizacionServicio,
            IListaAmigosServicio listaAmigosServicio,
            IAmigosServicio amigosServicio,
            ISalasServicio salasServicio)
        {
            _localizacionServicio = localizacionServicio ?? throw new ArgumentNullException(nameof(localizacionServicio));
            _listaAmigosServicio = listaAmigosServicio ?? throw new ArgumentNullException(nameof(listaAmigosServicio));
            _amigosServicio = amigosServicio ?? throw new ArgumentNullException(nameof(amigosServicio));
            _salasServicio = salasServicio ?? throw new ArgumentNullException(nameof(salasServicio));

            _listaAmigosServicio.ListaActualizada += ListaActualizada;

            _nombreUsuarioSesion = SesionUsuarioActual.Usuario?.NombreUsuario ?? string.Empty;

            CargarDatosUsuario();
            CargarOpcionesPartida();
            CargarIdiomas();

            AbrirPerfilComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                AbrirPerfil?.Invoke();
            });
            AbrirAjustesComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                AbrirAjustes?.Invoke();
            });
            AbrirComoJugarComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                AbrirComoJugar?.Invoke();
            });
            AbrirClasificacionComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                AbrirClasificacion?.Invoke();
            });
            AbrirBuscarAmigoComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                AbrirBuscarAmigo?.Invoke();
            });
            AbrirSolicitudesComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                EjecutarAbrirSolicitudes();
            });

            EliminarAmigoComando = new ComandoAsincrono(async param =>
            {
                ManejadorSonido.ReproducirClick();
                await EjecutarEliminarAmigoAsync(param as DTOs.AmigoDTO);
            }, param => param is DTOs.AmigoDTO);

            UnirseSalaComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await UnirseSalaInternoAsync();
            });

            IniciarJuegoComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await IniciarJuegoInternoAsync();
            }, _ => PuedeIniciarJuego());
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

        public ICommand AbrirPerfilComando { get; }
        public ICommand AbrirAjustesComando { get; }
        public ICommand AbrirComoJugarComando { get; }
        public ICommand AbrirClasificacionComando { get; }
        public ICommand AbrirBuscarAmigoComando { get; }
        public ICommand AbrirSolicitudesComando { get; }
        public IComandoAsincrono EliminarAmigoComando { get; }
        public IComandoAsincrono UnirseSalaComando { get; }
        public IComandoAsincrono IniciarJuegoComando { get; }

        public Action AbrirPerfil { get; set; }
        public Action AbrirAjustes { get; set; }
        public Action AbrirComoJugar { get; set; }
        public Action AbrirClasificacion { get; set; }
        public Action AbrirBuscarAmigo { get; set; }
        public Action AbrirSolicitudes { get; set; }
        public Func<string, bool?> ConfirmarEliminarAmigo { get; set; }
        public Action<DTOs.SalaDTO> UnirseSala { get; set; }
        public Action<DTOs.SalaDTO> IniciarJuego { get; set; }
        public Action<string> MostrarMensaje { get; set; }

        public async Task InicializarAsync()
        {
            if (_suscripcionActiva || string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
                return;

            try
            {
                await _listaAmigosServicio.SuscribirAsync(_nombreUsuarioSesion).ConfigureAwait(false);
                await _amigosServicio.SuscribirAsync(_nombreUsuarioSesion).ConfigureAwait(false);
                _suscripcionActiva = true;

                IReadOnlyList<DTOs.AmigoDTO> listaActual = _listaAmigosServicio.ListaActual;
                EjecutarEnDispatcher(() => ActualizarAmigos(listaActual));

            }
            catch (ServicioExcepcion ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
        }

        public async Task FinalizarAsync()
        {
            _listaAmigosServicio.ListaActualizada -= ListaActualizada;

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
                return;

            try
            {
                await _listaAmigosServicio.CancelarSuscripcionAsync(_nombreUsuarioSesion).ConfigureAwait(false);
                await _amigosServicio.CancelarSuscripcionAsync(_nombreUsuarioSesion).ConfigureAwait(false);
            }
            catch (ServicioExcepcion)
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
                    new OpcionTexto("dificil", Lang.principalTextoDificil)
                });
            DificultadSeleccionada = DificultadesDisponibles.FirstOrDefault();
        }

        private void CargarIdiomas()
        {
            WeakEventManager<ILocalizacionServicio, EventArgs>.AddHandler(
                _localizacionServicio,
                nameof(ILocalizacionServicio.IdiomaActualizado),
                LocalizacionServicioEnIdiomaActualizado);

            ActualizarIdiomasDisponibles(_localizacionServicio.CulturaActual?.Name
                ?? CultureInfo.CurrentUICulture?.Name);
        }

        private void LocalizacionServicioEnIdiomaActualizado(object sender, EventArgs e)
        {
            ActualizarIdiomasDisponibles(_localizacionServicio.CulturaActual?.Name);
        }

        private void ActualizarIdiomasDisponibles(string culturaActual)
        {
            var opciones = new[]
            {
                new IdiomaOpcion("es-MX", Lang.idiomaTextoEspañol),
                new IdiomaOpcion("en-US", Lang.idiomaTextoIngles),
                new IdiomaOpcion("mixto", Lang.principalTextoMixto)
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

        private void ListaActualizada(object sender, IReadOnlyList<DTOs.AmigoDTO> amigos)
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
                foreach (var amigo in amigos.Where(a => !string.IsNullOrWhiteSpace(a?.NombreUsuario)))
                {
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
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            try
            {
                ManejadorSonido.ReproducirExito();
                await _amigosServicio.EliminarAmigoAsync(_nombreUsuarioSesion, amigo.NombreUsuario).ConfigureAwait(true);
                MostrarMensaje?.Invoke(Lang.amigosTextoAmigoEliminado);
            }
            catch (ServicioExcepcion ex)
            {
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
        }

        private void EjecutarAbrirSolicitudes()
        {
            var solicitudesPendientes = _amigosServicio?.SolicitudesPendientes;

            if (solicitudesPendientes == null || solicitudesPendientes.Count == 0)
            {
                MostrarMensaje?.Invoke(Lang.amigosAvisoSinSolicitudesPendientes);
                return;
            }

            AbrirSolicitudes?.Invoke();
        }

        private async Task UnirseSalaInternoAsync()
        {
            string codigo = CodigoSala?.Trim();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(Lang.globalTextoIngreseCodigoPartida);
                return;
            }

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            try
            {
                var sala = await _salasServicio.UnirseSalaAsync(codigo, _nombreUsuarioSesion).ConfigureAwait(true);
                ManejadorSonido.ReproducirExito();
                UnirseSala?.Invoke(sala);
            }
            catch (ServicioExcepcion ex)
            {
                ManejadorSonido.ReproducirError();
                string mensajeServidor = ex?.Message;
                string mensajeLocalizado = MensajeServidorAyudante.Localizar(mensajeServidor, mensajeServidor);

                bool esSalaLlena = MensajeServidorAyudante.CoincideConMensaje(
                    mensajeServidor,
                    "La sala está llena",
                    Lang.errorTextoSalaLlena);

                bool esSalaNoEncontrada = MensajeServidorAyudante.CoincideConMensaje(
                    mensajeServidor,
                    "No se encontró la sala especificada",
                    Lang.errorTextoNoEncuentraPartida);

                if (esSalaLlena)
                {
                    MostrarMensaje?.Invoke(Lang.errorTextoSalaLlena);
                }
                else if (ex?.Tipo == TipoErrorServicio.FallaServicio || esSalaNoEncontrada)
                {
                    MostrarMensaje?.Invoke(Lang.errorTextoNoEncuentraPartida);
                }
                else
                {
                    MostrarMensaje?.Invoke(mensajeLocalizado ?? Lang.errorTextoErrorProcesarSolicitud);
                }
            }
        }

        private async Task IniciarJuegoInternoAsync()
        {
            if (!PuedeIniciarJuego())
            {
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            var configuracion = new DTOs.ConfiguracionPartidaDTO
            {
                NumeroRondas = NumeroRondasSeleccionada?.Valor ?? 0,
                TiempoPorRondaSegundos = TiempoRondaSeleccionada?.Valor ?? 0,
                IdiomaCanciones = IdiomaSeleccionado?.Codigo,
                Dificultad = DificultadSeleccionada?.Clave
            };

            try
            {
                var sala = await _salasServicio.CrearSalaAsync(_nombreUsuarioSesion, configuracion).ConfigureAwait(true);
                ManejadorSonido.ReproducirExito();
                IniciarJuego?.Invoke(sala);
            }
            catch (ServicioExcepcion ex)
            {
                ManejadorSonido.ReproducirError();
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
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
            IniciarJuegoComando?.NotificarPuedeEjecutar();
        }
    }
}