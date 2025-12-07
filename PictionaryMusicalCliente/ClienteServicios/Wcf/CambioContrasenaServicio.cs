using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Provee servicios para el flujo de recuperacion y cambio de contraseña.
    /// </summary>
    public class CambioContrasenaServicio : ICambioContrasenaServicio
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(CambioContrasenaServicio));

        private readonly IWcfClienteEjecutor _ejecutor;
        private readonly IWcfClienteFabrica _fabricaClientes;
        private readonly IManejadorErrorServicio _manejadorError;
        private readonly ILocalizadorServicio _localizador;

        /// <summary>
        /// Inicializa el servicio con las dependencias necesarias.
        /// </summary>
        public CambioContrasenaServicio(
            IWcfClienteEjecutor ejecutor,
            IWcfClienteFabrica fabricaClientes,
            IManejadorErrorServicio manejadorError,
            ILocalizadorServicio localizador)
        {
            _ejecutor = ejecutor ?? throw new ArgumentNullException(nameof(ejecutor));
            _fabricaClientes = fabricaClientes ??
                throw new ArgumentNullException(nameof(fabricaClientes));
            _manejadorError = manejadorError ??
                throw new ArgumentNullException(nameof(manejadorError));
            _localizador = localizador ?? throw new ArgumentNullException(nameof(localizador));
        }

        /// <summary>
        /// Inicia el proceso de recuperacion solicitando un codigo al identificador dado.
        /// </summary>
        public async Task<DTOs.ResultadoSolicitudRecuperacionDTO> SolicitarCodigoRecuperacionAsync(
            string identificador)
        {
            ValidarTextoObligatorio(
                identificador,
                Lang.errorTextoIdentificadorRecuperacionRequerido,
                nameof(identificador));

            var solicitud = new DTOs.SolicitudRecuperarCuentaDTO
            {
                Identificador = identificador.Trim(),
                Idioma = ObtenerIdiomaActual()
            };

            DTOs.ResultadoSolicitudRecuperacionDTO resultado = await EjecutarOperacionAsync(
                async () =>
                {
                    var cliente = _fabricaClientes.CrearClienteVerificacion();
                    return await _ejecutor.EjecutarAsincronoAsync(
                        cliente,
                        c => c.SolicitarCodigoRecuperacionAsync(solicitud));
                },
                Lang.errorTextoServidorSolicitudCambioContrasena).ConfigureAwait(false);
            
            return MapearResultadoSolicitudRecuperacion(resultado);
        }

        /// <summary>
        /// Solicita el reenvio del codigo de recuperacion en caso de no haberlo recibido.
        /// </summary>
        public async Task<DTOs.ResultadoSolicitudCodigoDTO> ReenviarCodigoRecuperacionAsync(
            string tokenCodigo)
        {
            ValidarTextoObligatorio(
                tokenCodigo,
                Lang.errorTextoTokenCodigoObligatorio,
                nameof(tokenCodigo));

            var solicitud = new DTOs.ReenvioCodigoDTO 
            { 
                TokenCodigo = tokenCodigo.Trim() 
            };

            DTOs.ResultadoSolicitudCodigoDTO resultado = await EjecutarOperacionAsync(
                async () =>
                {
                    var cliente = _fabricaClientes.CrearClienteCambioContrasena();
                    return await _ejecutor.EjecutarAsincronoAsync(
                        cliente,
                        c => c.ReenviarCodigoRecuperacionAsync(solicitud));
                },
                Lang.errorTextoServidorReenviarCodigo).ConfigureAwait(false);

            return MapearResultadoSolicitudCodigo(resultado);
        }

        /// <summary>
        /// Verifica que el codigo ingresado para recuperacion sea valido.
        /// </summary>
        public async Task<DTOs.ResultadoOperacionDTO> ConfirmarCodigoRecuperacionAsync(
            string tokenCodigo,
            string codigoIngresado)
        {
            ValidarConfirmacion(tokenCodigo, codigoIngresado);

            var solicitud = new DTOs.ConfirmacionCodigoDTO
            {
                TokenCodigo = tokenCodigo,
                CodigoIngresado = codigoIngresado.Trim()
            };

            DTOs.ResultadoOperacionDTO resultado = await EjecutarOperacionAsync(
                async () =>
                {
                    var cliente = _fabricaClientes.CrearClienteVerificacion();
                    return await _ejecutor.EjecutarAsincronoAsync(
                        cliente,
                        c => c.ConfirmarCodigoRecuperacionAsync(solicitud));
                },
                Lang.errorTextoServidorValidarCodigo).ConfigureAwait(false);

            return MapearResultadoOperacion(resultado);
        }

        /// <summary>
        /// Establece la nueva contraseña despues de haber verificado el codigo.
        /// </summary>
        public async Task<DTOs.ResultadoOperacionDTO> ActualizarContrasenaAsync(
            string tokenCodigo,
            string nuevaContrasena)
        {
            ValidarActualizacion(tokenCodigo, nuevaContrasena);

            var solicitud = CrearSolicitudCambio(tokenCodigo, nuevaContrasena);

            DTOs.ResultadoOperacionDTO resultado = await EjecutarOperacionAsync(
                async () =>
                {
                    var cliente = _fabricaClientes.CrearClienteCambioContrasena();
                    return await _ejecutor.EjecutarAsincronoAsync(
                        cliente,
                        c => c.ActualizarContrasenaAsync(solicitud));
                },
                Lang.errorTextoServidorActualizarContrasena).ConfigureAwait(false);

            RegistrarActualizacionExitosa(resultado);
            return MapearResultadoOperacion(resultado);
        }

        private async Task<T> EjecutarOperacionAsync<T>(
            Func<Task<T>> operacion,
            string mensajeError)
        {
            try
            {
                return await operacion().ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                _logger.WarnFormat("Falla controlada en cambio de contrasena: {0}", ex);
                string mensaje = _manejadorError.ObtenerMensaje(ex, mensajeError);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicacion WCF.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout WCF.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado en servicio.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }

        private static void ValidarConfirmacion(string token, string codigo)
        {
            ValidarTextoObligatorio(
                token,
                Lang.errorTextoTokenCodigoObligatorio,
                nameof(token));
            ValidarTextoObligatorio(
                codigo,
                Lang.errorTextoCodigoVerificacionRequerido,
                nameof(codigo));
        }

        private static void ValidarActualizacion(string token, string contrasena)
        {
            ValidarTextoObligatorio(
                token,
                Lang.errorTextoTokenCodigoObligatorio,
                nameof(token));

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                throw new ArgumentNullException(nameof(contrasena));
            }
        }

        private static DTOs.ActualizacionContrasenaDTO CrearSolicitudCambio(
            string token,
            string contrasena)
        {
            return new DTOs.ActualizacionContrasenaDTO
            {
                TokenCodigo = token,
                NuevaContrasena = contrasena
            };
        }

        private string ObtenerIdiomaActual()
        {
            return _localizador.Localizar(null, null) ?? "es-MX";
        }

        private static DTOs.ResultadoSolicitudRecuperacionDTO MapearResultadoSolicitudRecuperacion(
            DTOs.ResultadoSolicitudRecuperacionDTO dto)
        {
            if (dto == null) return null;
            return new DTOs.ResultadoSolicitudRecuperacionDTO
            {
                CuentaEncontrada = dto.CuentaEncontrada,
                CodigoEnviado = dto.CodigoEnviado,
                CorreoDestino = dto.CorreoDestino,
                Mensaje = dto.Mensaje,
                TokenCodigo = dto.TokenCodigo
            };
        }

        private static DTOs.ResultadoSolicitudCodigoDTO MapearResultadoSolicitudCodigo(
            DTOs.ResultadoSolicitudCodigoDTO dto)
        {
            if (dto == null) return null;
            return new DTOs.ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = dto.CodigoEnviado,
                Mensaje = dto.Mensaje,
                TokenCodigo = dto.TokenCodigo
            };
        }

        private static DTOs.ResultadoOperacionDTO MapearResultadoOperacion(
            DTOs.ResultadoOperacionDTO dto)
        {
            if (dto == null) return null;
            return new DTOs.ResultadoOperacionDTO
            {
                OperacionExitosa = dto.OperacionExitosa,
                Mensaje = dto.Mensaje
            };
        }

        private void RegistrarActualizacionExitosa(DTOs.ResultadoOperacionDTO resultado)
        {
            if (resultado?.OperacionExitosa == true)
            {
                _logger.Info("Contrasena actualizada mediante recuperacion.");
            }
        }

        private static void ValidarTextoObligatorio(
            string valor,
            string mensajeError,
            string nombreParametro)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ArgumentException(mensajeError, nombreParametro);
            }
        }
    }
}