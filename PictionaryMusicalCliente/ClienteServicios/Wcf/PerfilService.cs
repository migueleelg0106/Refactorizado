using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class PerfilService : IPerfilService
    {
        private const string PerfilEndpoint = "BasicHttpBinding_IPerfilManejador";
        private const string CatalogoAvataresEndpoint = "BasicHttpBinding_ICatalogoAvatares";

        public async Task<DTOs.UsuarioDTO> ObtenerPerfilAsync(int usuarioId)
        {
            var cliente = new PictionaryServidorServicioPerfil.PerfilManejadorClient(PerfilEndpoint);

            try
            {
                DTOs.UsuarioDTO perfilDto = await WcfClientHelper
                    .UsarAsync(cliente, c => c.ObtenerPerfilAsync(usuarioId))
                    .ConfigureAwait(false);

                return perfilDto;
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

        public async Task<DTOs.ResultadoOperacionDTO> ActualizarPerfilAsync(DTOs.ActualizarPerfilSolicitudDTO solicitud)
        {
            if (solicitud == null)
                throw new ArgumentNullException(nameof(solicitud));

            var cliente = new PictionaryServidorServicioPerfil.PerfilManejadorClient(PerfilEndpoint);

            try
            {
                var dto = new DTOs.ActualizarPerfilDTO
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

                DTOs.ResultadoOperacionDTO resultado = await WcfClientHelper
                    .UsarAsync(cliente, c => c.ActualizarPerfilAsync(dto))
                    .ConfigureAwait(false);

                if (resultado == null)
                    return null;

                return new DTOs.ResultadoOperacionDTO
                {
                    OperacionExitosa = resultado.OperacionExitosa,
                    Mensaje = resultado.Mensaje
                };
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
            var cliente = new PictionaryServidorServicioAvatares.CatalogoAvataresClient(CatalogoAvataresEndpoint);

            try
            {
                DTOs.AvatarDTO[] avatares = await WcfClientHelper
                    .UsarAsync(cliente, c => c.ObtenerAvataresDisponiblesAsync())
                    .ConfigureAwait(false);

                if (avatares == null)
                    return Array.Empty<ObjetoAvatar>();

                var lista = new List<ObjetoAvatar>();

                foreach (var avatar in avatares)
                {
                    lista.Add(new ObjetoAvatar
                    {
                        Id = avatar.Id,
                        RutaRelativa = avatar.RutaRelativa
                    });
                }

                return lista.AsReadOnly();
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
