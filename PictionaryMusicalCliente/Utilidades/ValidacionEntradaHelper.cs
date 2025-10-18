using System;
using System.Text.RegularExpressions;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente.Utilidades
{
    public static class ValidacionEntradaHelper
    {
        public const int LongitudMaximaNombreUsuario = 50;
        public const int LongitudMaximaNombre = 50;
        public const int LongitudMaximaApellido = 50;
        public const int LongitudMaximaCorreo = 50;
        public const int LongitudMaximaContrasena = 60;
        public const int LongitudMaximaRedSocial = 50;

        private static readonly Regex PatronCorreoValido = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex PatronContrasenaValida = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,15}$", RegexOptions.Compiled);

        public enum TipoCampo
        {
            Texto,
            Correo,
            Contrasena
        }

        public sealed class ResultadoValidacion
        {
            private ResultadoValidacion(bool esValido, string valorNormalizado, string mensajeError)
            {
                EsValido = esValido;
                ValorNormalizado = valorNormalizado;
                MensajeError = mensajeError;
            }

            public bool EsValido { get; }

            public string ValorNormalizado { get; }

            public string MensajeError { get; }

            public static ResultadoValidacion CrearValido(string valorNormalizado)
            {
                return new ResultadoValidacion(true, valorNormalizado, null);
            }

            public static ResultadoValidacion CrearInvalido(string mensajeError)
            {
                return new ResultadoValidacion(false, null, mensajeError);
            }
        }

        public static ResultadoValidacion ValidarUsuario(string valor)
        {
            return ValidarCampoInterno(
                valor,
                LangResources.Lang.globalTextoUsuario.ToLowerInvariant(),
                LongitudMaximaNombreUsuario,
                esObligatorio: true,
                TipoCampo.Texto,
                aplicarTrim: true);
        }

        public static ResultadoValidacion ValidarNombre(string valor)
        {
            return ValidarCampoInterno(
                valor,
                LangResources.Lang.globalTextoNombre.ToLowerInvariant(),
                LongitudMaximaNombre,
                esObligatorio: true,
                TipoCampo.Texto,
                aplicarTrim: true);
        }

        public static ResultadoValidacion ValidarApellido(string valor)
        {
            return ValidarCampoInterno(
                valor,
                LangResources.Lang.globalTextoApellido.ToLowerInvariant(),
                LongitudMaximaApellido,
                esObligatorio: true,
                TipoCampo.Texto,
                aplicarTrim: true);
        }

        public static ResultadoValidacion ValidarCorreo(string valor)
        {
            return ValidarCampoInterno(
                valor,
                LangResources.Lang.globalTextoCorreo.ToLowerInvariant(),
                LongitudMaximaCorreo,
                esObligatorio: true,
                TipoCampo.Correo,
                aplicarTrim: true);
        }

        public static ResultadoValidacion ValidarContrasena(string valor)
        {
            return ValidarCampoInterno(
                valor,
                LangResources.Lang.globalTextoContrasena.ToLowerInvariant(),
                LongitudMaximaContrasena,
                esObligatorio: true,
                TipoCampo.Contrasena,
                aplicarTrim: false);
        }

        public static ResultadoValidacion ValidarIdentificadorInicioSesion(string valor)
        {
            string texto = valor?.Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                string mensajeObligatorio = string.Format(
                    LangResources.Lang.errorTextoCampoObligatorio,
                    LangResources.Lang.inicioSesionTextoCorreoUsuario.ToLowerInvariant());
                return ResultadoValidacion.CrearInvalido(mensajeObligatorio);
            }

            if (texto.Contains("@"))
            {
                return ValidarCampoInterno(
                    texto,
                    LangResources.Lang.globalTextoCorreo.ToLowerInvariant(),
                    LongitudMaximaCorreo,
                    esObligatorio: true,
                    TipoCampo.Correo,
                    aplicarTrim: false);
            }

            return ValidarCampoInterno(
                texto,
                LangResources.Lang.globalTextoUsuario.ToLowerInvariant(),
                LongitudMaximaNombreUsuario,
                esObligatorio: true,
                TipoCampo.Texto,
                aplicarTrim: false);
        }

        public static ResultadoValidacion ValidarContrasenaOpcional(string valor)
        {
            return ValidarCampoInterno(
                valor,
                LangResources.Lang.globalTextoContrasena.ToLowerInvariant(),
                LongitudMaximaContrasena,
                esObligatorio: false,
                TipoCampo.Contrasena,
                aplicarTrim: false);
        }

        public static ResultadoValidacion ValidarRedSocial(string identificador, string nombreRed)
        {
            if (string.IsNullOrWhiteSpace(identificador))
            {
                return ResultadoValidacion.CrearValido(null);
            }

            string texto = identificador.Trim();

            if (string.Equals(texto, "@", StringComparison.Ordinal))
            {
                return ResultadoValidacion.CrearValido(null);
            }

            if (texto.StartsWith("@", StringComparison.Ordinal))
            {
                string contenido = texto.Substring(1);

                if (string.IsNullOrWhiteSpace(contenido))
                {
                    return ResultadoValidacion.CrearValido(null);
                }
            }

            if (texto.Length > LongitudMaximaRedSocial)
            {
                string descripcionRed = string.IsNullOrWhiteSpace(nombreRed)
                    ? LangResources.Lang.globalTextoRedSocial.ToLowerInvariant()
                    : nombreRed;

                string mensajeError = string.Format(
                    LangResources.Lang.errorTextoIdentificadorRedSocialLongitud,
                    descripcionRed,
                    LongitudMaximaRedSocial);
                return ResultadoValidacion.CrearInvalido(mensajeError);
            }

            return ResultadoValidacion.CrearValido(texto);
        }

        private static ResultadoValidacion ValidarCampoInterno(
            string valor,
            string descripcionCampo,
            int longitudMaxima,
            bool esObligatorio,
            TipoCampo tipoCampo,
            bool aplicarTrim)
        {
            string texto = valor;

            if (aplicarTrim && texto != null)
            {
                texto = texto.Trim();
            }

            if (esObligatorio && string.IsNullOrWhiteSpace(texto))
            {
                string mensaje = string.Format(
                    LangResources.Lang.errorTextoCampoObligatorio,
                    descripcionCampo);
                return ResultadoValidacion.CrearInvalido(mensaje);
            }

            if (string.IsNullOrWhiteSpace(texto))
            {
                return ResultadoValidacion.CrearValido(null);
            }

            if (texto.Length > longitudMaxima)
            {
                string mensaje = string.Format(
                    LangResources.Lang.errorTextoCampoLongitudMaxima,
                    descripcionCampo,
                    longitudMaxima);
                return ResultadoValidacion.CrearInvalido(mensaje);
            }

            if (tipoCampo == TipoCampo.Correo && !PatronCorreoValido.IsMatch(texto))
            {
                return ResultadoValidacion.CrearInvalido(LangResources.Lang.errorTextoCorreoInvalido);
            }

            if (tipoCampo == TipoCampo.Contrasena && !PatronContrasenaValida.IsMatch(texto))
            {
                return ResultadoValidacion.CrearInvalido(LangResources.Lang.errorTextoContrasenaFormato);
            }

            return ResultadoValidacion.CrearValido(texto);
        }
    }
}
