using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;

namespace PictionaryMusicalCliente.Utilidades
{
    internal static class AvatarAyudante
    {
        private static readonly object _sincronizacion = new object();

        private static IReadOnlyList<ObjetoAvatar> _avatares;
        private static Dictionary<int, ObjetoAvatar> _avataresPorId;
        private static Dictionary<string, ObjetoAvatar> _avataresPorRuta;

        static AvatarAyudante()
        {
            RestablecerCatalogoPredeterminado();
        }

        public static void ActualizarCatalogo(IEnumerable<ObjetoAvatar> avatares)
        {
            if (avatares == null)
            {
                RestablecerCatalogoPredeterminado();
                return;
            }

            var lista = new List<ObjetoAvatar>();
            var porId = new Dictionary<int, ObjetoAvatar>();
            var porRuta = new Dictionary<string, ObjetoAvatar>(StringComparer.OrdinalIgnoreCase);

            foreach (ObjetoAvatar avatar in avatares)
            {
                if (avatar == null)
                {
                    continue;
                }

                if (!porId.ContainsKey(avatar.Id))
                {
                    lista.Add(avatar);
                }

                porId[avatar.Id] = avatar;

                string rutaNormalizada = NormalizarRuta(avatar.RutaRelativa);
                if (!string.IsNullOrEmpty(rutaNormalizada))
                {
                    porRuta[rutaNormalizada] = avatar;
                }
            }

            if (lista.Count == 0)
            {
                RestablecerCatalogoPredeterminado();
                return;
            }

            AsignarCatalogo(lista, porId, porRuta);
        }

        public static void RestablecerCatalogoPredeterminado()
        {
            IReadOnlyList<ObjetoAvatar> locales = CatalogoAvataresLocales.ObtenerAvatares();
            var lista = locales?.Where(avatar => avatar != null).ToList() ?? new List<ObjetoAvatar>();
            var porId = lista.ToDictionary(avatar => avatar.Id);
            var porRuta = lista
                .Where(avatar => !string.IsNullOrWhiteSpace(avatar.RutaRelativa))
                .ToDictionary(
                    avatar => NormalizarRuta(avatar.RutaRelativa),
                    avatar => avatar,
                    StringComparer.OrdinalIgnoreCase);

            AsignarCatalogo(lista, porId, porRuta);
        }

        public static IReadOnlyList<ObjetoAvatar> ObtenerAvatares()
        {
            lock (_sincronizacion)
            {
                return _avatares;
            }
        }

        public static ObjetoAvatar ObtenerAvatarPredeterminado()
        {
            lock (_sincronizacion)
            {
                return _avatares != null && _avatares.Count > 0 ? _avatares[0] : null;
            }
        }

        public static ObjetoAvatar ObtenerAvatarPorId(int id)
        {
            lock (_sincronizacion)
            {
                if (_avataresPorId != null && _avataresPorId.TryGetValue(id, out ObjetoAvatar avatar))
                {
                    return avatar;
                }
            }

            return null;
        }

        public static ObjetoAvatar ObtenerAvatarPorRuta(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
            {
                return null;
            }

            string rutaNormalizada = NormalizarRuta(rutaRelativa);
            if (rutaNormalizada == null)
            {
                return null;
            }

            lock (_sincronizacion)
            {
                if (_avataresPorRuta != null && _avataresPorRuta.TryGetValue(rutaNormalizada, out ObjetoAvatar avatar))
                {
                    return avatar;
                }
            }

            return null;
        }

        public static bool SonRutasEquivalentes(string rutaA, string rutaB)
        {
            string normalizadaA = NormalizarRuta(rutaA);
            string normalizadaB = NormalizarRuta(rutaB);

            if (normalizadaA == null || normalizadaB == null)
            {
                return false;
            }

            return string.Equals(normalizadaA, normalizadaB, StringComparison.OrdinalIgnoreCase);
        }

        public static ImageSource ObtenerImagen(ObjetoAvatar avatar)
        {
            if (avatar == null)
            {
                return null;
            }

            if (avatar.Imagen != null)
            {
                return avatar.Imagen;
            }

            if (string.IsNullOrWhiteSpace(avatar.ImagenUriAbsoluta))
            {
                return null;
            }

            BitmapImage bitmap = null;
            try
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                if (Uri.TryCreate(avatar.ImagenUriAbsoluta, UriKind.Absolute, out Uri uriSource))
                {
                    bitmap.UriSource = uriSource;
                }
                else
                {
                    bitmap.EndInit();
                    return null;
                }
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (UriFormatException uriEx)
            {
                if (bitmap != null && bitmap.IsDownloading) bitmap.EndInit();
                return null;
            }
            catch (FileNotFoundException fnfEx)
            {
                if (bitmap != null && bitmap.IsDownloading) bitmap.EndInit();
                return null;
            }
            catch (WebException webEx)
            {
                if (bitmap != null && bitmap.IsDownloading) bitmap.EndInit();
                return null;
            }
            catch (NotSupportedException nsEx)
            {
                if (bitmap != null && bitmap.IsDownloading) bitmap.EndInit();
                return null;
            }
        }

        private static void AsignarCatalogo(
            List<ObjetoAvatar> lista,
            Dictionary<int, ObjetoAvatar> porId,
            Dictionary<string, ObjetoAvatar> porRuta)
        {
            lock (_sincronizacion)
            {
                _avatares = new ReadOnlyCollection<ObjetoAvatar>(lista);
                _avataresPorId = porId;
                _avataresPorRuta = porRuta;
            }
        }

        private static string NormalizarRuta(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
            {
                return null;
            }

            string rutaNormalizada = ruta.Trim();
            if (rutaNormalizada.StartsWith("~", StringComparison.Ordinal))
            {
                rutaNormalizada = rutaNormalizada.Substring(1);
            }

            rutaNormalizada = rutaNormalizada.TrimStart('/', '\\');
            rutaNormalizada = rutaNormalizada.Replace('\\', '/');

            return rutaNormalizada.Length == 0 ? null : rutaNormalizada;
        }
    }
}
