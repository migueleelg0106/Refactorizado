using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.Utilidades.Abstracciones;

namespace PictionaryMusicalCliente.VistaModelo.VentanaPrincipal
{
    public class VentanaPrincipalVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        private readonly ILocalizacionServicio _localizacion;
        private readonly IListaAmigosServicio _listaAmigosServicio;
        private readonly IAmigosServicio _amigosServicio;
        private readonly ISalasServicio _salasServicio;
        private readonly ISonidoManejador _sonidoManejador;
        private readonly IUsuarioAutenticado _usuarioSesion;

        private bool _suscripcionActiva;

        public VentanaPrincipalVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            ILocalizacionServicio localizacionServicio,
            IListaAmigosServicio listaAmigosServicio,
            IAmigosServicio amigosServicio,
            ISalasServicio salasServicio,
            ISonidoManejador sonidoManejador,
            IUsuarioAutenticado usuarioSesion)
            : base(ventana, localizador)
        {

            _localizacion = localizacionServicio ??
                throw new ArgumentNullException(nameof(localizacionServicio));
            _listaAmigosServicio = listaAmigosServicio ??
                throw new ArgumentNullException(nameof(listaAmigosServicio));
            _amigosServicio = amigosServicio ??
                throw new ArgumentNullException(nameof(amigosServicio));
            _salasServicio = salasServicio ??
                throw new ArgumentNullException(nameof(salasServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _usuarioSesion = usuarioSesion ??
                throw new ArgumentNullException(nameof(usuarioSesion));
            _listaAmigosServicio.ListaActualizada += ListaActualizada;
            _amigosServicio.SolicitudesActualizadas += SolicitudesAmistadActualizadas;

            _nombreUsuarioSesion = _usuarioSesion.NombreUsuario ?? string.Empty;

            CargarDatosUsuario();
            CargarOpcionesPartida();

            AbrirPerfilComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                AbrirPerfil?.Invoke();
            });
            AbrirAjustesComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                AbrirAjustes?.Invoke();
            });
            AbrirComoJugarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                AbrirComoJugar?.Invoke();
            });
            AbrirClasificacionComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                AbrirClasificacion?.Invoke();
            });
            AbrirBuscarAmigoComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                AbrirBuscarAmigo?.Invoke();
            });
            AbrirSolicitudesComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                EjecutarAbrirSolicitudes();
            });

            EliminarAmigoComando = new ComandoAsincrono(async param =>
            {
                _sonidoManejador.ReproducirClick();
                await EjecutarEliminarAmigoAsync(param as DTOs.AmigoDTO);
            }, param => param is DTOs.AmigoDTO);

            UnirseSalaComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await UnirseSalaInternoAsync();
            });

            IniciarJuegoComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
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
            if (!ValidarCondicionesInicializacion())
            {
                return;
            }

            await EjecutarOperacionAsync(async () =>
            {
                await SuscribirAServiciosAsync();
                MarcarSuscripcionActiva();
                await CargarListaAmigosInicialAsync();
            });
        }

        private bool ValidarCondicionesInicializacion()
        {
            return !_suscripcionActiva && !string.IsNullOrWhiteSpace(_nombreUsuarioSesion);
        }

        private async Task SuscribirAServiciosAsync()
        {
            _logger.InfoFormat("Inicializando suscripciones para usuario: {0}",
                _nombreUsuarioSesion);
            await _listaAmigosServicio.SuscribirAsync(_nombreUsuarioSesion).
                ConfigureAwait(false);
            await _amigosServicio.SuscribirAsync(_nombreUsuarioSesion).ConfigureAwait(false);
        }

        private void MarcarSuscripcionActiva()
        {
            _suscripcionActiva = true;
        }

        private async Task CargarListaAmigosInicialAsync()
        {
            IReadOnlyList<DTOs.AmigoDTO> listaActual = _listaAmigosServicio.ListaActual;
            EjecutarEnDispatcher(() => ActualizarAmigos(listaActual));
            await Task.CompletedTask;
        }

        public async Task FinalizarAsync()
        {
            DesuscribirEventos();

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                return;
            }

            await EjecutarOperacionAsync(async () =>
            {
                await CancelarSuscripcionesAsync();
                MarcarSuscripcionInactiva();
            });
        }

        private void DesuscribirEventos()
        {
            _listaAmigosServicio.ListaActualizada -= ListaActualizada;
            _amigosServicio.SolicitudesActualizadas -= SolicitudesAmistadActualizadas;
        }

        private async Task CancelarSuscripcionesAsync()
        {
            _logger.Info("Cancelando suscripciones al finalizar ventana principal.");
            await _listaAmigosServicio.CancelarSuscripcionAsync(
                _nombreUsuarioSesion).ConfigureAwait(false);
            await _amigosServicio.CancelarSuscripcionAsync(
                _nombreUsuarioSesion).ConfigureAwait(false);
        }

        private void MarcarSuscripcionInactiva()
        {
            _suscripcionActiva = false;
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
                new[] { new OpcionEntero(2), new OpcionEntero(3), new OpcionEntero(4) });
            NumeroRondasSeleccionada = NumeroRondasOpciones.FirstOrDefault();

            TiempoRondaOpciones = new ObservableCollection<OpcionEntero>(
                new[] { new OpcionEntero(60), new OpcionEntero(90), new OpcionEntero(120) });
            TiempoRondaSeleccionada = TiempoRondaOpciones.FirstOrDefault();

            IdiomasDisponibles = new ObservableCollection<IdiomaOpcion>(
                new[]
                {
                    new IdiomaOpcion("es-MX", Lang.idiomaTextoEspañol),
                    new IdiomaOpcion("en-US", Lang.idiomaTextoIngles),
                    new IdiomaOpcion("mixto", Lang.principalTextoMixto)
                });

            IdiomaSeleccionado = IdiomasDisponibles.FirstOrDefault();

            DificultadesDisponibles = new ObservableCollection<OpcionTexto>(
                new[]
                {
                    new OpcionTexto("facil", Lang.principalTextoFacil),
                    new OpcionTexto("media", Lang.principalTextoMedia),
                    new OpcionTexto("dificil", Lang.principalTextoDificil)
                });
            DificultadSeleccionada = DificultadesDisponibles.FirstOrDefault();
        }

        private void ListaActualizada(object sender, IReadOnlyList<DTOs.AmigoDTO> amigos)
        {
            EjecutarEnDispatcher(() => ActualizarAmigos(amigos));
        }

        private void SolicitudesAmistadActualizadas(
            object sender,
            IReadOnlyCollection<DTOs.SolicitudAmistadDTO> solicitudes)
        {
            _ = ActualizarListaAmigosDesdeServidorAsync();
        }

        private void ActualizarAmigos(IReadOnlyList<DTOs.AmigoDTO> amigos)
        {
            InicializarListaAmigos();
            LimpiarListaAmigos();
            AgregarAmigosValidos(amigos);
            ValidarAmigoSeleccionado(amigos);
        }

        private void InicializarListaAmigos()
        {
            if (Amigos == null)
            {
                Amigos = new ObservableCollection<DTOs.AmigoDTO>();
            }
        }

        private void LimpiarListaAmigos()
        {
            Amigos.Clear();
        }

        private void AgregarAmigosValidos(IReadOnlyList<DTOs.AmigoDTO> amigos)
        {
            if (amigos != null)
            {
                foreach (var amigo in amigos.Where(a =>
                    !string.IsNullOrWhiteSpace(a?.NombreUsuario)))
                {
                    Amigos.Add(amigo);
                }
            }
        }

        private void ValidarAmigoSeleccionado(IReadOnlyList<DTOs.AmigoDTO> amigos)
        {
            if (AmigoSeleccionado != null
                && (amigos == null || !amigos.Any(a => string.Equals(
                    a.NombreUsuario,
                    AmigoSeleccionado.NombreUsuario,
                    StringComparison.OrdinalIgnoreCase))))
            {
                AmigoSeleccionado = null;
            }
        }

        private async Task ActualizarListaAmigosDesdeServidorAsync()
        {
            if (!ValidarSesionParaActualizarAmigos())
            {
                return;
            }

            await EjecutarOperacionAsync(async () =>
            {
                var amigos = await ObtenerAmigosDelServidorAsync();
                ActualizarListaEnDispatcher(amigos);
            });
        }

        private bool ValidarSesionParaActualizarAmigos()
        {
            return !string.IsNullOrWhiteSpace(_nombreUsuarioSesion);
        }

        private async Task<IReadOnlyList<DTOs.AmigoDTO>> ObtenerAmigosDelServidorAsync()
        {
            return await _listaAmigosServicio.ObtenerAmigosAsync(
                _nombreUsuarioSesion).ConfigureAwait(false);
        }

        private void ActualizarListaEnDispatcher(IReadOnlyList<DTOs.AmigoDTO> amigos)
        {
            EjecutarEnDispatcher(() => ActualizarAmigos(amigos));
        }

        private static void EjecutarEnDispatcher(Action accion)
        {
            if (accion == null)
            {
                return;
            }

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

        private async Task EjecutarEliminarAmigoAsync(DTOs.AmigoDTO amigo)
        {
            if (!ValidarAmigoParaEliminar(amigo))
            {
                return;
            }

            if (!SolicitarConfirmacionEliminacion(amigo.NombreUsuario))
            {
                return;
            }

            if (!ValidarSesionActivaParaEliminar())
            {
                ManejarErrorSesionInactiva();
                return;
            }

            await EjecutarOperacionAsync(async () =>
            {
                await EliminarAmigoEnServidorAsync(amigo.NombreUsuario);
                await ActualizarListaTrasEliminacion();
                MostrarExitoEliminacion();
            });
        }

        private bool ValidarAmigoParaEliminar(DTOs.AmigoDTO amigo)
        {
            return amigo != null;
        }

        private bool SolicitarConfirmacionEliminacion(string nombreAmigo)
        {
            bool? confirmar = ConfirmarEliminarAmigo?.Invoke(nombreAmigo);
            return confirmar == true;
        }

        private bool ValidarSesionActivaParaEliminar()
        {
            return !string.IsNullOrWhiteSpace(_nombreUsuarioSesion);
        }

        private void ManejarErrorSesionInactiva()
        {
            _logger.Warn("Intento de eliminar amigo sin sesion activa.");
            _sonidoManejador.ReproducirError();
            MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
        }

        private async Task EliminarAmigoEnServidorAsync(string nombreAmigo)
        {
            _logger.InfoFormat("Eliminando amigo: {0}", nombreAmigo);
            await _amigosServicio.EliminarAmigoAsync(
                _nombreUsuarioSesion,
                nombreAmigo).ConfigureAwait(true);
        }

        private async Task ActualizarListaTrasEliminacion()
        {
            await ActualizarListaAmigosDesdeServidorAsync().ConfigureAwait(true);
        }

        private void MostrarExitoEliminacion()
        {
            _sonidoManejador.ReproducirNotificacion();
            MostrarMensaje?.Invoke(Lang.amigosTextoAmigoEliminado);
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
            string codigo = ValidarCodigoSala();
            if (codigo == null)
            {
                ManejarErrorCodigoInvalido();
                return;
            }

            if (!ValidarSesionActivaParaUnirse())
            {
                ManejarErrorSesionInactivaUnirse();
                return;
            }

            await EjecutarOperacionAsync(async () =>
            {
                var sala = await UnirseSalaEnServidorAsync(codigo);
                NavegarASalaUnida(sala);
            });
        }

        private string ValidarCodigoSala()
        {
            string codigo = CodigoSala?.Trim();
            return string.IsNullOrWhiteSpace(codigo) ? null : codigo;
        }

        private void ManejarErrorCodigoInvalido()
        {
            _sonidoManejador.ReproducirError();
            MostrarMensaje?.Invoke(Lang.unirseSalaTextoVacio);
        }

        private bool ValidarSesionActivaParaUnirse()
        {
            return !string.IsNullOrWhiteSpace(_nombreUsuarioSesion);
        }

        private void ManejarErrorSesionInactivaUnirse()
        {
            _sonidoManejador.ReproducirError();
            MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
        }

        private async Task<DTOs.SalaDTO> UnirseSalaEnServidorAsync(string codigo)
        {
            _logger.InfoFormat("Intentando unirse a sala: {0}", codigo);
            return await _salasServicio.UnirseSalaAsync(
                codigo,
                _nombreUsuarioSesion).ConfigureAwait(true);
        }

        private void NavegarASalaUnida(DTOs.SalaDTO sala)
        {
            _sonidoManejador.ReproducirNotificacion();
            UnirseSala?.Invoke(sala);
        }

        private async Task IniciarJuegoInternoAsync()
        {
            if (!ValidarConfiguracionJuego())
            {
                ManejarErrorConfiguracionInvalida();
                return;
            }

            if (!ValidarSesionActivaParaIniciar())
            {
                ManejarErrorSesionInactivaIniciar();
                return;
            }

            await EjecutarOperacionAsync(async () =>
            {
                var configuracion = CrearConfiguracionPartida();
                var sala = await CrearSalaEnServidorAsync(configuracion);
                NavegarASalaCreada(sala);
            });
        }

        private bool ValidarConfiguracionJuego()
        {
            return PuedeIniciarJuego();
        }

        private void ManejarErrorConfiguracionInvalida()
        {
            _sonidoManejador.ReproducirError();
            MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
        }

        private bool ValidarSesionActivaParaIniciar()
        {
            return !string.IsNullOrWhiteSpace(_nombreUsuarioSesion);
        }

        private void ManejarErrorSesionInactivaIniciar()
        {
            _sonidoManejador.ReproducirError();
            MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
        }

        private DTOs.ConfiguracionPartidaDTO CrearConfiguracionPartida()
        {
            return new DTOs.ConfiguracionPartidaDTO
            {
                NumeroRondas = NumeroRondasSeleccionada?.Valor ?? 0,
                TiempoPorRondaSegundos = TiempoRondaSeleccionada?.Valor ?? 0,
                IdiomaCanciones = IdiomaSeleccionado?.Codigo,
                Dificultad = DificultadSeleccionada?.Clave
            };
        }

        private async Task<DTOs.SalaDTO> CrearSalaEnServidorAsync(
            DTOs.ConfiguracionPartidaDTO configuracion)
        {
            _logger.Info("Creando nueva sala de juego.");
            return await _salasServicio.CrearSalaAsync(
                _nombreUsuarioSesion,
                configuracion).ConfigureAwait(true);
        }

        private void NavegarASalaCreada(DTOs.SalaDTO sala)
        {
            _sonidoManejador.ReproducirNotificacion();
            IniciarJuego?.Invoke(sala);
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