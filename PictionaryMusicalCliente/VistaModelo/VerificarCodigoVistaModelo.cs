using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class VerificarCodigoVistaModelo : BaseVistaModelo
    {
        private const int SegundosEsperaReenvio = 30;
        private static readonly TimeSpan TiempoExpiracionCodigo = TimeSpan.FromMinutes(5);

        private readonly ICodigoVerificacionService _codigoVerificacionService;
        private string _tokenCodigo;
        private readonly DispatcherTimer _temporizadorReenvio;
        private readonly DispatcherTimer _temporizadorExpiracion;

        private string _codigoVerificacion;
        private bool _estaVerificando;
        private bool _puedeReenviar;
        private string _textoBotonReenviar;
        private int _segundosRestantes;

        public VerificarCodigoVistaModelo(
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionService codigoVerificacionService)
        {
            Descripcion = descripcion ?? throw new ArgumentNullException(nameof(descripcion));
            _tokenCodigo = tokenCodigo ?? throw new ArgumentNullException(nameof(tokenCodigo));
            _codigoVerificacionService = codigoVerificacionService ?? throw new ArgumentNullException(nameof(codigoVerificacionService));

            VerificarCodigoCommand = new ComandoAsincrono(_ => VerificarCodigoAsync());
            ReenviarCodigoCommand = new ComandoAsincrono(_ => ReenviarCodigoAsync(), _ => PuedeReenviar);
            CancelarCommand = new ComandoDelegado(Cancelar);

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
                    ((IComandoNotificable)ReenviarCodigoCommand).NotificarPuedeEjecutar();
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
                    ((IComandoNotificable)ReenviarCodigoCommand).NotificarPuedeEjecutar();
                }
            }
        }

        public string TextoBotonReenviar
        {
            get => _textoBotonReenviar;
            private set => EstablecerPropiedad(ref _textoBotonReenviar, value);
        }

        public IComandoAsincrono VerificarCodigoCommand { get; }

        public IComandoAsincrono ReenviarCodigoCommand { get; }

        public ICommand CancelarCommand { get; }

        public Action<ResultadoRegistroCuenta> VerificacionCompletada { get; set; }

        public Action Cancelado { get; set; }

        public Action<bool> MarcarCodigoInvalido { get; set; }

        private async Task VerificarCodigoAsync()
        {
            MarcarCodigoInvalido?.Invoke(false);

            if (string.IsNullOrWhiteSpace(CodigoVerificacion))
            {
                MarcarCodigoInvalido?.Invoke(true);
                AvisoHelper.Mostrar(Lang.errorTextoCodigoVerificacionRequerido);
                return;
            }

            EstaVerificando = true;

            try
            {
                ResultadoRegistroCuenta resultado = await _codigoVerificacionService
                    .ConfirmarCodigoRegistroAsync(_tokenCodigo, CodigoVerificacion).ConfigureAwait(true);

                if (resultado == null)
                {
                    MarcarCodigoInvalido?.Invoke(true);
                    AvisoHelper.Mostrar(Lang.errorTextoVerificarCodigo);
                    return;
                }

                if (!resultado.RegistroExitoso)
                {
                    string mensaje = MensajeServidorHelper.Localizar(resultado.Mensaje, Lang.errorTextoCodigoIncorrecto);
                    resultado.Mensaje = mensaje;
                    MarcarCodigoInvalido?.Invoke(true);

                    if (string.Equals(mensaje, Lang.avisoTextoCodigoExpirado, StringComparison.Ordinal))
                    {
                        DetenerTemporizadores();
                        VerificacionCompletada?.Invoke(resultado);
                        return;
                    }

                    AvisoHelper.Mostrar(mensaje);
                    return;
                }

                MarcarCodigoInvalido?.Invoke(false);
                DetenerTemporizadores();
                VerificacionCompletada?.Invoke(resultado);
            }
            catch (ServicioException ex)
            {
                MarcarCodigoInvalido?.Invoke(true);
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoVerificarCodigo);
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
                ResultadoSolicitudCodigo resultado = await _codigoVerificacionService
                    .ReenviarCodigoRegistroAsync(_tokenCodigo).ConfigureAwait(true);

                if (resultado?.CodigoEnviado == true)
                {
                    if (!string.IsNullOrWhiteSpace(resultado.TokenCodigo))
                    {
                        _tokenCodigo = resultado.TokenCodigo;
                    }
                    IniciarTemporizadorReenvio();
                    IniciarTemporizadorExpiracion();
                }
                else
                {
                    AvisoHelper.Mostrar(resultado?.Mensaje ?? Lang.errorTextoSolicitarNuevoCodigo);
                }
            }
            catch (ServicioException ex)
            {
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoSolicitarNuevoCodigo);
            }
        }

        private void Cancelar()
        {
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
            _temporizadorExpiracion.Stop();
            AvisoHelper.Mostrar(Lang.avisoTextoCodigoExpirado);
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
