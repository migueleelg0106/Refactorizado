using System;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Gestiona el envio de reportes de jugadores hacia el servidor.
    /// </summary>
    public class ReportesServicio : IReportesServicio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ReportesServicio));
        private readonly IWcfClienteEjecutor _ejecutor;
        private readonly IWcfClienteFabrica _fabricaClientes;
        private readonly IManejadorErrorServicio _manejadorError;
        private readonly ILocalizadorServicio _localizador;

        /// <summary>
        /// Inicializa el servicio de reportes con sus dependencias.
        /// </summary>
        public ReportesServicio(
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

        /// <inheritdoc />
        public async Task<DTOs.ResultadoOperacionDTO> ReportarJugadorAsync(
            DTOs.ReporteJugadorDTO reporte)
        {
            if (reporte == null)
            {
                throw new ArgumentNullException(nameof(reporte));
            }

            try
            {
                DTOs.ResultadoOperacionDTO resultado = await _ejecutor.EjecutarAsincronoAsync(
                    _fabricaClientes.CrearClienteReportes(),
                    cliente => cliente.ReportarJugadorAsync(reporte)
                ).ConfigureAwait(false);

                ProcesarResultadoReporte(resultado, reporte);
                return resultado;
            }
            catch (FaultException excepcion)
            {
                _logger.Warn("Fallo al reportar jugador (FaultException).", excepcion);
                string mensaje = _manejadorError.ObtenerMensaje(
                    excepcion,
                    Lang.errorTextoReportarJugador);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, excepcion);
            }
            catch (CommunicationException excepcion)
            {
                _logger.Error("Error de comunicacion al enviar reporte de jugador.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.avisoTextoComunicacionServidorSesion,
                    excepcion);
            }
            catch (TimeoutException excepcion)
            {
                _logger.Error("Timeout al enviar reporte de jugador.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.avisoTextoServidorTiempoSesion,
                    excepcion);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Operacion invalida al enviar reporte de jugador.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoReportarJugador,
                    excepcion);
            }
        }

        private void ProcesarResultadoReporte(
            DTOs.ResultadoOperacionDTO resultado,
            DTOs.ReporteJugadorDTO reporte)
        {
            if (resultado != null)
            {
                resultado.Mensaje = _localizador.Localizar(
                    resultado.Mensaje,
                    Lang.errorTextoReportarJugador);
            }

            if (resultado?.OperacionExitosa == true)
            {
                _logger.InfoFormat(
                    "Reporte enviado por {0} contra {1}.",
                    reporte.NombreUsuarioReportante,
                    reporte.NombreUsuarioReportado);
            }
            else
            {
                _logger.WarnFormat(
                    "No se pudo completar el reporte para {0}. Mensaje: {1}",
                    reporte.NombreUsuarioReportado,
                    resultado?.Mensaje);
            }
        }
    }
}