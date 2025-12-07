using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.Utilidades.Abstracciones;

namespace PictionaryMusicalCliente.VistaModelo.Perfil
{
    /// <summary>
    /// Administra la logica para el cambio de contrasena mediante un token de recuperacion.
    /// </summary>
    public class CambioContrasenaVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _tokenCodigo;
        private readonly ICambioContrasenaServicio _cambioContrasenaServicio;
        private readonly ISonidoManejador _sonidoManejador;
        private readonly IAvisoServicio _avisoServicio;
        private readonly IValidadorEntrada _validadorEntrada;

        private string _nuevaContrasena;
        private string _confirmacionContrasena;
        private bool _estaProcesando;

        /// <summary>
        /// Inicializa el ViewModel con el token y el servicio necesario para realizar el cambio.
        /// </summary>
        /// <param name="tokenCodigo">El codigo de verificacion validado previamente.</param>
        /// <param name="cambioContrasenaServicio">Servicio para ejecutar la actualizacion.</param>
        public CambioContrasenaVistaModelo(
            string tokenCodigo,
            ICambioContrasenaServicio cambioContrasenaServicio,
            IAvisoServicio avisoServicio,
            IValidadorEntrada validadorEntrada,
            ISonidoManejador sonidoManejador)
        {
            _tokenCodigo = tokenCodigo ?? throw new ArgumentNullException(nameof(tokenCodigo));
            _cambioContrasenaServicio = cambioContrasenaServicio ??
                throw new ArgumentNullException(nameof(cambioContrasenaServicio));
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _validadorEntrada = validadorEntrada ??
                throw new ArgumentNullException(nameof(validadorEntrada));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));

            ConfirmarComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await ConfirmarAsync();
            }, _ => !EstaProcesando);

            CancelarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                Cancelar();
            });
        }

        /// <summary>
        /// La nueva contrasena ingresada por el usuario.
        /// </summary>
        public string NuevaContrasena
        {
            get => _nuevaContrasena;
            set => EstablecerPropiedad(ref _nuevaContrasena, value);
        }

        /// <summary>
        /// La confirmacion de la contrasena para asegurar que el usuario no cometio errores.
        /// </summary>
        public string ConfirmacionContrasena
        {
            get => _confirmacionContrasena;
            set => EstablecerPropiedad(ref _confirmacionContrasena, value);
        }

        /// <summary>
        /// Indica si se esta realizando una operacion asincrona para bloquear la interfaz.
        /// </summary>
        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ((IComandoNotificable)ConfirmarComando).NotificarPuedeEjecutar();
                }
            }
        }

        /// <summary>
        /// Comando para ejecutar la validacion y solicitud de cambio de contrasena.
        /// </summary>
        public IComandoAsincrono ConfirmarComando { get; }

        /// <summary>
        /// Comando para cancelar la operacion y cerrar la vista.
        /// </summary>
        public ICommand CancelarComando { get; }

        /// <summary>
        /// Accion invocada cuando el cambio se realiza exitosamente para notificar a la vista.
        /// </summary>
        public Action<DTOs.ResultadoOperacionDTO> CambioContrasenaCompletado { get; set; }

        /// <summary>
        /// Accion invocada cuando el usuario decide cancelar la operacion.
        /// </summary>
        public Action Cancelado { get; set; }

        /// <summary>
        /// Accion para notificar a la vista que campos especificos son invalidos.
        /// </summary>
        public Action<IList<string>> MostrarCamposInvalidos { get; set; }

        private async Task ConfirmarAsync()
        {
            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());

            var camposInvalidos = ValidarEntradas();
            if (camposInvalidos != null && camposInvalidos.Count > 0)
            {
				_logger.Warn("Validación de contraseña fallida en cliente.");
                _sonidoManejador.ReproducirError();
                MostrarCamposInvalidos?.Invoke(camposInvalidos);
                return;
            }

            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());
            EstaProcesando = true;

            try
            {
                DTOs.ResultadoOperacionDTO resultadoCambio = await RealizarCambioContrasenaAsync()
                    .ConfigureAwait(true);

                if (resultadoCambio != null && resultadoCambio.OperacionExitosa)
                {
                    _logger.Info("Contraseña actualizada exitosamente.");
                    CambioContrasenaCompletado?.Invoke(resultadoCambio);
                }
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private List<string> ValidarEntradas()
        {
            var camposInvalidos = new List<string>();

            if (string.IsNullOrWhiteSpace(NuevaContrasena))
            {
                camposInvalidos.Add(nameof(NuevaContrasena));
            }
            if (string.IsNullOrWhiteSpace(ConfirmacionContrasena))
            {
                camposInvalidos.Add(nameof(ConfirmacionContrasena));
            }

            if (camposInvalidos.Count > 0)
            {
                _avisoServicio.Mostrar(Lang.errorTextoConfirmacionContrasenaRequerida);
                return camposInvalidos;
            }

            DTOs.ResultadoOperacionDTO validacion = _validadorEntrada.ValidarContrasena(
                NuevaContrasena);

            if (validacion?.OperacionExitosa != true)
            {
                _avisoServicio.Mostrar(validacion?.Mensaje ?? Lang.errorTextoContrasenaFormato);
                return new List<string> { nameof(NuevaContrasena) };
            }

            if (!string.Equals(NuevaContrasena, ConfirmacionContrasena, StringComparison.Ordinal))
            {
                _avisoServicio.Mostrar(Lang.errorTextoContrasenasNoCoinciden);
                return new List<string>
                {
                    nameof(NuevaContrasena),
                    nameof(ConfirmacionContrasena)
                };
            }

            return camposInvalidos;
        }

        private async Task<DTOs.ResultadoOperacionDTO> RealizarCambioContrasenaAsync()
        {
            try
            {
                DTOs.ResultadoOperacionDTO resultado = await _cambioContrasenaServicio
                    .ActualizarContrasenaAsync(_tokenCodigo, NuevaContrasena).ConfigureAwait(true);

                if (resultado == null)
                {
                    _logger.Error("Servicio de cambio de contraseña devolvió null.");
                    _sonidoManejador.ReproducirError();
                    _avisoServicio.Mostrar(Lang.errorTextoActualizarContrasena);
                    return null;
                }

                string mensaje = resultado.Mensaje ??
                    (resultado.OperacionExitosa
                        ? Lang.avisoTextoContrasenaActualizada
                        : Lang.errorTextoActualizarContrasena);

                if (resultado.OperacionExitosa)
                {
                    _sonidoManejador.ReproducirExito();
                }
                else
                {
                    _logger.WarnFormat("Fallo al actualizar contraseña en servidor: {0}",
                        resultado.Mensaje);
                    _sonidoManejador.ReproducirError();
                }
                _avisoServicio.Mostrar(mensaje);
                resultado.Mensaje = mensaje;

                return resultado;
            }
            catch (ServicioExcepcion ex)
            {
                _logger.Error("Excepción de servicio al actualizar contraseña.", ex);
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(ex.Message ?? Lang.errorTextoActualizarContrasena);
                return null;
            }
        }

        private void Cancelar()
        {
            Cancelado?.Invoke();
        }
    }
}