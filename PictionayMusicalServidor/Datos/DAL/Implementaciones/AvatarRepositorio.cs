using System;
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
    }
}
