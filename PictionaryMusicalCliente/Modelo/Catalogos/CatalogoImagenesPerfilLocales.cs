using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PictionaryMusicalCliente.Modelo.Catalogos
{
    /// <summary>
    /// Provee acceso centralizado a los recursos de imagen estaticos utilizados en el perfil.
    /// </summary>
    public class CatalogoImagenesPerfilLocales : ICatalogoImagenesPerfil
    {
        private static readonly IReadOnlyDictionary<string, ImageSource> IconosRedesSociales =
            new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase)
            {
                ["Instagram"] = CrearImagen("instagram.png"),
                ["Facebook"] = CrearImagen("facebook.png"),
                ["X"] = CrearImagen("x_logo.png"),
                ["Discord"] = CrearImagen("discord.png")
            };

        /// <summary>
        /// Recupera el icono asociado a una red social especifica.
        /// </summary>
        /// <param name="nombre">Nombre clave de la red social.</param>
        /// <returns>El recurso grafico correspondiente o null si no existe.</returns>
        public ImageSource ObtenerIconoRedSocial(string nombre)
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
            var uri = new Uri(
                $"pack://application:,,,/Recursos/{archivo}",
                UriKind.Absolute);
            return new BitmapImage(uri);
        }
    }
}