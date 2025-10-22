using System.Text.RegularExpressions;
using PictionaryMusicalCliente.Properties.Langs;
using DTOs = global::Servicios.Contratos.DTOs;

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

        public static DTOs.ResultadoOperacionDTO ValidarUsuario(string usuario)
        {
            return ValidarCampoObligatorio(usuario, Lang.errorTextoCampoObligatorio);
        }

        public static DTOs.ResultadoOperacionDTO ValidarNombre(string nombre)
        {
            return ValidarCampoObligatorio(nombre, Lang.errorTextoNombreObligatorioLongitud);
        }

        public static DTOs.ResultadoOperacionDTO ValidarApellido(string apellido)
        {
            return ValidarCampoObligatorio(apellido, Lang.errorTextoApellidoObligatorioLongitud);
        }

        public static DTOs.ResultadoOperacionDTO ValidarCorreo(string correo)
        {
            DTOs.ResultadoOperacionDTO resultado = ValidarCampoObligatorio(correo,
                Lang.errorTextoCorreoInvalido);

            if (!resultado.Exito)
            {
                return resultado;
            }

            string correoNormalizado = correo.Trim();

            if (!CorreoRegex.IsMatch(correoNormalizado))
            {
                return DTOs.ResultadoOperacionDTO.Fallo(Lang.errorTextoCorreoInvalido);
            }

            return DTOs.ResultadoOperacionDTO.Exitoso();
        }

        public static DTOs.ResultadoOperacionDTO ValidarContrasena(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
            {
                return DTOs.ResultadoOperacionDTO.Fallo(Lang.errorTextoCampoObligatorio);
            }

            string contrasenaNormalizada = contrasena.Trim();

            if (!ContrasenaRegex.IsMatch(contrasenaNormalizada))
            {
                return DTOs.ResultadoOperacionDTO.Fallo(Lang.errorTextoContrasenaFormato);
            }

            return DTOs.ResultadoOperacionDTO.Exitoso();
        }

        private static DTOs.ResultadoOperacionDTO    ValidarCampoObligatorio(
            string valor,
            string mensajeCampoVacio)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return DTOs.ResultadoOperacionDTO.Fallo(mensajeCampoVacio);
            }

            string valorNormalizado = valor.Trim();

            if (valorNormalizado.Length == 0)
            {
                return DTOs.ResultadoOperacionDTO.Fallo(mensajeCampoVacio);
            }

            return DTOs.ResultadoOperacionDTO.Exitoso();
        }
    }
}
