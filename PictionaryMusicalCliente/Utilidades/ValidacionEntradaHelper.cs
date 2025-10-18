using System.Text.RegularExpressions;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente.Utilidades
{
    public static class ValidacionEntradaHelper
    {
        private static readonly Regex CorreoRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex ContrasenaRegex = new Regex(
            @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-\[\]{};:'"",.<>/?]).{8,15}$",
            RegexOptions.Compiled);

        public static ResultadoOperacion ValidarUsuario(string usuario)
        {
            return ValidarCampoObligatorio(usuario, Lang.errorTextoCampoObligatorio);
        }

        public static ResultadoOperacion ValidarNombre(string nombre)
        {
            return ValidarCampoObligatorio(nombre, Lang.errorTextoNombreObligatorioLongitud);
        }

        public static ResultadoOperacion ValidarApellido(string apellido)
        {
            return ValidarCampoObligatorio(apellido, Lang.errorTextoApellidoObligatorioLongitud);
        }

        public static ResultadoOperacion ValidarCorreo(string correo)
        {
            ResultadoOperacion resultado = ValidarCampoObligatorio(correo,
                Lang.errorTextoCorreoInvalido);

            if (!resultado.Exito)
            {
                return resultado;
            }

            string correoNormalizado = correo.Trim();

            if (!CorreoRegex.IsMatch(correoNormalizado))
            {
                return ResultadoOperacion.Fallo(Lang.errorTextoCorreoInvalido);
            }

            return ResultadoOperacion.Exitoso();
        }

        public static ResultadoOperacion ValidarContrasena(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
            {
                return ResultadoOperacion.Fallo(Lang.errorTextoCampoObligatorio);
            }

            string contrasenaNormalizada = contrasena.Trim();

            if (!ContrasenaRegex.IsMatch(contrasenaNormalizada))
            {
                return ResultadoOperacion.Fallo(Lang.errorTextoContrasenaFormato);
            }

            return ResultadoOperacion.Exitoso();
        }

        private static ResultadoOperacion ValidarCampoObligatorio(
            string valor,
            string mensajeCampoVacio)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return ResultadoOperacion.Fallo(mensajeCampoVacio);
            }

            string valorNormalizado = valor.Trim();

            if (valorNormalizado.Length == 0)
            {
                return ResultadoOperacion.Fallo(mensajeCampoVacio);
            }

            return ResultadoOperacion.Exitoso();
        }
    }
}
