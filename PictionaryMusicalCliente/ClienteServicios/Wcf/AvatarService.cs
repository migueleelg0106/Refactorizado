using PictionaryMusicalCliente.ClienteServicios.Wcf.Helpers;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf
{
    public class AvatarService : IAvatarService
    {
        private const string CatalogoAvataresEndpoint = "BasicHttpBinding_ICatalogoAvatares";

        public async Task<IReadOnlyList<ObjetoAvatar>> ObtenerCatalogoAsync()
        {
            DTOs.AvatarDTO[] avatares = await EjecutarCatalogoAsync(
                cliente => cliente.ObtenerAvataresDisponiblesAsync()).ConfigureAwait(false);

            IReadOnlyList<ObjetoAvatar> lista = AvatarServicioHelper.Convertir(avatares);

            return lista?.Count > 0 ? lista : Array.Empty<ObjetoAvatar>();
        }

        public async Task<int?> ObtenerIdPorRutaAsync(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
                return null;

            IReadOnlyList<ObjetoAvatar> avatares = await ObtenerCatalogoAsync().ConfigureAwait(false);

            if (avatares == null || avatares.Count == 0)
                return null;

            string rutaNormalizada = AvatarRutaHelper.NormalizarRutaParaComparacion(rutaRelativa);

            foreach (ObjetoAvatar avatar in avatares)
            {
                string rutaAvatar = AvatarRutaHelper.NormalizarRutaParaComparacion(avatar?.RutaRelativa);

                if (!string.IsNullOrEmpty(rutaAvatar) &&
                    string.Equals(rutaAvatar, rutaNormalizada, StringComparison.OrdinalIgnoreCase))
                {
                    return avatar.Id;
                }
            }

            return null;
        }

        private async Task<T> EjecutarCatalogoAsync<T>(Func<PictionaryServidorServicioAvatares.CatalogoAvataresClient, Task<T>> operacion)
        {
            var cliente = new PictionaryServidorServicioAvatares.CatalogoAvataresClient(CatalogoAvataresEndpoint);

            try
            {
                return await WcfClientHelper.UsarAsync(cliente, operacion).ConfigureAwait(false);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorInformacionAvatar);
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
