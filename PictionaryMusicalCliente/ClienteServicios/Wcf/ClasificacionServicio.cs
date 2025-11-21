using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Implementa la comunicacion con el servicio de clasificacion para obtener rankings.
    /// </summary>
    public class ClasificacionServicio : IClasificacionServicio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClasificacionServicio));
        private const string ClasificacionEndpoint = "BasicHttpBinding_IClasificacionManejador";

        /// <summary>
        /// Consulta al servidor el listado de jugadores con mejores puntuaciones.
        /// </summary>
        public async Task<IReadOnlyList<DTOs.ClasificacionUsuarioDTO>> ObtenerTopJugadoresAsync()
        {
            var cliente = new PictionaryServidorServicioClasificacion
                .ClasificacionManejadorClient(ClasificacionEndpoint);

            try
            {
                DTOs.ClasificacionUsuarioDTO[] clasificacion = await WcfClienteAyudante
                    .UsarAsincronoAsync(cliente, c => c.ObtenerTopJugadoresAsync())
                    .ConfigureAwait(false);

                return clasificacion ?? Array.Empty<DTOs.ClasificacionUsuarioDTO>();
            }
            catch (FaultException ex)
            {
                _logger.Warn("Fallo al obtener clasificación desde servidor.", ex);
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    Lang.errorTextoErrorProcesarSolicitud);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.Error("Endpoint de clasificación no encontrado.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al obtener clasificación.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicación al obtener clasificación.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida al obtener clasificación.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }
    }
}