using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    public class InicioSesionService : IInicioSesionService
    {
        private const string Endpoint = "BasicHttpBinding_IInicioSesionManejador";

        public async Task<DTOs.ResultadoInicioSesionDTO> IniciarSesionAsync(DTOs.CredencialesInicioSesionDTO solicitud)
        {
            if (solicitud == null)
                throw new ArgumentNullException(nameof(solicitud));

            var cliente = new PictionaryServidorServicioInicioSesion.InicioSesionManejadorClient(Endpoint);

            try
            {
                // Llamamos al servicio remoto
                var resultadoDto = await WcfClientHelper
                    .UsarAsync(cliente, c => c.IniciarSesionAsync(solicitud))
                    .ConfigureAwait(false);

                if (resultadoDto == null)
                    return null;

                // Localización del mensaje (si aplica)
                if (!string.IsNullOrWhiteSpace(resultadoDto.Mensaje))
                    resultadoDto.Mensaje = MensajeServidorHelper.Localizar(resultadoDto.Mensaje, resultadoDto.Mensaje);

                // Si hay un usuario válido, lo guardamos en el singleton
                if (resultadoDto.Usuario != null)
                    UsuarioAutenticado.Instancia.CargarDesdeDTO(resultadoDto.Usuario);

                return resultadoDto;
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorInicioSesion);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }
    }
}
