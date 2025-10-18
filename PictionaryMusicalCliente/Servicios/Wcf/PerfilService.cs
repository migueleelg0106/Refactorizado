using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using AvataresSrv = PictionaryMusicalCliente.PictionaryServidorServicioAvatares;
using PerfilSrv = PictionaryMusicalCliente.PictionaryServidorServicioPerfil;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class PerfilService : IPerfilService
    {
        private const string PerfilEndpoint = "BasicHttpBinding_IPerfilManejador";
        private const string CatalogoAvataresEndpoint = "BasicHttpBinding_ICatalogoAvatares";

        public async Task<UsuarioAutenticado> ObtenerPerfilAsync(int usuarioId)
        {
            var cliente = new PerfilSrv.PerfilManejadorClient(PerfilEndpoint);

            try
            {
                PerfilSrv.UsuarioDTO perfilDto = await WcfClientHelper
                    .UsarAsync(cliente, c => c.ObtenerPerfilAsync(usuarioId))
                    .ConfigureAwait(false);

                return UsuarioMapper.CrearDesde(perfilDto);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorObtenerPerfil);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.Comunicacion,
                    Lang.avisoTextoComunicacionServidorSesion,
                    ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.avisoTextoServidorTiempoSesion,
                    ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.Comunicacion,
                    Lang.avisoTextoComunicacionServidorSesion,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoPerfilActualizarInformacion,
                    ex);
            }
        }

        public async Task<ResultadoOperacion> ActualizarPerfilAsync(ActualizarPerfilSolicitud solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            var cliente = new PerfilSrv.PerfilManejadorClient(PerfilEndpoint);

            try
            {
                var dto = new PerfilSrv.ActualizarPerfilDTO
                {
                    UsuarioId = solicitud.UsuarioId,
                    Nombre = solicitud.Nombre,
                    Apellido = solicitud.Apellido,
                    AvatarRutaRelativa = solicitud.AvatarRutaRelativa,
                    Instagram = solicitud.Instagram,
                    Facebook = solicitud.Facebook,
                    X = solicitud.X,
                    Discord = solicitud.Discord
                };

                PerfilSrv.ResultadoOperacionDTO resultado = await WcfClientHelper
                    .UsarAsync(cliente, c => c.ActualizarPerfilAsync(dto))
                    .ConfigureAwait(false);

                if (resultado == null)
                {
                    return null;
                }

                return resultado.OperacionExitosa
                    ? ResultadoOperacion.Exitoso(resultado.Mensaje)
                    : ResultadoOperacion.Fallo(resultado.Mensaje);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorActualizarPerfil);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }

        public async Task<IReadOnlyList<ObjetoAvatar>> ObtenerAvataresDisponiblesAsync()
        {
            var cliente = new AvataresSrv.CatalogoAvataresClient(CatalogoAvataresEndpoint);

            try
            {
                AvataresSrv.AvatarDTO[] avatares = await WcfClientHelper
                    .UsarAsync(cliente, c => c.ObtenerAvataresDisponiblesAsync())
                    .ConfigureAwait(false);

                return AvatarServicioHelper.Convertir(avatares);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorNoDisponible);
                throw new ServicioException(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ServicioException(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }
    }
}
