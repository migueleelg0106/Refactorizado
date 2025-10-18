using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using InicioSesionSrv = PictionaryMusicalCliente.PictionaryServidorServicioInicioSesion;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class InicioSesionService : IInicioSesionService
    {
        public async Task<ResultadoInicioSesion> IniciarSesionAsync(SolicitudInicioSesion solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            var cliente = new InicioSesionSrv.InicioSesionManejadorClient("BasicHttpBinding_IInicioSesionManejador");
            try
            {
                var dto = new InicioSesionSrv.CredencialesInicioSesionDTO
                {
                    Identificador = solicitud.Identificador?.Trim(),
                    Contrasena = solicitud.Contrasena
                };

                InicioSesionSrv.ResultadoInicioSesionDTO resultadoDto = await WcfClientHelper
                    .UsarAsync(cliente, c => c.IniciarSesionAsync(dto)).ConfigureAwait(false);

                if (resultadoDto == null)
                {
                    return null;
                }

                string mensaje = resultadoDto.Mensaje;

                if (!string.IsNullOrWhiteSpace(mensaje))
                {
                    mensaje = MensajeServidorHelper.Localizar(mensaje, mensaje);
                }

                return new ResultadoInicioSesion
                {
                    InicioSesionExitoso = resultadoDto.InicioSesionExitoso,
                    CuentaNoEncontrada = resultadoDto.CuentaNoEncontrada,
                    ContrasenaIncorrecta = resultadoDto.ContrasenaIncorrecta,
                    Mensaje = mensaje,
                    Usuario = MapearUsuario(resultadoDto.Usuario)
                };
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

        private static UsuarioSesion MapearUsuario(InicioSesionSrv.UsuarioDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new UsuarioSesion
            {
                IdUsuario = dto.IdUsuario,
                JugadorId = dto.JugadorId,
                NombreUsuario = dto.NombreUsuario,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Correo = dto.Correo,
                AvatarId = dto.AvatarId,
                AvatarRutaRelativa = dto.AvatarRutaRelativa
            };
        }
    }
}
