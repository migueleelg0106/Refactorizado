using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.Globalization;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.ClienteServicios.Idiomas;
using PictionaryMusicalServidor.Servicios.Contratos;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    /// <summary>
    /// Maneja el envio de invitaciones a partidas mediante correo electronico.
    /// </summary>
    public class InvitacionesServicio : IInvitacionesServicio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(InvitacionesServicio));
        private const string Endpoint = "BasicHttpBinding_IInvitacionesManejador";

        /// <summary>
        /// Envia una invitacion por correo para unirse a una sala especifica.
        /// </summary>
        public async Task<DTOs.ResultadoOperacionDTO> EnviarInvitacionAsync(
            string codigoSala,
            string correoDestino)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                throw new ArgumentException("El código de sala es obligatorio.",
                    nameof(codigoSala));
            }

            if (string.IsNullOrWhiteSpace(correoDestino))
            {
                throw new ArgumentException("El correo de destino es obligatorio.",
                    nameof(correoDestino));
            }

            ChannelFactory<IInvitacionesManejador> fabrica = null;
            IInvitacionesManejador canal = null;

            try
            {
                fabrica = new ChannelFactory<IInvitacionesManejador>(
                    Endpoint);
                canal = fabrica.CreateChannel();

                var solicitud = new DTOs.InvitacionSalaDTO
                {
                    CodigoSala = codigoSala.Trim(),
                    Correo = correoDestino.Trim(),
                    Idioma = ObtenerCodigoIdiomaActual()
                };

                var resultado = await canal.EnviarInvitacionAsync(solicitud).ConfigureAwait(false);

                _logger.InfoFormat("Invitación enviada a '{0}' para sala '{1}'.", 
                    correoDestino, codigoSala);
                return resultado;
            }
            catch (FaultException ex)
            {
                _logger.Warn("El servidor rechazó la invitación (Validación/Lógica).", ex);
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    Lang.errorTextoEnviarCorreo);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                _logger.Error("Endpoint de invitaciones no encontrado.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error("Timeout al enviar invitación.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error("Error de comunicación al enviar invitación.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida en servicio de invitaciones.", ex);
                throw new ServicioExcepcion(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
            finally
            {
                CerrarCanal(canal);
                CerrarFabrica(fabrica);
            }
        }

        private static void CerrarCanal(
            IInvitacionesManejador canal)
        {
            if (canal is ICommunicationObject comunicacion)
            {
                try
                {
                    if (comunicacion.State == CommunicationState.Opened)
                    {
                        comunicacion.Close();
                    }
                    else
                    {
                        comunicacion.Abort();
                    }
                }
                catch (CommunicationException ex)
                {
                    _logger.Warn("Error al cerrar canal de invitaciones.", ex);
                    comunicacion.Abort();
                }
                catch (TimeoutException ex)
                {
                    _logger.Warn("Timeout al cerrar canal de invitaciones.", ex);
                    comunicacion.Abort();
                }
                catch (Exception ex)
                {
                    _logger.Error("Error inesperado al cerrar canal de invitaciones.", ex);
                    comunicacion.Abort();
                }
            }
        }

        private static void CerrarFabrica(
            ChannelFactory<IInvitacionesManejador> fabrica)
        {
            if (fabrica == null)
            {
                return;
            }

            try
            {
                if (fabrica.State == CommunicationState.Opened)
                {
                    fabrica.Close();
                }
                else
                {
                    fabrica.Abort();
                }
            }
            catch (CommunicationException ex)
            {
                _logger.Warn("Error al cerrar fábrica de canales.", ex);
                fabrica.Abort();
            }
            catch (TimeoutException ex)
            {
                _logger.Warn("Timeout al cerrar fábrica de canales.", ex);
                fabrica.Abort();
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al cerrar fábrica.", ex);
                fabrica.Abort();
            }
        }

        private static string ObtenerCodigoIdiomaActual()
        {
            return LocalizacionServicio.Instancia.CulturaActual?.Name
                ?? CultureInfo.CurrentUICulture?.Name;
        }
    }
}