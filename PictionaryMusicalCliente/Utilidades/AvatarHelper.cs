using System;
using System.Collections.Generic;
using System.Linq;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;

namespace PictionaryMusicalCliente.Utilidades
{
    internal static class AvatarHelper
    {
        private static readonly IReadOnlyList<ObjetoAvatar> Avatares =
            CatalogoAvataresLocales.ObtenerAvatares();

        private static readonly IReadOnlyDictionary<int, ObjetoAvatar> AvataresPorId =
            Avatares.ToDictionary(avatar => avatar.Id);

        private static readonly IReadOnlyDictionary<string, ObjetoAvatar> AvataresPorRuta =
            Avatares
                .Where(avatar => !string.IsNullOrWhiteSpace(avatar.RutaRelativa))
                .ToDictionary(
                    avatar => NormalizarRuta(avatar.RutaRelativa),
                    avatar => avatar,
                    StringComparer.OrdinalIgnoreCase);

        public static ObjetoAvatar ObtenerAvatarPredeterminado()
        {
            return Avatares.FirstOrDefault();
        }

        public static ObjetoAvatar ObtenerAvatarPorId(int id)
        {
            AvataresPorId.TryGetValue(id, out ObjetoAvatar avatar);
            return avatar;
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

            AvataresPorRuta.TryGetValue(rutaNormalizada, out ObjetoAvatar avatar);
            return avatar;
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
