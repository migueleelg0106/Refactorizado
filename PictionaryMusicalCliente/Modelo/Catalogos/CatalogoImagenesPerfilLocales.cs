using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PictionaryMusicalCliente.Modelo.Catalogos
{
    public static class CatalogoImagenesPerfilLocales
    {
        private static readonly IReadOnlyDictionary<string, ImageSource> IconosRedesSociales =
            new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase)
            {
                ["Instagram"] = CrearImagen("instagram.png"),
                ["Facebook"] = CrearImagen("facebook.png"),
                ["X"] = CrearImagen("x_logo.png"),
                ["Discord"] = CrearImagen("discord.png")
            };

        public static ImageSource ObtenerIconoRedSocial(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return null;
            }

            IconosRedesSociales.TryGetValue(nombre, out ImageSource icono);
            return icono;
        }

        private static ImageSource CrearImagen(string archivo)
        {
            var uri = new Uri($"pack://application:,,,/Recursos/{archivo}", UriKind.Absolute);
            return new BitmapImage(uri);
        }
    }
}
