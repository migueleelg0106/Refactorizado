using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
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
        private readonly IWcfClienteEjecutor _ejecutor;
        private readonly IWcfClienteFabrica _fabricaClientes;
        private readonly IManejadorErrorServicio _manejadorError;

        /// <summary>
        /// Inicializa el servicio de perfil con las dependencias requeridas.
        /// </summary>
        public PerfilServicio(
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
        /// Obtiene la informacion del perfil de un usuario por su ID.
        /// </summary>
        public async Task<DTOs.UsuarioDTO> ObtenerPerfilAsync(int usuarioId)
        {
            try
            {
                DTOs.UsuarioDTO perfil = await _ejecutor.EjecutarAsincronoAsync(
                    _fabricaClientes.CrearClientePerfil(),
                    cliente => cliente.ObtenerPerfilAsync(usuarioId)
                ).ConfigureAwait(false);

                _logger.InfoFormat("Perfil obtenido para ID: {0}", usuarioId);
                return perfil;
            }
            catch (FaultException excepcion)
            {
                _logger.WarnFormat("Fallo al obtener perfil para ID {0}.", usuarioId);
                string mensaje = _manejadorError.ObtenerMensaje(
                    excepcion,
                    Lang.errorTextoServidorObtenerPerfil);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, excepcion);
            }
            catch (CommunicationException excepcion)
            {
                _logger.Error("Error de comunicacion al obtener perfil.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.avisoTextoComunicacionServidorSesion,
                    excepcion);
            }
            catch (TimeoutException excepcion)
            {
                _logger.Error("Timeout al obtener perfil.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.avisoTextoServidorTiempoSesion,
                    excepcion);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado en perfil.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoPerfilActualizarInformacion,
                    excepcion);
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

            try
            {
                DTOs.ResultadoOperacionDTO resultado = await _ejecutor.EjecutarAsincronoAsync(
                    _fabricaClientes.CrearClientePerfil(),
                    cliente => cliente.ActualizarPerfilAsync(solicitud)
                ).ConfigureAwait(false);

                RegistrarLogActualizacion(resultado, solicitud.UsuarioId);

                return new DTOs.ResultadoOperacionDTO
                {
                    OperacionExitosa = resultado?.OperacionExitosa ?? false,
                    Mensaje = resultado?.Mensaje
                };
            }
            catch (FaultException excepcion)
            {
                _logger.Warn("Error de servidor al actualizar perfil.", excepcion);
                string mensaje = _manejadorError.ObtenerMensaje(
                    excepcion,
                    Lang.errorTextoServidorActualizarPerfil);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, excepcion);
            }
            catch (CommunicationException excepcion)
            {
                _logger.Error("Error de comunicacion al actualizar perfil.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    excepcion);
            }
            catch (TimeoutException excepcion)
            {
                _logger.Error("Timeout al actualizar perfil.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    excepcion);
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error inesperado al actualizar perfil.", excepcion);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    excepcion);
            }
        }

        private static void RegistrarLogActualizacion(
            DTOs.ResultadoOperacionDTO resultado,
            int usuarioId)
        {
            if (resultado?.OperacionExitosa == true)
            {
                _logger.InfoFormat("Perfil actualizado para ID: {0}", usuarioId);
            }
            else
            {
                _logger.WarnFormat("Fallo actualizacion perfil: {0}", resultado?.Mensaje);
            }
        }
    }
}