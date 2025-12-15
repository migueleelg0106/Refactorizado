using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Servicio para manejar la confirmacion y reenvio de codigos de verificacion.
    /// </summary>
    public class VerificacionCodigoServicio : ICodigoVerificacionServicio
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(VerificacionCodigoServicio));

        private readonly IWcfClienteEjecutor _ejecutor;
        private readonly IWcfClienteFabrica _fabricaClientes;
        private readonly ILocalizadorServicio _localizador;
        private readonly IManejadorErrorServicio _errorServicio;

        /// <summary>
        /// Inicializa el servicio con las dependencias necesarias.
        /// </summary>
        /// <param name="ejecutor">Manejador seguro de llamadas WCF.</param>
        /// <param name="fabricaClientes">Fabrica para crear clientes WCF.</param>
        /// <param name="localizador">Servicio para obtener la cultura actual.</param>
        public VerificacionCodigoServicio(
            IWcfClienteEjecutor ejecutor,
            IWcfClienteFabrica fabricaClientes,
            ILocalizadorServicio localizador,
            IManejadorErrorServicio errorServicio)
        {
            _ejecutor = ejecutor ?? throw new ArgumentNullException(nameof(ejecutor));
            _fabricaClientes = fabricaClientes ??
                throw new ArgumentNullException(nameof(fabricaClientes));
            _localizador = localizador ?? throw new ArgumentNullException(nameof(localizador));
            _errorServicio = errorServicio ?? 
                throw new ArgumentNullException(nameof(errorServicio));
        }

        /// <summary>
        /// Solicita un codigo de verificacion para el registro de una nueva cuenta.
        /// </summary>
        public async Task<DTOs.ResultadoSolicitudCodigoDTO> SolicitarCodigoRegistroAsync(
            DTOs.NuevaCuentaDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            solicitud.Idioma ??= ObtenerIdiomaActual();

            DTOs.ResultadoSolicitudCodigoDTO resultado = await EjecutarOperacionAsync(
                async () =>
                {
                    var cliente = _fabricaClientes.CrearClienteVerificacion();
                    return await _ejecutor.EjecutarAsincronoAsync(
                        cliente,
                        c => c.SolicitarCodigoVerificacionAsync(solicitud));
                },
                Lang.errorTextoServidorSolicitudCodigo).ConfigureAwait(false);

            return resultado;
        }

        /// <summary>
        /// Valida el codigo ingresado por el usuario contra el token del servidor.
        /// </summary>
        public async Task<DTOs.ResultadoRegistroCuentaDTO> ConfirmarCodigoRegistroAsync(
            string tokenCodigo,
            string codigoIngresado)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(
                    Lang.errorTextoTokenCodigoObligatorio,
                    nameof(tokenCodigo));
            }

            var solicitud = new DTOs.ConfirmacionCodigoDTO
            {
                TokenCodigo = tokenCodigo,
                CodigoIngresado = codigoIngresado?.Trim()
            };

            DTOs.ResultadoRegistroCuentaDTO resultado = await EjecutarOperacionAsync(
                async () =>
                {
                    var cliente = _fabricaClientes.CrearClienteVerificacion();
                    return await _ejecutor.EjecutarAsincronoAsync(
                        cliente,
                        c => c.ConfirmarCodigoVerificacionAsync(solicitud));
                },
                Lang.errorTextoServidorValidarCodigo).ConfigureAwait(false);

            return resultado;
        }

        /// <summary>
        /// Solicita el reenvio del codigo de verificacion.
        /// </summary>
        public async Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRegistroAsync(
            string tokenCodigo)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(
                    Lang.errorTextoTokenCodigoObligatorio,
                    nameof(tokenCodigo));
            }

            var solicitud = new DTOs.ReenvioCodigoVerificacionDTO
            {
                TokenCodigo = tokenCodigo.Trim()
            };

            DTOs.ResultadoSolicitudCodigoDTO resultado = await EjecutarOperacionAsync(
                async () =>
                {
                    var cliente = _fabricaClientes.CrearClienteCuenta();
                    return await _ejecutor.EjecutarAsincronoAsync(
                        cliente,
                        c => c.ReenviarCodigoVerificacionAsync(solicitud));
                },
                Lang.errorTextoServidorReenviarCodigo).ConfigureAwait(false);

            return resultado;
        }

        private async Task<T> EjecutarOperacionAsync<T>(
            Func<Task<T>> operacion,
            string mensajeErrorPredeterminado)
        {
            try
            {
                return await operacion().ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                _logger.WarnFormat("Error de logica del servidor: {0}", ex);
                string mensaje = _errorServicio.ObtenerMensaje(
                    ex,
                    mensajeErrorPredeterminado);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.Error("Endpoint no encontrado.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout en servicio.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicacion.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }

        private string ObtenerIdiomaActual()
        {
            var culturaActual = Lang.Culture;
            if (culturaActual != null)
            {
                return culturaActual.Name;
            }

            return System.Globalization.CultureInfo.CurrentUICulture.Name;
        }
    }
}