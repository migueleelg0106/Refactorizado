using System;
using System.Globalization;
using System.Reflection;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Servicios.Wcf.Helpers
{
    internal static class UsuarioMapper
    {
        public static UsuarioAutenticado CrearDesde(object dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new UsuarioAutenticado
            {
                IdUsuario = ObtenerValor(dto, nameof(UsuarioAutenticado.IdUsuario), 0),
                JugadorId = ObtenerValor(dto, nameof(UsuarioAutenticado.JugadorId), 0),
                NombreUsuario = ObtenerValor(dto, nameof(UsuarioAutenticado.NombreUsuario), string.Empty),
                Nombre = ObtenerValor(dto, nameof(UsuarioAutenticado.Nombre), string.Empty),
                Apellido = ObtenerValor(dto, nameof(UsuarioAutenticado.Apellido), string.Empty),
                Correo = ObtenerValor(dto, nameof(UsuarioAutenticado.Correo), string.Empty),
                AvatarId = ObtenerValor(dto, nameof(UsuarioAutenticado.AvatarId), 0),
                Instagram = ObtenerValor(dto, nameof(UsuarioAutenticado.Instagram), string.Empty),
                Facebook = ObtenerValor(dto, nameof(UsuarioAutenticado.Facebook), string.Empty),
                X = ObtenerValor(dto, nameof(UsuarioAutenticado.X), string.Empty),
                Discord = ObtenerValor(dto, nameof(UsuarioAutenticado.Discord), string.Empty)
            };
        }

        private static int ObtenerValor(object origen, string propiedad, int predeterminado)
        {
            object valor = ObtenerValor(origen, propiedad);
            if (valor == null)
            {
                return predeterminado;
            }

            try
            {
                return Convert.ToInt32(valor, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return predeterminado;
            }
            catch (InvalidCastException)
            {
                return predeterminado;
            }
        }

        private static string ObtenerValor(object origen, string propiedad, string predeterminado)
        {
            object valor = ObtenerValor(origen, propiedad);
            return valor as string ?? predeterminado;
        }

        private static object ObtenerValor(object origen, string propiedad)
        {
            if (origen == null || string.IsNullOrWhiteSpace(propiedad))
            {
                return null;
            }

            PropertyInfo propiedadInfo = origen.GetType().GetProperty(propiedad, BindingFlags.Public | BindingFlags.Instance);

            if (propiedadInfo == null)
            {
                return null;
            }

            try
            {
                return propiedadInfo.GetValue(origen);
            }
            catch (TargetInvocationException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
