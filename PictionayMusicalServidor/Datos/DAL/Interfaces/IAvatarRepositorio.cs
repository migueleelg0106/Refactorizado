using System.Collections.Generic;
using Datos.Modelo;

namespace Datos.DAL.Interfaces
{
    public interface IAvatarRepositorio
    {
        bool ExisteAvatar(int avatarId);

        IEnumerable<Avatar> ObtenerAvatares();

        Avatar ObtenerAvatarPorRuta(string rutaRelativa);
    }
}
