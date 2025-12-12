using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Perfil
{
    /// <summary>
    /// Gestiona la logica de validacion de codigos de verificacion con 
    /// temporizadores.
    /// </summary>
    public class VerificacionCodigoVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IAvisoServicio _avisoServicio;
        private readonly ISonidoManejador _sonidoManejador;

        private const int SegundosEsperaReenvio = 30;
        private static readonly TimeSpan TiempoExpiracionCodigo = 
            TimeSpan.FromMinutes(5);

        private readonly ICodigoVerificacionServicio _codigoVerificacionServicio;
        private string _tokenCodigo;
        private readonly DispatcherTimer _temporizadorReenvio;
        private readonly DispatcherTimer _temporizadorExpiracion;

        private string _codigoVerificacion;
        private bool _estaVerificando;
        private bool _puedeReenviar;
        private string _textoBotonReenviar;
        private int _segundosRestantes;

        public VerificacionCodigoVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionServicio codigoVerificacionServicio,
            IAvisoServicio avisoServicio,
            ISonidoManejador sonidoManejador)
            : base(ventana, localizador)
        {
            Descripcion = descripcion ?? 
                throw new ArgumentNullException(nameof(descripcion));
            _tokenCodigo = tokenCodigo ?? 
                throw new ArgumentNullException(nameof(tokenCodigo));
            _codigoVerificacionServicio = codigoVerificacionServicio ??
                throw new ArgumentNullException(
                    nameof(codigoVerificacionServicio));
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));

            VerificarCodigoComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await VerificarCodigoAsync();
            });

            ReenviarCodigoComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await ReenviarCodigoAsync();
            }, _ => PuedeReenviar);

            CancelarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
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

        public string Descripcion { get; }

        public string CodigoVerificacion
        {
            get => _codigoVerificacion;
            set => EstablecerPropiedad(ref _codigoVerificacion, value);
        }

        public bool EstaVerificando
        {
            get => _estaVerificando;
            private set
            {
                if (EstablecerPropiedad(ref _estaVerificando, value))
                {
                    ((IComandoNotificable)ReenviarCodigoComando)
                        .NotificarPuedeEjecutar();
                }
            }
        }

        public bool PuedeReenviar
        {
            get => _puedeReenviar;
            private set
            {
                if (EstablecerPropiedad(ref _puedeReenviar, value))
                {
                    ((IComandoNotificable)ReenviarCodigoComando)
                        .NotificarPuedeEjecutar();
                }
            }
        }

        public string TextoBotonReenviar
        {
            get => _textoBotonReenviar;
            private set => EstablecerPropiedad(ref _textoBotonReenviar, value);
        }

        public IComandoAsincrono VerificarCodigoComando { get; }

        public IComandoAsincrono ReenviarCodigoComando { get; }

        public ICommand CancelarComando { get; }

        public Action<bool> MarcarCodigoInvalido { get; set; }

        public Action<DTOs.ResultadoRegistroCuentaDTO> VerificacionCompletada { get; set; }

        public Action Cancelado { get; set; }

        public DTOs.ResultadoRegistroCuentaDTO ResultadoVerificacion { get; private set; }

        public bool VerificacionExitosa { get; private set; }

        private async Task VerificarCodigoAsync()
        {
            MarcarCodigoInvalido?.Invoke(false);

            if (!ValidarCodigoIngresado())
            {
                return;
            }

            EstaVerificando = true;

            await EjecutarOperacionAsync(async () =>
            {
                DTOs.ResultadoRegistroCuentaDTO resultado = 
                    await ConfirmarCodigoEnServidorAsync();

                await ProcesarResultadoVerificacionAsync(resultado);
            },
            ex =>
            {
                _logger.Error(
                    "Error de servicio durante la verificacion del codigo.", ex);
                _sonidoManejador.ReproducirError();
                MarcarCodigoInvalido?.Invoke(true);
                _avisoServicio.Mostrar(ex.Message ?? 
                    Lang.errorTextoVerificarCodigo);
                EstaVerificando = false;
            });

            EstaVerificando = false;
        }

        private bool ValidarCodigoIngresado()
        {
            if (string.IsNullOrWhiteSpace(CodigoVerificacion))
            {
                _sonidoManejador.ReproducirError();
                MarcarCodigoInvalido?.Invoke(true);
                _avisoServicio.Mostrar(Lang.errorTextoCodigoVerificacionRequerido);
                return false;
            }

            return true;
        }

        private async Task<DTOs.ResultadoRegistroCuentaDTO> 
            ConfirmarCodigoEnServidorAsync()
        {
            _logger.Info("Enviando codigo de verificacion al servidor.");
            return await _codigoVerificacionServicio
                .ConfirmarCodigoRegistroAsync(
                    _tokenCodigo,
                    CodigoVerificacion).ConfigureAwait(true);
        }

        private async Task ProcesarResultadoVerificacionAsync(
            DTOs.ResultadoRegistroCuentaDTO resultado)
        {
            if (resultado == null)
            {
                ManejarResultadoNulo();
                return;
            }

            if (!resultado.RegistroExitoso)
            {
                await ManejarVerificacionFallidaAsync(resultado);
                return;
            }

            ManejarVerificacionExitosa(resultado);
        }

        private void ManejarResultadoNulo()
        {
            _logger.Error("El servicio de verificacion retorno null.");
            _sonidoManejador.ReproducirError();
            MarcarCodigoInvalido?.Invoke(true);
            _avisoServicio.Mostrar(Lang.errorTextoVerificarCodigo);
        }

        private async Task ManejarVerificacionFallidaAsync(
            DTOs.ResultadoRegistroCuentaDTO resultado)
        {
            _logger.WarnFormat("Verificacion fallida: {0}", resultado.Mensaje);
            _sonidoManejador.ReproducirError();
            
            string mensajeLocalizado = LocalizarMensajeError(resultado.Mensaje);
            resultado.Mensaje = mensajeLocalizado;
            MarcarCodigoInvalido?.Invoke(true);

            if (EsCodigoExpirado(mensajeLocalizado, resultado.Mensaje))
            {
                await ManejarCodigoExpiradoAsync(resultado);
                return;
            }

            _avisoServicio.Mostrar(mensajeLocalizado);
        }

        private string LocalizarMensajeError(string mensajeOriginal)
        {
            return _localizador.Localizar(
                mensajeOriginal,
                Lang.errorTextoCodigoIncorrecto);
        }

        private bool EsCodigoExpirado(string mensajeLocalizado, string mensajeOriginal)
        {
            return string.Equals(
                    mensajeLocalizado,
                    Lang.avisoTextoCodigoExpirado,
                    StringComparison.Ordinal) ||
                string.Equals(
                    mensajeOriginal,
                    Lang.avisoTextoCodigoExpirado,
                    StringComparison.Ordinal);
        }

        private Task ManejarCodigoExpiradoAsync(
            DTOs.ResultadoRegistroCuentaDTO resultado)
        {
            _logger.Info("Codigo expirado detectado durante verificacion.");
            DetenerTemporizadores();
            FinalizarConResultado(resultado, false);
            return Task.CompletedTask;
        }

        private void ManejarVerificacionExitosa(
            DTOs.ResultadoRegistroCuentaDTO resultado)
        {
            _logger.Info("Codigo verificado correctamente.");
            _sonidoManejador.ReproducirNotificacion();
            MarcarCodigoInvalido?.Invoke(false);
            DetenerTemporizadores();
            FinalizarConResultado(resultado, true);
        }

        private void FinalizarConResultado(
            DTOs.ResultadoRegistroCuentaDTO resultado,
            bool exitoso)
        {
            ResultadoVerificacion = resultado;
            VerificacionExitosa = exitoso;

            if (exitoso)
            {
                VerificacionCompletada?.Invoke(resultado);
            }

            _ventana.CerrarVentana(this);
        }

        private async Task ReenviarCodigoAsync()
        {
            if (!PuedeReenviar)
            {
                return;
            }

            await EjecutarOperacionAsync(async () =>
            {
                _logger.Info("Solicitando reenvio de codigo de verificacion.");
                DTOs.ResultadoSolicitudCodigoDTO resultado = 
                    await _codigoVerificacionServicio
                        .ReenviarCodigoRegistroAsync(_tokenCodigo)
                        .ConfigureAwait(true);

                ProcesarResultadoReenvio(resultado);
            },
            ex =>
            {
                _logger.Error("Excepcion de servicio al reenviar codigo.", ex);
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(ex.Message ?? 
                    Lang.errorTextoSolicitarNuevoCodigo);
            });
        }

        private void ProcesarResultadoReenvio(
            DTOs.ResultadoSolicitudCodigoDTO resultado)
        {
            if (resultado?.CodigoEnviado == true)
            {
                ManejarReenvioExitoso(resultado);
            }
            else
            {
                ManejarReenvioFallido(resultado);
            }
        }

        private void ManejarReenvioExitoso(
            DTOs.ResultadoSolicitudCodigoDTO resultado)
        {
            _logger.Info("Codigo reenviado exitosamente.");
            _sonidoManejador.ReproducirNotificacion();
            
            if (!string.IsNullOrWhiteSpace(resultado.TokenCodigo))
            {
                _tokenCodigo = resultado.TokenCodigo;
            }
            
            IniciarTemporizadorReenvio();
            IniciarTemporizadorExpiracion();
        }

        private void ManejarReenvioFallido(
            DTOs.ResultadoSolicitudCodigoDTO resultado)
        {
            _logger.WarnFormat("Fallo al reenviar codigo: {0}", 
                resultado?.Mensaje);
            _sonidoManejador.ReproducirError();
            _avisoServicio.Mostrar(
                resultado?.Mensaje ?? Lang.errorTextoSolicitarNuevoCodigo);
        }

        private void Cancelar()
        {
            _logger.Info("Operacion de verificacion cancelada por el usuario.");
            DetenerTemporizadores();
            Cancelado?.Invoke(); 
            _ventana.CerrarVentana(this);
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
            _logger.Info("El tiempo de validez del codigo ha expirado.");
            _temporizadorExpiracion.Stop();
            _avisoServicio.Mostrar(Lang.avisoTextoCodigoExpirado);
            DetenerTemporizadores();
            _ventana.CerrarVentana(this);
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