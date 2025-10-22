using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class CuentaService : ICuentaService
    {
        private const string CuentaEndpoint = "BasicHttpBinding_ICuentaManejador";

        public async Task<DTOs.ResultadoRegistroCuentaDTO> RegistrarCuentaAsync(DTOs.SolicitudRegistroCuentaDTO solicitud)
        {
            if (solicitud == null)
                throw new ArgumentNullException(nameof(solicitud));

            var cliente = new PictionaryServidorServicioCuenta.CuentaManejadorClient(CuentaEndpoint);

            try
            {
                var dto = new DTOs.NuevaCuentaDTO
                {
                    Usuario = solicitud.Usuario,
                    Correo = solicitud.Correo,
                    Nombre = solicitud.Nombre,
                    Apellido = solicitud.Apellido,
                    Contrasena = solicitud.Contrasena,
                    AvatarRutaRelativa = solicitud.AvatarRutaRelativa
                };

                DTOs.ResultadoRegistroCuentaDTO resultado = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.RegistrarCuentaAsync(dto))
                    .ConfigureAwait(false);

                if (resultado == null)
                    return null;

                return new DTOs.ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = resultado.RegistroExitoso,
                    UsuarioYaRegistrado = resultado.UsuarioYaRegistrado,
                    CorreoYaRegistrado = resultado.CorreoYaRegistrado,
                    Mensaje = resultado.Mensaje
                };
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoRegistrarCuentaMasTarde);
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
