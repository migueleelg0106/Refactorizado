using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class VerificarCodigoVistaModelo : BaseVistaModelo
    {
        private const int SegundosEsperaReenvio = 30;

        private readonly ICodigoVerificacionService _codigoVerificacionService;
        private readonly string _tokenCodigo;
        private readonly DispatcherTimer _temporizadorReenvio;

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

            IniciarTemporizadorReenvio();
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

        private async Task VerificarCodigoAsync()
        {
            if (string.IsNullOrWhiteSpace(CodigoVerificacion))
            {
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
                    AvisoHelper.Mostrar(Lang.errorTextoVerificarCodigo);
                    return;
                }

                if (!resultado.RegistroExitoso)
                {
                    AvisoHelper.Mostrar(resultado.Mensaje ?? Lang.errorTextoCodigoIncorrectoExpirado);
                    return;
                }

                VerificacionCompletada?.Invoke(resultado);
            }
            catch (ServicioException ex)
            {
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
                    IniciarTemporizadorReenvio();
                    AvisoHelper.Mostrar(Lang.cambiarContrasenaTextoReenviarCodigo);
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
            Cancelado?.Invoke();
        }

        private void IniciarTemporizadorReenvio()
        {
            PuedeReenviar = false;
            _segundosRestantes = SegundosEsperaReenvio;
            ActualizarTextoReenvio();
            _temporizadorReenvio.Start();
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

        private void ActualizarTextoReenvio()
        {
            TextoBotonReenviar = string.Format(
                "{0} ({1})",
                Lang.cambiarContrasenaTextoReenviarCodigo,
                _segundosRestantes);
        }
    }
}
