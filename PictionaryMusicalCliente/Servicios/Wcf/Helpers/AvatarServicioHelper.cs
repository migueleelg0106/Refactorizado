using System;
using System.Collections.Generic;
using System.Linq;
using PictionaryMusicalCliente.Modelo;
using AvataresSrv = PictionaryMusicalCliente.PictionaryServidorServicioAvatares;

namespace PictionaryMusicalCliente.Servicios.Wcf.Helpers
{
    internal static class AvatarServicioHelper
    {
        private const string BaseImagenesRemotas = "http://localhost:8086/";

        public static IReadOnlyList<ObjetoAvatar> Convertir(AvataresSrv.AvatarDTO[] avatares)
        {
            if (avatares == null || avatares.Length == 0)
            {
                return Array.Empty<ObjetoAvatar>();
            }

            return avatares
                .Where(avatar => avatar != null)
                .Select(Convertir)
                .Where(avatar => avatar != null)
                .ToList();
        }

        public static ObjetoAvatar Convertir(AvataresSrv.AvatarDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new ObjetoAvatar(
                dto.Id,
                dto.Nombre,
                imagen: null,
                rutaRelativa: dto.RutaRelativa,
                imagenUriAbsoluta: ObtenerRutaAbsoluta(dto.RutaRelativa));
        }

        private static string ObtenerRutaAbsoluta(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
            {
                return null;
            }

            if (Uri.TryCreate(rutaRelativa, UriKind.Absolute, out Uri uriAbsoluta))
            {
                return uriAbsoluta.ToString();
            }

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
