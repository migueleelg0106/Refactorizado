using System;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using AvataresSrv = PictionaryMusicalCliente.PictionaryServidorServicioAvatares;

namespace PictionaryMusicalCliente.Servicios.Wcf
{
    public class AvatarService : IAvatarService
    {
        private const string CatalogoAvataresEndpoint = "BasicHttpBinding_ICatalogoAvatares";

        public async Task<int?> ObtenerIdPorRutaAsync(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
            {
                return null;
            }

            var cliente = new AvataresSrv.CatalogoAvataresClient(CatalogoAvataresEndpoint);

            try
            {
                AvataresSrv.AvatarDTO[] avatares = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.ObtenerAvataresDisponiblesAsync()).ConfigureAwait(false);

                if (avatares == null || avatares.Length == 0)
                {
                    return null;
                }

                string rutaNormalizada = AvatarRutaHelper.NormalizarRutaParaComparacion(rutaRelativa);

                foreach (AvataresSrv.AvatarDTO avatar in avatares)
                {
                    string rutaAvatar = AvatarRutaHelper.NormalizarRutaParaComparacion(avatar?.RutaRelativa);

                    if (!string.IsNullOrEmpty(rutaAvatar)
                        && string.Equals(rutaAvatar, rutaNormalizada, StringComparison.OrdinalIgnoreCase))
                    {
                        return avatar.Id;
                    }
                }

                return null;
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
