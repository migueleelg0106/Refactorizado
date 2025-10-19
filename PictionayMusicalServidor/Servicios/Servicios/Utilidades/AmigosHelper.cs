using Datos.Modelo;
using Servicios.Contratos.DTOs;

namespace Servicios.Servicios.Utilidades
{
    internal static class AmigosHelper
    {
        public static bool EstaAceptada(Solicitud solicitud)
        {
            return solicitud?.Estado != null && solicitud.Estado.Length > 0 && solicitud.Estado[0] == 1;
        }

        public static byte[] ConvertirEstado(bool estado)
        {
            return new[] { (byte)(estado ? 1 : 0) };
        }

        public static AmigoDTO CrearAmigoDTO(Jugador jugador)
        {
            if (jugador == null)
            {
                return null;
            }

            return new AmigoDTO
            {
                IdJugador = jugador.idJugador,
                Nombre = jugador.Nombre,
                Apellido = jugador.Apellido,
                Correo = jugador.Correo,
                AvatarId = jugador.Avatar_idAvatar,
                AvatarRutaRelativa = jugador.Avatar?.Avatar_Ruta
            };
        }
    }
}
