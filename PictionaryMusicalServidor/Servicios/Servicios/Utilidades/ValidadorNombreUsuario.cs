using System.Globalization;
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Servicio de validación para nombres de usuario.
    /// Centraliza las reglas de validación de nombres de usuario.
    /// </summary>
    internal static class ValidadorNombreUsuario
    {
        /// <summary>
        /// Valida que el nombre de usuario cumpla con los requisitos.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a validar.</param>
        /// <param name="parametro">Nombre del parámetro para mensajes de error.</param>
        /// <exception cref="FaultException">Se lanza si la validación falla.</exception>
        public static void Validar(string nombreUsuario, string parametro)
        {
            string normalizado = nombreUsuario?.Trim();

            if (string.IsNullOrWhiteSpace(normalizado))
            {
                string mensaje = string.Format(CultureInfo.CurrentCulture, MensajesError.Cliente.ParametroObligatorio, parametro);
                throw new FaultException(mensaje);
            }

            if (normalizado.Length > EntradaComunValidador.LongitudMaximaTexto)
            {
                throw new FaultException(MensajesError.Cliente.UsuarioRegistroInvalido);
            }
        }

        /// <summary>
        /// Obtiene el nombre normalizado de usuario, priorizando el de la base de datos.
        /// </summary>
        /// <param name="nombreBaseDatos">Nombre almacenado en la base de datos.</param>
        /// <param name="nombreAlterno">Nombre alternativo a usar si el de BD no es válido.</param>
        /// <returns>Nombre normalizado de usuario.</returns>
        public static string ObtenerNombreNormalizado(string nombreBaseDatos, string nombreAlterno)
        {
            string nombre = nombreBaseDatos?.Trim();

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                return nombre;
            }

            return nombreAlterno?.Trim();
        }
    }
}
