using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Administra las operaciones relacionadas con el perfil del usuario.
    /// </summary>
    public class PerfilServicio : IPerfilServicio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PerfilServicio));
        private const string PerfilEndpoint = "BasicHttpBinding_IPerfilManejador";

        /// <summary>
        /// Obtiene la informacion del perfil de un usuario por su ID.
        /// </summary>
        public async Task<DTOs.UsuarioDTO> ObtenerPerfilAsync(int usuarioId)
        {
            var cliente = new PictionaryServidorServicioPerfil.PerfilManejadorClient
                (PerfilEndpoint);

            try
            {
                DTOs.UsuarioDTO perfilDto = await WcfClienteAyudante
                    .UsarAsincronoAsync(cliente, c => c.ObtenerPerfilAsync(usuarioId))
                    .ConfigureAwait(false);

                _logger.InfoFormat("Perfil obtenido exitosamente para Usuario ID: {0}",
                    usuarioId);
                return perfilDto;
            }
            catch (FaultException ex)
            {
                _logger.WarnFormat("Fallo al obtener perfil para ID {0}.",
                    usuarioId, ex);
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorObtenerPerfil);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.Error("Endpoint de perfil no encontrado.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.avisoTextoComunicacionServidorSesion,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al obtener perfil.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.avisoTextoServidorTiempoSesion,
                    ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicación al obtener perfil.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.avisoTextoComunicacionServidorSesion,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida al obtener perfil.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoPerfilActualizarInformacion,
                    ex);
            }
        }

        /// <summary>
        /// Actualiza la informacion personal del perfil del usuario.
        /// </summary>
        public async Task<DTOs.ResultadoOperacionDTO> ActualizarPerfilAsync(
            DTOs.ActualizacionPerfilDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            var cliente = new PictionaryServidorServicioPerfil.PerfilManejadorClient
                (PerfilEndpoint);

            try
            {
                DTOs.ResultadoOperacionDTO resultado = await WcfClienteAyudante
                    .UsarAsincronoAsync(cliente, c => c.ActualizarPerfilAsync(solicitud))
                    .ConfigureAwait(false);

                if (resultado != null && resultado.OperacionExitosa)
                {
                    _logger.InfoFormat("Perfil actualizado correctamente para Usuario ID: {0}",
                        solicitud.UsuarioId);
                }
                else
                {
                    _logger.WarnFormat("No se pudo actualizar el perfil. Mensaje: {0}",
                        resultado?.Mensaje);
                }

                return new DTOs.ResultadoOperacionDTO
                {
                    OperacionExitosa = resultado?.OperacionExitosa ?? false,
                    Mensaje = resultado?.Mensaje
                };
            }
            catch (FaultException ex)
            {
                _logger.Warn("Error de servidor al actualizar perfil.", ex);
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorActualizarPerfil);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.Error("Endpoint no encontrado al actualizar perfil.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al actualizar perfil.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicación al actualizar perfil.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida al actualizar perfil.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }
    }
}