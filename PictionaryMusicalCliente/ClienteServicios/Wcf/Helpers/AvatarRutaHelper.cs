namespace PictionaryMusicalCliente.Servicios.Wcf.Helpers
{
    public static class AvatarRutaHelper
    {
        public static string NormalizarRutaRelativa(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
            {
                return null;
            }

            string rutaNormalizada = ruta
                .Trim()
                .TrimStart('~', '/', '\\')
                .Replace('\\', '/');
            return string.IsNullOrWhiteSpace(rutaNormalizada) ? null : rutaNormalizada;
        }

        public static string NormalizarRutaParaComparacion(string ruta)
        {
            string rutaNormalizada = NormalizarRutaRelativa(ruta);
            return string.IsNullOrWhiteSpace(rutaNormalizada)
                ? null
                : rutaNormalizada.ToLowerInvariant();
        }

        public static string NormalizarRutaParaClaveDiccionario(string ruta)
        {
            return NormalizarRutaParaComparacion(ruta) ?? string.Empty;
        }
    }
}
