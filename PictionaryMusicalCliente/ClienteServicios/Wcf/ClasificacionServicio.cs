using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Implementa la comunicacion con el servicio de clasificacion para obtener rankings.
    /// </summary>
    public class ClasificacionServicio : IClasificacionServicio
    {
        private static readonly ILog _logger = 
            LogManager.GetLogger(typeof(ClasificacionServicio));
        private readonly IWcfClienteEjecutor _ejecutor;
        private readonly IWcfClienteFabrica _fabricaClientes;
        private readonly IManejadorErrorServicio _manejadorError;

        /// <summary>
        /// Inicializa el servicio con las dependencias necesarias.
        /// </summary>
        public ClasificacionServicio(
            IWcfClienteEjecutor ejecutor,
            IWcfClienteFabrica fabricaClientes,
            IManejadorErrorServicio manejadorError)
        {
            _ejecutor = ejecutor ?? throw new ArgumentNullException(nameof(ejecutor));
            _fabricaClientes = fabricaClientes ??
                throw new ArgumentNullException(nameof(fabricaClientes));
            _manejadorError = manejadorError ??
                throw new ArgumentNullException(nameof(manejadorError));
        }

        /// <summary>
        /// Consulta al servidor el listado de jugadores con mejores puntuaciones.
        /// </summary>
        public async Task<IReadOnlyList<DTOs.ClasificacionUsuarioDTO>> ObtenerTopJugadoresAsync()
        {
            try
            {
                DTOs.ClasificacionUsuarioDTO[] clasificacion = 
                    await _ejecutor.EjecutarAsincronoAsync(
                    _fabricaClientes.CrearClienteClasificacion(),
                    c => c.ObtenerTopJugadoresAsync()
                ).ConfigureAwait(false);

                return clasificacion ?? Array.Empty<DTOs.ClasificacionUsuarioDTO>();
            }
            catch (FaultException ex)
            {
                _logger.Warn("Fallo al obtener clasificacion desde servidor.", ex);
                string mensaje = _manejadorError.ObtenerMensaje(
                    ex,
                    Lang.errorTextoErrorProcesarSolicitud);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicacion al obtener clasificacion.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al obtener clasificacion.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado en clasificacion.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }
    }
}