using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PictionaryMusicalCliente.Modelo;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Helpers
{
    internal static class AvatarServicioAyudante
    {
        private const string BaseImagenesRemotas = "http://localhost:8086/";

        public static IReadOnlyList<ObjetoAvatar> Convertir(DTOs.AvatarDTO[] avatares)
        {
            if (avatares == null || avatares.Length == 0)
                return Array.Empty<ObjetoAvatar>();

            return avatares
                .Where(avatar => avatar != null)
                .Select(Convertir)
                .Where(avatar => avatar != null)
                .ToList();
        }

        public static ObjetoAvatar Convertir(DTOs.AvatarDTO dto)
        {
            if (dto == null)
                return null;

            string rutaAbsoluta = ObtenerRutaAbsoluta(dto.RutaRelativa);

            ImageSource imagen = CrearImagen(rutaAbsoluta);

            return new ObjetoAvatar(
                dto.Id,
                dto.Nombre,
                imagen,
                rutaRelativa: dto.RutaRelativa,
                imagenUriAbsoluta: rutaAbsoluta);
        }

        private static ImageSource CrearImagen(string rutaAbsoluta)
        {
            if (string.IsNullOrWhiteSpace(rutaAbsoluta))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(rutaAbsoluta, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private static string ObtenerRutaAbsoluta(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
                return null;

            if (Uri.TryCreate(rutaRelativa, UriKind.Absolute, out Uri uriAbsoluta))
                return uriAbsoluta.ToString();

            string rutaNormalizada = rutaRelativa.TrimStart('/');

            if (!string.IsNullOrEmpty(BaseImagenesRemotas)
                && Uri.TryCreate(BaseImagenesRemotas, UriKind.Absolute, out Uri baseUri))
            {
                return new Uri(baseUri, rutaNormalizada).ToString();
            }

            return null;
        }
    }
}
