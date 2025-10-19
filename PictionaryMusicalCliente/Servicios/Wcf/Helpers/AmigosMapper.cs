using PictionaryMusicalCliente.Modelo.Amigos;
using AmigosSrv = PictionaryMusicalCliente.PictionaryServidorServicioAmigos;
using ListaAmigosSrv = PictionaryMusicalCliente.PictionaryServidorServicioListaAmigos;

namespace PictionaryMusicalCliente.Servicios.Wcf.Helpers
{
    internal static class AmigosMapper
    {
        public static Amigo Convertir(AmigosSrv.AmigoDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Amigo
            {
                JugadorId = dto.IdJugador,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Correo = dto.Correo,
                AvatarId = dto.AvatarId,
                AvatarRutaRelativa = dto.AvatarRutaRelativa
            };
        }

        public static Amigo Convertir(ListaAmigosSrv.AmigoDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Amigo
            {
                JugadorId = dto.IdJugador,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Correo = dto.Correo,
                AvatarId = dto.AvatarId,
                AvatarRutaRelativa = dto.AvatarRutaRelativa
            };
        }
    }
}
