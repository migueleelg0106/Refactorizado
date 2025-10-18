using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using CuentaSrv = PictionaryMusicalCliente.PictionaryServidorServicioCuenta;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class CuentaService : ICuentaService
    {
        private const string CuentaEndpoint = "BasicHttpBinding_ICuentaManejador";

        public async Task<ResultadoRegistroCuenta> RegistrarCuentaAsync(SolicitudRegistroCuenta solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            var cliente = new CuentaSrv.CuentaManejadorClient(CuentaEndpoint);

            try
            {
                var dto = new CuentaSrv.NuevaCuentaDTO
                {
                    Usuario = solicitud.Usuario,
                    Correo = solicitud.Correo,
                    Nombre = solicitud.Nombre,
                    Apellido = solicitud.Apellido,
                    Contrasena = solicitud.Contrasena,
                    AvatarRutaRelativa = solicitud.AvatarRutaRelativa
                };

                CuentaSrv.ResultadoRegistroCuentaDTO resultado = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.RegistrarCuentaAsync(dto)).ConfigureAwait(false);

                if (resultado == null)
                {
                    return null;
                }

                return new ResultadoRegistroCuenta
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
