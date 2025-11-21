using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Perfil
{
    /// <summary>
    /// Gestiona la logica de validacion de codigos de verificacion con temporizadores.
    /// </summary>
    public class VerificacionCodigoVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int SegundosEsperaReenvio = 30;
        private static readonly TimeSpan TiempoExpiracionCodigo = TimeSpan.FromMinutes(5);

        private readonly ICodigoVerificacionServicio _codigoVerificacionServicio;
        private string _tokenCodigo;
        private readonly DispatcherTimer _temporizadorReenvio;
        private readonly DispatcherTimer _temporizadorExpiracion;

        private string _codigoVerificacion;
        private bool _estaVerificando;
        private bool _puedeReenviar;
        private string _textoBotonReenviar;
        private int _segundosRestantes;

        /// <summary>
        /// Inicializa el ViewModel y arranca los temporizadores de seguridad.
        /// </summary>
        /// <param name="descripcion">Mensaje a mostrar al usuario sobre qué código ingresar.
        /// </param>
        /// <param name="tokenCodigo">Token de sesión asociado al código enviado.</param>
        /// <param name="codigoVerificacionServicio">Servicio para validar el código.</param>
        public VerificacionCodigoVistaModelo(
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionServicio codigoVerificacionServicio)
        {
            Descripcion = descripcion ?? throw new ArgumentNullException(nameof(descripcion));
            _tokenCodigo = tokenCodigo ?? throw new ArgumentNullException(nameof(tokenCodigo));
            _codigoVerificacionServicio = codigoVerificacionServicio ??
                throw new ArgumentNullException(nameof(codigoVerificacionServicio));

            VerificarCodigoComando = new ComandoAsincrono(async _ =>
            {
                SonidoManejador.ReproducirClick();
                await VerificarCodigoAsync();
            });

            ReenviarCodigoComando = new ComandoAsincrono(async _ =>
            {
                SonidoManejador.ReproducirClick();
                await ReenviarCodigoAsync();
            }, _ => PuedeReenviar);

            CancelarComando = new ComandoDelegado(_ =>
            {
                SonidoManejador.ReproducirClick();
                Cancelar();
            });

            _temporizadorReenvio = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _temporizadorReenvio.Tick += TemporizadorReenvioTick;

            _temporizadorExpiracion = new DispatcherTimer
            {
                Interval = TiempoExpiracionCodigo
            };
            _temporizadorExpiracion.Tick += TemporizadorExpiracionTick;

            IniciarTemporizadorReenvio();
            IniciarTemporizadorExpiracion();
        }

        /// <summary>
        /// Texto descriptivo para orientar al usuario.
        /// </summary>
        public string Descripcion { get; }

        /// <summary>
        /// Código numérico ingresado por el usuario.
        /// </summary>
        public string CodigoVerificacion
        {
            get => _codigoVerificacion;
            set => EstablecerPropiedad(ref _codigoVerificacion, value);
        }

        /// <summary>
        /// Indica si se está realizando la validación en el servidor.
        /// </summary>
        public bool EstaVerificando
        {
            get => _estaVerificando;
            private set
            {
                if (EstablecerPropiedad(ref _estaVerificando, value))
                {
                    ((IComandoNotificable)ReenviarCodigoComando).NotificarPuedeEjecutar();
                }
            }
        }

        /// <summary>
        /// Indica si ha pasado el tiempo suficiente para solicitar un nuevo código.
        /// </summary>
        public bool PuedeReenviar
        {
            get => _puedeReenviar;
            private set
            {
                if (EstablecerPropiedad(ref _puedeReenviar, value))
                {
                    ((IComandoNotificable)ReenviarCodigoComando).NotificarPuedeEjecutar();
                }
            }
        }

        /// <summary>
        /// Texto dinámico del botón de reenvío que muestra la cuenta regresiva.
        /// </summary>
        public string TextoBotonReenviar
        {
            get => _textoBotonReenviar;
            private set => EstablecerPropiedad(ref _textoBotonReenviar, value);
        }

        /// <summary>
        /// Comando para validar el código ingresado.
        /// </summary>
        public IComandoAsincrono VerificarCodigoComando { get; }

        /// <summary>
        /// Comando para solicitar un nuevo código.
        /// </summary>
        public IComandoAsincrono ReenviarCodigoComando { get; }

        /// <summary>
        /// Comando para cancelar la operación.
        /// </summary>
        public ICommand CancelarComando { get; }

        /// <summary>
        /// Acción ejecutada cuando la verificación es exitosa.
        /// </summary>
        public Action<DTOs.ResultadoRegistroCuentaDTO> VerificacionCompletada { get; set; }

        /// <summary>
        /// Acción ejecutada al cancelar.
        /// </summary>
        public Action Cancelado { get; set; }

        /// <summary>
        /// Acción para indicar visualmente si el código es inválido.
        /// </summary>
        public Action<bool> MarcarCodigoInvalido { get; set; }

        private async Task VerificarCodigoAsync()
        {
            MarcarCodigoInvalido?.Invoke(false);

            if (string.IsNullOrWhiteSpace(CodigoVerificacion))
            {
                SonidoManejador.ReproducirError();
                MarcarCodigoInvalido?.Invoke(true);
                AvisoAyudante.Mostrar(Lang.errorTextoCodigoVerificacionRequerido);
                return;
            }

            EstaVerificando = true;

            try
            {
				_logger.Info("Enviando código de verificación al servidor.");
                DTOs.ResultadoRegistroCuentaDTO resultado = await _codigoVerificacionServicio
                    .ConfirmarCodigoRegistroAsync(
                        _tokenCodigo,
                        CodigoVerificacion).ConfigureAwait(true);

                if (resultado == null)
                {
                    _logger.Error("El servicio de verificación retornó null.");
                    SonidoManejador.ReproducirError();
                    MarcarCodigoInvalido?.Invoke(true);
                    AvisoAyudante.Mostrar(Lang.errorTextoVerificarCodigo);
                    return;
                }

                if (!resultado.RegistroExitoso)
                {
                    _logger.WarnFormat("Verificación fallida: {0}",
                        resultado.Mensaje);
                    SonidoManejador.ReproducirError();
                    string mensajeOriginal = resultado.Mensaje;
                    string mensajeLocalizado = MensajeServidorAyudante.Localizar(
                        mensajeOriginal,
                        Lang.errorTextoCodigoIncorrecto);

                    resultado.Mensaje = mensajeLocalizado;
                    MarcarCodigoInvalido?.Invoke(true);

                    if (string.Equals(
                            mensajeLocalizado,
                            Lang.avisoTextoCodigoExpirado,
                            StringComparison.Ordinal) ||
                        string.Equals(
                            mensajeOriginal,
                            Lang.avisoTextoCodigoExpirado,
                            StringComparison.Ordinal))
                    {
                        _logger.Info("Código expirado detectado durante verificación.");
                        DetenerTemporizadores();
                        VerificacionCompletada?.Invoke(resultado);
                        return;
                    }

                    AvisoAyudante.Mostrar(mensajeLocalizado);
                    return;
                }

                _logger.Info("Código verificado correctamente.");
                SonidoManejador.ReproducirExito();
                MarcarCodigoInvalido?.Invoke(false);
                DetenerTemporizadores();
                VerificacionCompletada?.Invoke(resultado);
            }
            catch (ServicioExcepcion ex)
            {
                _logger.Error("Error de servicio durante la verificación del código.", ex);
                SonidoManejador.ReproducirError();
                MarcarCodigoInvalido?.Invoke(true);
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoVerificarCodigo);
            }
            finally
            {
                EstaVerificando = false;
            }
        }

        private async Task ReenviarCodigoAsync()
        {
            if (!PuedeReenviar)
            {
                return;
            }

            try
            {
                _logger.Info("Solicitando reenvío de código de verificación.");
                DTOs.ResultadoSolicitudCodigoDTO resultado = await _codigoVerificacionServicio
                    .ReenviarCodigoRegistroAsync(_tokenCodigo).ConfigureAwait(true);

                if (resultado?.CodigoEnviado == true)
                {
                    _logger.Info("Código reenviado exitosamente.");
                    SonidoManejador.ReproducirExito();
                    if (!string.IsNullOrWhiteSpace(resultado.TokenCodigo))
                    {
                        _tokenCodigo = resultado.TokenCodigo;
                    }
                    IniciarTemporizadorReenvio();
                    IniciarTemporizadorExpiracion();
                }
                else
                {
                    _logger.WarnFormat("Fallo al reenviar código: {0}",
                        resultado?.Mensaje);
                    SonidoManejador.ReproducirError();
                    AvisoAyudante.Mostrar(
                        resultado?.Mensaje ?? Lang.errorTextoSolicitarNuevoCodigo);
                }
            }
            catch (ServicioExcepcion ex)
            {
                _logger.Error("Excepción de servicio al reenviar código.", ex);
                SonidoManejador.ReproducirError();
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoSolicitarNuevoCodigo);
            }
        }

        private void Cancelar()
        {
            _logger.Info("Operación de verificación cancelada por el usuario.");
            DetenerTemporizadores();
            Cancelado?.Invoke();
        }

        private void IniciarTemporizadorReenvio()
        {
            PuedeReenviar = false;
            _segundosRestantes = SegundosEsperaReenvio;
            ActualizarTextoReenvio();
            _temporizadorReenvio.Start();
        }

        private void IniciarTemporizadorExpiracion()
        {
            _temporizadorExpiracion.Stop();
            _temporizadorExpiracion.Start();
        }

        private void TemporizadorReenvioTick(object sender, EventArgs e)
        {
            if (_segundosRestantes <= 0)
            {
                _temporizadorReenvio.Stop();
                PuedeReenviar = true;
                TextoBotonReenviar = Lang.cambiarContrasenaTextoReenviarCodigo;
                return;
            }

            _segundosRestantes--;
            ActualizarTextoReenvio();
        }

        private void TemporizadorExpiracionTick(object sender, EventArgs e)
        {
            _logger.Info("El tiempo de validez del código ha expirado.");
            _temporizadorExpiracion.Stop();
            AvisoAyudante.Mostrar(Lang.avisoTextoCodigoExpirado);
            DetenerTemporizadores();
            Cancelado?.Invoke();
        }

        private void ActualizarTextoReenvio()
        {
            TextoBotonReenviar = string.Format(
                "{0} ({1})",
                Lang.cambiarContrasenaTextoReenviarCodigo,
                _segundosRestantes);
        }

        private void DetenerTemporizadores()
        {
            _temporizadorReenvio.Stop();
            _temporizadorExpiracion.Stop();
        }
    }
}