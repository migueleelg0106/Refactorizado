using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Helpers;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    public class InicioSesionServicio : IInicioSesionServicio
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
                var resultadoDto = await WcfClienteAyudante
                    .UsarAsync(cliente, c => c.IniciarSesionAsync(solicitud))
                    .ConfigureAwait(false);

                if (resultadoDto == null)
                    return null;

                UsuarioMapeador.ActualizarSesion(resultadoDto.Usuario);
                    resultadoDto.Mensaje = MensajeServidorAyudante.Localizar(resultadoDto.Mensaje, resultadoDto.Mensaje);

                // Si hay un usuario válido, lo guardamos en el singleton
                if (resultadoDto.Usuario != null)
                    UsuarioAutenticado.Instancia.CargarDesdeDTO(resultadoDto.Usuario);

                return resultadoDto;
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoServidorInicioSesion);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (TimeoutException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.TiempoAgotado, Lang.errorTextoServidorTiempoAgotado, ex);
            }
            catch (CommunicationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.Comunicacion, Lang.errorTextoServidorNoDisponible, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ExcepcionServicio(TipoErrorServicio.OperacionInvalida, Lang.errorTextoErrorProcesarSolicitud, ex);
            }
        }
    }
}
