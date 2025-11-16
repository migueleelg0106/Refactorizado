using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    public class InvitacionesServicio : IInvitacionesServicio
    {
        private const string Endpoint = "BasicHttpBinding_IInvitacionesManejador";

        public async Task<DTOs.ResultadoOperacionDTO> EnviarInvitacionAsync(string codigoSala, string correoDestino)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                throw new ArgumentException("El código de sala es obligatorio.", nameof(codigoSala));
            }

            if (string.IsNullOrWhiteSpace(correoDestino))
            {
                throw new ArgumentException("El correo de destino es obligatorio.", nameof(correoDestino));
            }

            ChannelFactory<PictionaryMusicalServidor.Servicios.Contratos.IInvitacionesManejador> fabrica = null;
            PictionaryMusicalServidor.Servicios.Contratos.IInvitacionesManejador canal = null;

            try
            {
                fabrica = new ChannelFactory<PictionaryMusicalServidor.Servicios.Contratos.IInvitacionesManejador>(Endpoint);
                canal = fabrica.CreateChannel();

                var solicitud = new DTOs.InvitacionSalaDTO
                {
                    CodigoSala = codigoSala.Trim(),
                    Correo = correoDestino.Trim()
                };

                return await Task.Run(() => canal.EnviarInvitacion(solicitud)).ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoEnviarCorreo);
                throw new ServicioExcepcion(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioExcepcion(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioExcepcion(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioExcepcion(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioExcepcion(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
            finally
            {
                CerrarCanal(canal);
                CerrarFabrica(fabrica);
            }
        }

        private static void CerrarCanal(PictionaryMusicalServidor.Servicios.Contratos.IInvitacionesManejador canal)
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
                catch
                {
                    comunicacion.Abort();
                }
            }
        }

        private static void CerrarFabrica(ChannelFactory<PictionaryMusicalServidor.Servicios.Contratos.IInvitacionesManejador> fabrica)
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
            catch
            {
                fabrica.Abort();
            }
        }
    }
}
