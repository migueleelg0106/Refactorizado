using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

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
                var resultadoDto = await WcfClienteAyudante
                    .UsarAsincronoAsync(cliente, c => c.IniciarSesionAsync(solicitud))
                    .ConfigureAwait(false);

                if (resultadoDto == null)
                    return null;

                UsuarioMapeador.ActualizarSesion(resultadoDto.Usuario);
                    resultadoDto.Mensaje = MensajeServidorAyudante.Localizar(resultadoDto.Mensaje, resultadoDto.Mensaje);

                if (resultadoDto.Usuario != null)
                    UsuarioAutenticado.Instancia.CargarDesdeDTO(resultadoDto.Usuario);

                return resultadoDto;
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(ex, Lang.errorTextoServidorInicioSesion);
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
        }
    }
}
