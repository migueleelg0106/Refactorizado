using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.Utilidades;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class VerificacionCodigoVistaModelo : BaseVistaModelo
    {
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

        public VerificacionCodigoVistaModelo(
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionServicio codigoVerificacionServicio)
        {
            Descripcion = descripcion ?? throw new ArgumentNullException(nameof(descripcion));
            _tokenCodigo = tokenCodigo ?? throw new ArgumentNullException(nameof(tokenCodigo));
            _codigoVerificacionServicio = codigoVerificacionServicio ?? throw new ArgumentNullException(nameof(codigoVerificacionServicio));

            VerificarCodigoComando = new ComandoAsincrono(_ => VerificarCodigoAsync());
            ReenviarCodigoComando = new ComandoAsincrono(_ => ReenviarCodigoAsync(), _ => PuedeReenviar);
            CancelarComando = new ComandoDelegado(Cancelar);

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
                    ((IComandoNotificable)ReenviarCodigoComando).NotificarPuedeEjecutar();
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
                    ((IComandoNotificable)ReenviarCodigoComando).NotificarPuedeEjecutar();
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

        public Action<DTOs.ResultadoRegistroCuentaDTO> VerificacionCompletada { get; set; }

        public Action Cancelado { get; set; }

        public Action<bool> MarcarCodigoInvalido { get; set; }

        private async Task VerificarCodigoAsync()
        {
            MarcarCodigoInvalido?.Invoke(false);

            if (string.IsNullOrWhiteSpace(CodigoVerificacion))
            {
                MarcarCodigoInvalido?.Invoke(true);
                AvisoAyudante.Mostrar(Lang.errorTextoCodigoVerificacionRequerido);
                return;
            }

            EstaVerificando = true;

            try
            {
                DTOs.ResultadoRegistroCuentaDTO resultado = await _codigoVerificacionServicio
                    .ConfirmarCodigoRegistroAsync(_tokenCodigo, CodigoVerificacion).ConfigureAwait(true);

                if (resultado == null)
                {
                    MarcarCodigoInvalido?.Invoke(true);
                    AvisoAyudante.Mostrar(Lang.errorTextoVerificarCodigo);
                    return;
                }

                if (!resultado.RegistroExitoso)
                {
                    string mensaje = MensajeServidorAyudante.Localizar(resultado.Mensaje, Lang.errorTextoCodigoIncorrecto);
                    resultado.Mensaje = mensaje;
                    MarcarCodigoInvalido?.Invoke(true);

                    if (string.Equals(mensaje, Lang.avisoTextoCodigoExpirado, StringComparison.Ordinal))
                    {
                        DetenerTemporizadores();
                        VerificacionCompletada?.Invoke(resultado);
                        return;
                    }

                    AvisoAyudante.Mostrar(mensaje);
                    return;
                }

                MarcarCodigoInvalido?.Invoke(false);
                DetenerTemporizadores();
                VerificacionCompletada?.Invoke(resultado);
            }
            catch (ExcepcionServicio ex)
            {
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
                DTOs.ResultadoSolicitudCodigoDTO resultado = await _codigoVerificacionServicio
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
                    AvisoAyudante.Mostrar(resultado?.Mensaje ?? Lang.errorTextoSolicitarNuevoCodigo);
                }
            }
            catch (ExcepcionServicio ex)
            {
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoSolicitarNuevoCodigo);
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
