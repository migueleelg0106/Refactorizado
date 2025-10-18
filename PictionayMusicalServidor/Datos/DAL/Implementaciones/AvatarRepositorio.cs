using System;
using System.Collections.Generic;
using System.Linq;
using Datos.DAL.Interfaces;
using Datos.Modelo;

namespace Datos.DAL.Implementaciones
{
    public class AvatarRepositorio : IAvatarRepositorio
    {
        private readonly BaseDatosPruebaEntities1 contexto;

        public AvatarRepositorio(BaseDatosPruebaEntities1 contexto)
        {
            this.contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public bool ExisteAvatar(int avatarId)
        {
            return contexto.Avatar.Any(a => a.idAvatar == avatarId);
        }

        public IEnumerable<Avatar> ObtenerAvatares()
        {
            return contexto.Avatar
                .Select(a => new Avatar
                {
                    idAvatar = a.idAvatar,
                    Avatar_Ruta = a.Avatar_Ruta,
                    Nombre_Avatar = a.Nombre_Avatar
                })
                .ToList();
        }

        public Avatar ObtenerAvatarPorRuta(string rutaRelativa)
        {
            string rutaNormalizada = NormalizarRuta(rutaRelativa);

            if (rutaNormalizada == null)
            {
                return null;
            }

            return contexto.Avatar
                .AsEnumerable()
                .Select(a => new Avatar
                {
                    idAvatar = a.idAvatar,
                    Avatar_Ruta = a.Avatar_Ruta,
                    Nombre_Avatar = a.Nombre_Avatar
                })
                .FirstOrDefault(a => string.Equals(
                    NormalizarRuta(a.Avatar_Ruta),
                    rutaNormalizada,
                    StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizarRuta(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
            {
                return null;
            }

            string rutaNormalizada = ruta.Trim();
            if (rutaNormalizada.StartsWith("~"))
            {
                rutaNormalizada = rutaNormalizada.Substring(1);
            }

            rutaNormalizada = rutaNormalizada.TrimStart('/', '\\');
            rutaNormalizada = rutaNormalizada.Replace('\\', '/');

            return rutaNormalizada.Length == 0 ? null : rutaNormalizada;
        }
    }
}
