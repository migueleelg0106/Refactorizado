using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Linq;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;

namespace PictionaryMusicalCliente.Utilidades
{
    internal static class AvatarHelper
    {
        private static readonly IReadOnlyDictionary<int, ObjetoAvatar> AvataresPorId =
            CatalogoAvataresLocales.ObtenerAvatares().ToDictionary(avatar => avatar.Id);

        public static ObjetoAvatar ObtenerAvatarPredeterminado()
        {
            return AvataresPorId.Values.FirstOrDefault();
        }

        public static ObjetoAvatar ObtenerAvatarPorId(int id)
        {
            AvataresPorId.TryGetValue(id, out ObjetoAvatar avatar);
            return avatar;
        }
    }
}
