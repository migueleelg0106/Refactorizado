using PictionaryMusicalCliente.ClienteServicios.Wcf.Helpers;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class PerfilServicio : IPerfilServicio
    {
        private const string PerfilEndpoint = "BasicHttpBinding_IPerfilManejador";
        private const string CatalogoAvataresEndpoint = "BasicHttpBinding_ICatalogoAvatares";

        public async Task<DTOs.UsuarioDTO> ObtenerPerfilAsync(int usuarioId)
        {
            var cliente = new PictionaryServidorServicioPerfil.PerfilManejadorClient(PerfilEndpoint);

            try
            {
                DTOs.UsuarioDTO perfilDto = await WcfClienteAyudante
                    .UsarAsincrono(cliente, c => c.ObtenerPerfilAsync(usuarioId))
                    .ConfigureAwait(false);

                return perfilDto;
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorObtenerPerfil);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.Comunicacion,
                    Lang.avisoTextoComunicacionServidorSesion,
                    ex);
            }
            catch (TimeoutException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.avisoTextoServidorTiempoSesion,
                    ex);
            }
            catch (CommunicationException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.Comunicacion,
                    Lang.avisoTextoComunicacionServidorSesion,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoPerfilActualizarInformacion,
                    ex);
            }
        }

        public async Task<DTOs.ResultadoOperacionDTO> ActualizarPerfilAsync(DTOs.ActualizacionPerfilDTO solicitud)
        {
            if (solicitud == null)
                throw new ArgumentNullException(nameof(solicitud));

            var cliente = new PictionaryServidorServicioPerfil.PerfilManejadorClient(PerfilEndpoint);

            try
            {
                DTOs.ResultadoOperacionDTO resultado = await WcfClienteAyudante
                    .UsarAsincrono(cliente, c => c.ActualizarPerfilAsync(solicitud))
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
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorActualizarPerfil);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }

        public async Task<IReadOnlyList<ObjetoAvatar>> ObtenerAvataresDisponiblesAsync()
        {
            var cliente = new PictionaryServidorServicioAvatares.CatalogoAvataresManejadorClient(CatalogoAvataresEndpoint);

            try
            {
                DTOs.AvatarDTO[] avatares = await WcfClienteAyudante
                    .UsarAsincrono(cliente, c => c.ObtenerAvataresDisponiblesAsync())
                    .ConfigureAwait(false);

                IReadOnlyList<ObjetoAvatar> lista = AvatarServicioAyudante.Convertir(avatares);

                return lista?.Count > 0 ? lista : Array.Empty<ObjetoAvatar>();
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioAyudante.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorNoDisponible);
                throw new ExcepcionServicio(TipoErrorServicio.FallaServicio, mensaje, ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (TimeoutException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.TiempoAgotado,
                    Lang.errorTextoServidorTiempoAgotado,
                    ex);
            }
            catch (CommunicationException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.Comunicacion,
                    Lang.errorTextoServidorNoDisponible,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ExcepcionServicio(
                    TipoErrorServicio.OperacionInvalida,
                    Lang.errorTextoErrorProcesarSolicitud,
                    ex);
            }
        }
    }
}
