using PictionaryMusicalCliente.ClienteServicios;
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
    /// <summary>
    /// ViewModel principal de la aplicacion que gestiona el lobby, amigos y creacion de partidas.
    /// </summary>
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
        private readonly ILocalizadorServicio _localizador;

        private bool _suscripcionActiva;

        /// <summary>
        /// Constructor por defecto que inicializa los servicios estandar.
        /// </summary>
        public VentanaPrincipalVistaModelo(ISonidoManejador sonidoManejador,
            IUsuarioAutenticado usuarioSesion, ILocalizacionServicio localizacion)
        {
            _sonidoManejador = sonidoManejador
                ?? throw new ArgumentNullException(nameof(sonidoManejador));
            _usuarioSesion = usuarioSesion ??
                throw new ArgumentNullException(nameof(usuarioSesion));
            _localizacion = localizacion ??
                throw new ArgumentNullException(nameof(localizacion));
        }

        /// <summary>
        /// Inicializa el ViewModel con las dependencias inyectadas.
        /// </summary>
        public VentanaPrincipalVistaModelo(
            ILocalizacionServicio localizacionServicio,
            IListaAmigosServicio listaAmigosServicio,
            IAmigosServicio amigosServicio,
            ISalasServicio salasServicio,
            ISonidoManejador sonidoManejador,
            IUsuarioAutenticado usuarioSesion,
            ILocalizadorServicio localizador)
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
            _localizador = localizador ?? 
                throw new ArgumentNullException(nameof(localizador));

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

        /// <summary>
        /// Nombre del usuario conectado para mostrar en la interfaz.
        /// </summary>
        public string NombreUsuario
        {
            get => _nombreUsuario;
            private set => EstablecerPropiedad(ref _nombreUsuario, value);
        }

        /// <summary>
        /// Codigo de sala ingresado por el usuario para unirse.
        /// </summary>
        public string CodigoSala
        {
            get => _codigoSala;
            set => EstablecerPropiedad(ref _codigoSala, value);
        }

        /// <summary>
        /// Opciones disponibles para la configuracion de rondas.
        /// </summary>
        public ObservableCollection<OpcionEntero> NumeroRondasOpciones
        {
            get => _numeroRondasOpciones;
            private set => EstablecerPropiedad(ref _numeroRondasOpciones, value);
        }

        /// <summary>
        /// Cantidad de rondas seleccionada para la nueva partida.
        /// </summary>
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

        /// <summary>
        /// Opciones de tiempo limite por ronda.
        /// </summary>
        public ObservableCollection<OpcionEntero> TiempoRondaOpciones
        {
            get => _tiempoRondaOpciones;
            private set => EstablecerPropiedad(ref _tiempoRondaOpciones, value);
        }

        /// <summary>
        /// Tiempo por ronda seleccionado.
        /// </summary>
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

        /// <summary>
        /// Lista de idiomas disponibles para la configuracion de la partida.
        /// </summary>
        public ObservableCollection<IdiomaOpcion> IdiomasDisponibles
        {
            get => _idiomasDisponibles;
            private set => EstablecerPropiedad(ref _idiomasDisponibles, value);
        }

        /// <summary>
        /// Idioma de las canciones seleccionado.
        /// </summary>
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

        /// <summary>
        /// Niveles de dificultad disponibles.
        /// </summary>
        public ObservableCollection<OpcionTexto> DificultadesDisponibles
        {
            get => _dificultadesDisponibles;
            private set => EstablecerPropiedad(ref _dificultadesDisponibles, value);
        }

        /// <summary>
        /// Dificultad seleccionada para la partida.
        /// </summary>
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

        /// <summary>
        /// Lista observable de amigos conectados.
        /// </summary>
        public ObservableCollection<DTOs.AmigoDTO> Amigos
        {
            get => _amigos;
            private set => EstablecerPropiedad(ref _amigos, value);
        }

        /// <summary>
        /// Amigo seleccionado actualmente en la lista.
        /// </summary>
        public DTOs.AmigoDTO AmigoSeleccionado
        {
            get => _amigoSeleccionado;
            set
            {
                EstablecerPropiedad(ref _amigoSeleccionado, value);
            }
        }

        /// <summary>
        /// Comando para abrir el perfil del usuario.
        /// </summary>
        public ICommand AbrirPerfilComando { get; }
        /// <summary>
        /// Comando para abrir la ventana de ajustes.
        /// </summary>
        public ICommand AbrirAjustesComando { get; }
        /// <summary>
        /// Comando para abrir la ventana de instrucciones.
        /// </summary>
        public ICommand AbrirComoJugarComando { get; }
        /// <summary>
        /// Comando para abrir la tabla de clasificacion.
        /// </summary>
        public ICommand AbrirClasificacionComando { get; }
        /// <summary>
        /// Comando para abrir la busqueda de amigos.
        /// </summary>
        public ICommand AbrirBuscarAmigoComando { get; }
        /// <summary>
        /// Comando para ver las solicitudes de amistad.
        /// </summary>
        public ICommand AbrirSolicitudesComando { get; }
        /// <summary>
        /// Comando asincrono para eliminar un amigo de la lista.
        /// </summary>
        public IComandoAsincrono EliminarAmigoComando { get; }
        /// <summary>
        /// Comando asincrono para unirse a una sala existente.
        /// </summary>
        public IComandoAsincrono UnirseSalaComando { get; }
        /// <summary>
        /// Comando asincrono para crear e iniciar una nueva partida.
        /// </summary>
        public IComandoAsincrono IniciarJuegoComando { get; }

        /// <summary>
        /// Accion para navegar al perfil.
        /// </summary>
        public Action AbrirPerfil { get; set; }
        /// <summary>
        /// Accion para navegar a ajustes.
        /// </summary>
        public Action AbrirAjustes { get; set; }
        /// <summary>
        /// Accion para navegar a como jugar.
        /// </summary>
        public Action AbrirComoJugar { get; set; }
        /// <summary>
        /// Accion para navegar a clasificacion.
        /// </summary>
        public Action AbrirClasificacion { get; set; }
        /// <summary>
        /// Accion para navegar a buscar amigo.
        /// </summary>
        public Action AbrirBuscarAmigo { get; set; }
        /// <summary>
        /// Accion para navegar a solicitudes.
        /// </summary>
        public Action AbrirSolicitudes { get; set; }
        /// <summary>
        /// Funcion para confirmar eliminacion de amigo.
        /// </summary>
        public Func<string, bool?> ConfirmarEliminarAmigo { get; set; }
        /// <summary>
        /// Accion al unirse exitosamente a una sala.
        /// </summary>
        public Action<DTOs.SalaDTO> UnirseSala { get; set; }
        /// <summary>
        /// Accion al iniciar exitosamente una partida.
        /// </summary>
        public Action<DTOs.SalaDTO> IniciarJuego { get; set; }
        /// <summary>
        /// Accion para mostrar mensajes al usuario.
        /// </summary>
        public Action<string> MostrarMensaje { get; set; }

        /// <summary>
        /// Inicia las suscripciones a los servicios de notificacion en tiempo real.
        /// </summary>
        public async Task InicializarAsync()
        {
            if (_suscripcionActiva || string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                return;
            }

            try
            {
				_logger.InfoFormat("Inicializando suscripciones para usuario: {0}",
                    _nombreUsuarioSesion);
                await _listaAmigosServicio.SuscribirAsync(_nombreUsuarioSesion).
                    ConfigureAwait(false);
                await _amigosServicio.SuscribirAsync(_nombreUsuarioSesion).ConfigureAwait(false);
                _suscripcionActiva = true;

                IReadOnlyList<DTOs.AmigoDTO> listaActual = _listaAmigosServicio.ListaActual;
                EjecutarEnDispatcher(() => ActualizarAmigos(listaActual));

            }
            catch (ServicioExcepcion ex)
            {
                _logger.Error("Error al inicializar suscripciones.", ex);
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
        }

        /// <summary>
        /// Cierra las conexiones y libera recursos al cerrar la ventana.
        /// </summary>
        public async Task FinalizarAsync()
        {
            _listaAmigosServicio.ListaActualizada -= ListaActualizada;
            _amigosServicio.SolicitudesActualizadas -= SolicitudesAmistadActualizadas;

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                return;
            }

            try
            {
                _logger.Info("Cancelando suscripciones al finalizar ventana principal.");
                await _listaAmigosServicio.CancelarSuscripcionAsync(
                    _nombreUsuarioSesion).ConfigureAwait(false);
                await _amigosServicio.CancelarSuscripcionAsync(
                    _nombreUsuarioSesion).ConfigureAwait(false);
            }
            catch (ServicioExcepcion ex)
            {
                _logger.WarnFormat("Error al cancelar suscripciones (ignorado): {0}",
                    ex.Message);
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
            if (Amigos == null)
            {
                Amigos = new ObservableCollection<DTOs.AmigoDTO>();
            }

            Amigos.Clear();

            if (amigos != null)
            {
                foreach (var amigo in amigos.Where(a => !string.IsNullOrWhiteSpace
                (a?.NombreUsuario)))
                {
                    Amigos.Add(amigo);
                }
            }

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
            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                return;
            }

            try
            {
                var amigos = await _listaAmigosServicio.ObtenerAmigosAsync(
                    _nombreUsuarioSesion).ConfigureAwait(false);

                EjecutarEnDispatcher(() => ActualizarAmigos(amigos));
            }
            catch (ServicioExcepcion ex)
            {
                _logger.Warn("No se pudo actualizar la lista de amigos tras cambios en solicitudes.", ex);
            }
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
            if (amigo == null)
            {
                return;
            }

            bool? confirmar = ConfirmarEliminarAmigo?.Invoke(amigo.NombreUsuario);
            if (confirmar != true)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                _logger.Warn("Intento de eliminar amigo sin sesión activa.");
                _sonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            try
            {
                _logger.InfoFormat("Eliminando amigo: {0}",
                    amigo.NombreUsuario);
                _sonidoManejador.ReproducirExito();
                await _amigosServicio.EliminarAmigoAsync(
                    _nombreUsuarioSesion,
                    amigo.NombreUsuario).ConfigureAwait(true);
                await ActualizarListaAmigosDesdeServidorAsync().ConfigureAwait(true);
                MostrarMensaje?.Invoke(Lang.amigosTextoAmigoEliminado);
            }
            catch (ServicioExcepcion ex)
            {
                _logger.Error("Error al eliminar amigo.", ex);
                _sonidoManejador.ReproducirError();
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
                _sonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(Lang.unirseSalaTextoVacio);
                return;
            }

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                _sonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            try
            {
                _logger.InfoFormat("Intentando unirse a sala: {0}",
                    codigo);
                var sala = await _salasServicio.UnirseSalaAsync(
                    codigo,
                    _nombreUsuarioSesion).ConfigureAwait(true);

                _sonidoManejador.ReproducirExito();
                UnirseSala?.Invoke(sala);
            }
            catch (ServicioExcepcion ex)
            {
                _logger.Error("Error al unirse a sala.", ex);

                string mensaje;
                if (string.IsNullOrWhiteSpace(ex?.Message))
                {
                    mensaje = Lang.errorTextoNoEncuentraPartida;
                }
                else
                {
                    mensaje = ex.Message;
                }

                _sonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(mensaje);
            }
        }

        private async Task IniciarJuegoInternoAsync()
        {
            if (!PuedeIniciarJuego())
            {
                _sonidoManejador.ReproducirError();
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            if (string.IsNullOrWhiteSpace(_nombreUsuarioSesion))
            {
                _sonidoManejador.ReproducirError();
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
                _logger.Info("Creando nueva sala de juego.");
                var sala = await _salasServicio.CrearSalaAsync(
                    _nombreUsuarioSesion,
                    configuracion).ConfigureAwait(true);

                _sonidoManejador.ReproducirExito();
                IniciarJuego?.Invoke(sala);
            }
            catch (ServicioExcepcion ex)
            {
                _logger.Error("Error al crear sala de juego.", ex);
                _sonidoManejador.ReproducirError();
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