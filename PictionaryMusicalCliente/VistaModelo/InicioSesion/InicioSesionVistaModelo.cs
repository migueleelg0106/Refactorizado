using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.VistaModelo.Salas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.Utilidades.Abstracciones;

namespace PictionaryMusicalCliente.VistaModelo.InicioSesion
{
    public class InicioSesionVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IInicioSesionServicio _inicioSesionServicio;
        private readonly ICambioContrasenaServicio _cambioContrasenaServicio;
        private readonly IRecuperacionCuentaServicio _recuperacionCuentaDialogoServicio;
        private readonly ILocalizacionServicio _localizacionServicio;
        private readonly ISonidoManejador _sonidoManejador;
        private readonly IAvisoServicio _avisoServicio;
        private readonly INombreInvitadoGenerador _generadorNombres;
        private readonly IUsuarioAutenticado _usuarioSesion;
        private readonly Func<ISalasServicio> _salasServicioFactory;

        public const string CampoContrasena = "Contrasena";

        private string _identificador;
        private string _contrasena;
        private bool _estaProcesando;
        private ObservableCollection<IdiomaOpcion> _idiomasDisponibles;
        private IdiomaOpcion _idiomaSeleccionado;
        private bool _sesionIniciada;

        public InicioSesionVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            IInicioSesionServicio inicioSesionServicio,
            ICambioContrasenaServicio cambioContrasenaServicio,
            IRecuperacionCuentaServicio recuperacionCuentaDialogoServicio,
            ILocalizacionServicio localizacionServicio,
            ISonidoManejador sonidoManejador,
            IAvisoServicio avisoServicio,
            INombreInvitadoGenerador generadorNombres,
            IUsuarioAutenticado usuarioSesion,
            Func<ISalasServicio> salasServicioFactory)
            : base(ventana, localizador)
        {
            _inicioSesionServicio = inicioSesionServicio ??
                throw new ArgumentNullException(nameof(inicioSesionServicio));
            _cambioContrasenaServicio = cambioContrasenaServicio ??
                throw new ArgumentNullException(nameof(cambioContrasenaServicio));
            _recuperacionCuentaDialogoServicio = recuperacionCuentaDialogoServicio ??
                throw new ArgumentNullException(nameof(recuperacionCuentaDialogoServicio));
            _localizacionServicio = localizacionServicio ??
                throw new ArgumentNullException(nameof(localizacionServicio));
            _salasServicioFactory = salasServicioFactory ??
                throw new ArgumentNullException(nameof(salasServicioFactory));
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _usuarioSesion = usuarioSesion ??
                throw new ArgumentNullException(nameof(usuarioSesion));
            _generadorNombres = generadorNombres ??
                throw new ArgumentNullException(nameof(generadorNombres));

            IniciarSesionComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await IniciarSesionAsync();
            }, _ => !EstaProcesando);

            RecuperarCuentaComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await RecuperarCuentaAsync();
            }, _ => !EstaProcesando);

            AbrirCrearCuentaComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                AbrirCrearCuenta?.Invoke();
            });

            IniciarSesionInvitadoComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await IniciarSesionInvitadoAsync().ConfigureAwait(true);
            }, _ => !EstaProcesando);

            CargarIdiomas();
        }

        public string Identificador
        {
            get => _identificador;
            set => EstablecerPropiedad(ref _identificador, value);
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
                    _localizacionServicio.EstablecerIdioma(value.Codigo);
                }
            }
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ((IComandoNotificable)IniciarSesionComando).NotificarPuedeEjecutar();
                    ((IComandoNotificable)RecuperarCuentaComando).NotificarPuedeEjecutar();
                    ((IComandoNotificable)IniciarSesionInvitadoComando).NotificarPuedeEjecutar();
                }
            }
        }

        public bool SesionIniciada
        {
            get => _sesionIniciada;
            private set => EstablecerPropiedad(ref _sesionIniciada, value);
        }

        public IComandoAsincrono IniciarSesionComando { get; }
        public IComandoAsincrono RecuperarCuentaComando { get; }
        public ICommand AbrirCrearCuentaComando { get; }
        public IComandoAsincrono IniciarSesionInvitadoComando { get; }

        public Action AbrirCrearCuenta { get; set; }
        public Action<DTOs.ResultadoInicioSesionDTO> InicioSesionCompletado { get; set; }
        public Action CerrarAccion { get; set; }
        public Action<IList<string>> MostrarCamposInvalidos { get; set; }
        public Action<IngresoPartidaInvitadoVistaModelo> MostrarIngresoInvitado { get; set; }
        public Action<DTOs.SalaDTO, ISalasServicio, string> AbrirVentanaJuegoInvitado 
            { get; set; }

        public void EstablecerContrasena(string contrasena)
        {
            _contrasena = contrasena;
        }

        private async Task IniciarSesionInvitadoAsync()
        {
            await EjecutarOperacionAsync(() =>
            {
                ISalasServicio salasServicio = CrearServicioSalas();

                if (!ValidarServicioSalas(salasServicio))
                {
                    return Task.CompletedTask;
                }

                var vistaModelo = CrearVistaModeloIngresoInvitado(salasServicio);

                if (!MostrarDialogoIngresoInvitado(vistaModelo))
                {
                    salasServicio.Dispose();
                    return Task.CompletedTask;
                }

                if (vistaModelo.SeUnioSala)
                {
                    UnirseASalaComoInvitado(vistaModelo, salasServicio);
                }
                else
                {
                    salasServicio.Dispose();
                }

                return Task.CompletedTask;
            }, ManejarErrorInicioInvitado);
        }

        private ISalasServicio CrearServicioSalas()
        {
            return _salasServicioFactory?.Invoke();
        }

        private bool ValidarServicioSalas(ISalasServicio servicio)
        {
            if (servicio == null)
            {
                _logger.Error("La fabrica de servicios devolvio un servicio de salas nulo.");
                MostrarErrorInvitadoGenerico();
                return false;
            }

            return true;
        }

        private IngresoPartidaInvitadoVistaModelo CrearVistaModeloIngresoInvitado(
            ISalasServicio salasServicio)
        {
            return new IngresoPartidaInvitadoVistaModelo(
                _ventana,
                _localizador,
                _localizacionServicio,
                salasServicio,
                _avisoServicio,
                _sonidoManejador,
                _generadorNombres);
        }

        private void UnirseASalaComoInvitado(
            IngresoPartidaInvitadoVistaModelo vistaModelo,
            ISalasServicio salasServicio)
        {
            _logger.InfoFormat("Invitado {0} se unio a sala {1}",
                vistaModelo.NombreInvitadoGenerado, vistaModelo.SalaUnida?.Codigo);
            AbrirVentanaJuegoInvitado?.Invoke(
                vistaModelo.SalaUnida,
                salasServicio,
                vistaModelo.NombreInvitadoGenerado);
        }

        private bool MostrarDialogoIngresoInvitado(
            IngresoPartidaInvitadoVistaModelo vistaModelo)
        {
            if (MostrarIngresoInvitado == null)
            {
                _logger.Error("MostrarIngresoInvitado es nulo, no se puede mostrar dialogo.");
                MostrarErrorInvitadoGenerico();
                return false;
            }

            MostrarIngresoInvitado(vistaModelo);
            return true;
        }

        private void ManejarErrorInicioInvitado(Exception ex)
        {
            _logger.Error("Error critico al iniciar flujo de invitado.", ex);
            MostrarErrorInvitadoGenerico();
        }

        private void MostrarErrorInvitadoGenerico()
        {
            _sonidoManejador.ReproducirError();
            _avisoServicio.Mostrar(Lang.errorTextoNoEncuentraPartida);
        }

        private async Task IniciarSesionAsync()
        {
            var camposInvalidos = ValidarCamposInicioSesion();

            if (camposInvalidos.Any())
            {
                _sonidoManejador.ReproducirError();
                _logger.Warn("Intento de inicio de sesion con campos vacios.");
                return;
            }

            EstaProcesando = true;

            await EjecutarOperacionAsync(async () =>
            {
                var solicitud = CrearCredenciales();
                _logger.InfoFormat("Intentando iniciar sesion para: {0}", solicitud.Identificador);

                DTOs.ResultadoInicioSesionDTO resultado = await EnviarCredencialesAlServidorAsync(
                    solicitud);

                ProcesarResultadoInicioSesion(resultado);
            });

            EstaProcesando = false;
        }

        private List<string> ValidarCamposInicioSesion()
        {
            string identificador = Identificador?.Trim();
            bool identificadorIngresado = !string.IsNullOrWhiteSpace(identificador);
            bool contrasenaIngresada = !string.IsNullOrWhiteSpace(_contrasena);

            LimpiarErroresVisuales();

            var camposInvalidos = new List<string>();

            if (!identificadorIngresado)
            {
                camposInvalidos.Add(nameof(Identificador));
            }

            if (!contrasenaIngresada)
            {
                camposInvalidos.Add(CampoContrasena);
            }

            if (camposInvalidos.Any())
            {
                MarcarCamposInvalidosEnUI(camposInvalidos);
                _avisoServicio.Mostrar(Lang.errorTextoCamposInvalidosGenerico);
            }

            return camposInvalidos;
        }

        private void LimpiarErroresVisuales()
        {
            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());
        }

        private void MarcarCamposInvalidosEnUI(List<string> campos)
        {
            MostrarCamposInvalidos?.Invoke(campos);
        }

        private DTOs.CredencialesInicioSesionDTO CrearCredenciales()
        {
            return new DTOs.CredencialesInicioSesionDTO
            {
                Identificador = Identificador?.Trim(),
                Contrasena = _contrasena
            };
        }

        private async Task<DTOs.ResultadoInicioSesionDTO> EnviarCredencialesAlServidorAsync(
            DTOs.CredencialesInicioSesionDTO solicitud)
        {
            return await _inicioSesionServicio.IniciarSesionAsync(solicitud)
                .ConfigureAwait(true);
        }

        private void ProcesarResultadoInicioSesion(DTOs.ResultadoInicioSesionDTO resultado)
        {
            if (resultado == null)
            {
                _logger.Error("El servicio de inicio de sesion retorno null.");
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.errorTextoServidorInicioSesion);
                return;
            }

            if (!resultado.InicioSesionExitoso)
            {
                _logger.WarnFormat("Inicio de sesion fallido. Mensaje servidor: {0}",
                    resultado.Mensaje);
                _sonidoManejador.ReproducirError();
                MostrarErrorInicioSesion(resultado);
                return;
            }

            ValidarYCargarUsuarioAutenticado(resultado);
            _sonidoManejador.ReproducirNotificacion();
            SesionIniciada = true;
            InicioSesionCompletado?.Invoke(resultado);
            CerrarAccion?.Invoke();
        }

        private void ValidarYCargarUsuarioAutenticado(DTOs.ResultadoInicioSesionDTO resultado)
        {
            if (resultado.Usuario != null)
            {
                _logger.InfoFormat("Sesion establecida exitosamente para ID: {0}",
                    resultado.Usuario.UsuarioId);
                _usuarioSesion.CargarDesdeDTO(resultado.Usuario);
            }
        }

        private void MostrarErrorInicioSesion(DTOs.ResultadoInicioSesionDTO resultado)
        {
            string mensaje = resultado?.Mensaje;

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = Lang.errorTextoCredencialesIncorrectas;
            }

            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                _avisoServicio.Mostrar(mensaje);
            }
        }

        private async Task RecuperarCuentaAsync()
        {
            if (!ValidarIdentificadorRecuperacion())
            {
                return;
            }

            EstaProcesando = true;

            await EjecutarOperacionAsync(async () =>
            {
                string identificador = Identificador?.Trim();
                _logger.InfoFormat("Solicitando recuperacion de cuenta para: {0}",
                    identificador);

                DTOs.ResultadoOperacionDTO resultado = await SolicitarRecuperacionCuentaAsync(
                    identificador);

                ProcesarResultadoRecuperacion(resultado);
            });

            EstaProcesando = false;
        }

        private bool ValidarIdentificadorRecuperacion()
        {
            string identificador = Identificador?.Trim();

            if (string.IsNullOrWhiteSpace(identificador))
            {
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.errorTextoIdentificadorRecuperacionRequerido);
                return false;
            }

            return true;
        }

        private async Task<DTOs.ResultadoOperacionDTO> SolicitarRecuperacionCuentaAsync(
            string identificador)
        {
            return await _recuperacionCuentaDialogoServicio.RecuperarCuentaAsync(
                identificador, _cambioContrasenaServicio).ConfigureAwait(true);
        }

        private void ProcesarResultadoRecuperacion(DTOs.ResultadoOperacionDTO resultado)
        {
            if (resultado?.OperacionExitosa == false &&
                !string.IsNullOrWhiteSpace(resultado.Mensaje))
            {
                _logger.WarnFormat("Recuperacion fallida o cancelada: {0}", resultado.Mensaje);
                _sonidoManejador.ReproducirError();

                string mensajeLocalizado = LocalizarMensajeRecuperacion(resultado.Mensaje);
                _avisoServicio.Mostrar(mensajeLocalizado);
            }
        }

        private string LocalizarMensajeRecuperacion(string mensaje)
        {
            return _localizador.Localizar(mensaje, Lang.errorTextoCuentaNoRegistrada);
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
                .FirstOrDefault(i => string.Equals(i.Codigo, culturaActual,
                StringComparison.OrdinalIgnoreCase))
                ?? IdiomasDisponibles.FirstOrDefault();
        }
    }
}