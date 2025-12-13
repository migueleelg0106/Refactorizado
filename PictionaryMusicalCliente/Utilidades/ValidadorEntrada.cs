using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.Text.RegularExpressions;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Utilidades
{
    /// <summary>
    /// Provee metodos estaticos para validar la entrada de datos del usuario.
    /// </summary>
    public static class ValidadorEntrada
    {
        private static readonly Regex CorreoRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));

        private static readonly Regex ContrasenaRegex = new Regex(
            @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-\[\]{};:'"",.<>/?]).{8,15}$",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        /// <summary>
        /// Valida que el nombre de usuario no este vacio.
        /// </summary>
        public static DTOs.ResultadoOperacionDTO ValidarUsuario(string usuario)
        {
            return ValidarCampoObligatorio(usuario, Lang.errorTextoCampoObligatorio);
        }

        /// <summary>
        /// Valida que el nombre real no este vacio.
        /// </summary>
        public static DTOs.ResultadoOperacionDTO ValidarNombre(string nombre)
        {
            return ValidarCampoObligatorio(
                nombre,
                Lang.errorTextoNombreObligatorioLongitud);
        }

        /// <summary>
        /// Valida que el apellido no este vacio.
        /// </summary>
        public static DTOs.ResultadoOperacionDTO ValidarApellido(string apellido)
        {
            return ValidarCampoObligatorio(
                apellido,
                Lang.errorTextoApellidoObligatorioLongitud);
        }

        /// <summary>
        /// Valida el formato y presencia del correo electronico.
        /// </summary>
        public static DTOs.ResultadoOperacionDTO ValidarCorreo(string correo)
        {
            DTOs.ResultadoOperacionDTO resultado = ValidarCampoObligatorio(
                correo,
                Lang.errorTextoCorreoInvalido);

            if (!EsOperacionExitosa(resultado))
            {
                return resultado;
            }

            string correoNormalizado = correo.Trim();

            if (!CorreoRegex.IsMatch(correoNormalizado))
            {
                return CrearResultadoFallido(Lang.errorTextoCorreoInvalido);
            }

            return CrearResultadoExitoso();
        }

        /// <summary>
        /// Valida que la contrase�a cumpla con los requisitos de seguridad.
        /// </summary>
        public static DTOs.ResultadoOperacionDTO ValidarContrasena(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
            {
                return CrearResultadoFallido(Lang.errorTextoCampoObligatorio);
            }

            string contrasenaNormalizada = contrasena.Trim();

            if (!ContrasenaRegex.IsMatch(contrasenaNormalizada))
            {
                return CrearResultadoFallido(Lang.errorTextoContrasenaFormato);
            }

            return CrearResultadoExitoso();
        }

        private static DTOs.ResultadoOperacionDTO ValidarCampoObligatorio(
            string valor,
            string mensajeCampoVacio)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return CrearResultadoFallido(mensajeCampoVacio);
            }

            string valorNormalizado = valor.Trim();

            if (valorNormalizado.Length == 0)
            {
                return CrearResultadoFallido(mensajeCampoVacio);
            }

            return CrearResultadoExitoso();
        }

        private static DTOs.ResultadoOperacionDTO CrearResultadoExitoso()
        {
            return new DTOs.ResultadoOperacionDTO
            {
                OperacionExitosa = true
            };
        }

        private static DTOs.ResultadoOperacionDTO CrearResultadoFallido(string mensaje)
        {
            return new DTOs.ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = mensaje
            };
        }

        private static bool EsOperacionExitosa(DTOs.ResultadoOperacionDTO resultado)
        {
            return resultado != null && resultado.OperacionExitosa;
        }
    }
}