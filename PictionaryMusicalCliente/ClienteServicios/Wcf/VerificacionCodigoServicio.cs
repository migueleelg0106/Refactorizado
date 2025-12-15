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
                        cliente => cliente.SolicitarCodigoVerificacionAsync(solicitud));
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
                        cliente => cliente.ConfirmarCodigoVerificacionAsync(solicitud));
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
                        cliente => cliente.ReenviarCodigoVerificacionAsync(solicitud));
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
            catch (FaultException excepcion)
            {
                _logger.WarnFormat("Error de logica del servidor: {0}", excepcion);
                string mensaje = _errorServicio.ObtenerMensaje(
                    excepcion,
                    mensajeErrorPredeterminado);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, excepcion);
            }
            catch (EndpointNotFoundException excepcion)
            {
                _logger.Error("Endpoint no encontrado.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    excepcion);
            }
            catch (TimeoutException excepcion)
            {
                _logger.Error("Timeout en servicio.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    excepcion);
            }
            catch (CommunicationException excepcion)
            {
                _logger.Error("Error de comunicacion.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    excepcion);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    excepcion);
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