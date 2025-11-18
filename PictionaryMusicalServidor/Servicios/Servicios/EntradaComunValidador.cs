using System;
using System.Linq;
using System.Text.RegularExpressions;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    internal static class EntradaComunValidador
    {
        internal const int LongitudMaximaTexto = 50;
        internal const int LongitudMaximaContrasena = 15;
        internal const int LongitudCodigoVerificacion = 6;

        private static readonly Regex CorreoRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));

        private static readonly Regex ContrasenaRegex = new Regex(
            @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-\[\]{};:'"",.<>/?]).{8,15}$",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        private static readonly Regex TokenRegex = new Regex(
            @"^[a-fA-F0-9]{32}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));

        public static string NormalizarTexto(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        public static bool EsLongitudValida(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor) && valor.Length <= LongitudMaximaTexto;
        }

        public static bool EsCorreoValido(string valor)
        {
            return EsLongitudValida(valor) && CorreoRegex.IsMatch(valor);
        }

        public static bool EsContrasenaValida(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return false;
            }

            string normalizado = valor.Trim();
            return normalizado.Length <= LongitudMaximaContrasena && ContrasenaRegex.IsMatch(normalizado);
        }

        public static bool EsTokenValido(string token)
        {
            string normalizado = NormalizarTexto(token);
            return normalizado != null && TokenRegex.IsMatch(normalizado);
        }

        public static bool EsCodigoVerificacionValido(string codigo)
        {
            string normalizado = NormalizarTexto(codigo);
            if (normalizado == null || normalizado.Length != LongitudCodigoVerificacion)
            {
                return false;
            }

            return normalizado.All(char.IsDigit);
        }

        public static ResultadoOperacionDTO ValidarNuevaCuenta(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                return CrearResultadoOperacion(false, MensajesError.Cliente.DatosInvalidos);
            }

            ResultadoOperacionDTO resultado = ValidarCampoObligatorio(
                ref nuevaCuenta.Usuario,
                EsLongitudValida,
                MensajesError.Cliente.UsuarioRegistroInvalido);
            if (resultado != null)
            {
                return resultado;
            }

            resultado = ValidarCampoObligatorio(
                ref nuevaCuenta.Nombre,
                EsLongitudValida,
                MensajesError.Cliente.NombreRegistroInvalido);
            if (resultado != null)
            {
                return resultado;
            }

            resultado = ValidarCampoObligatorio(
                ref nuevaCuenta.Apellido,
                EsLongitudValida,
                MensajesError.Cliente.ApellidoRegistroInvalido);
            if (resultado != null)
            {
                return resultado;
            }

            resultado = ValidarCampoObligatorio(
                ref nuevaCuenta.Correo,
                EsCorreoValido,
                MensajesError.Cliente.CorreoRegistroInvalido);
            if (resultado != null)
            {
                return resultado;
            }

            resultado = ValidarCampoObligatorio(
                ref nuevaCuenta.Contrasena,
                EsContrasenaValida,
                MensajesError.Cliente.ContrasenaRegistroInvalida);
            if (resultado != null)
            {
                return resultado;
            }

            return CrearResultadoOperacion(true);
        }

        public static ResultadoOperacionDTO ValidarActualizacionPerfil(ActualizacionPerfilDTO solicitud)
        {
            if (solicitud == null || solicitud.UsuarioId <= 0)
            {
                return CrearResultadoOperacion(false, MensajesError.Cliente.DatosInvalidos);
            }

            ResultadoOperacionDTO resultado = ValidarCampoObligatorio(
                ref solicitud.Nombre,
                EsLongitudValida,
                MensajesError.Cliente.NombreRegistroInvalido);
            if (resultado != null)
            {
                return resultado;
            }

            resultado = ValidarCampoObligatorio(
                ref solicitud.Apellido,
                EsLongitudValida,
                MensajesError.Cliente.ApellidoRegistroInvalido);
            if (resultado != null)
            {
                return resultado;
            }

            if (solicitud.AvatarId <= 0)
            {
                return CrearResultadoOperacion(false, MensajesError.Cliente.AvatarInvalido);
            }

            resultado = ValidarRedesSociales(solicitud);
            if (resultado != null)
            {
                return resultado;
            }

            return CrearResultadoOperacion(true);
        }

        private static ResultadoOperacionDTO ValidarCampoObligatorio(
            ref string campo,
            Func<string, bool> regla,
            string mensajeError)
        {
            string normalizado = NormalizarTexto(campo);
            if (!regla(normalizado))
            {
                return CrearResultadoOperacion(false, mensajeError);
            }

            campo = normalizado;
            return null;
        }

        private static ResultadoOperacionDTO ValidarRedesSociales(ActualizacionPerfilDTO solicitud)
        {
            ResultadoOperacionDTO resultado = ValidarRedSocial("Instagram", ref solicitud.Instagram);
            if (resultado != null)
            {
                return resultado;
            }

            resultado = ValidarRedSocial("Facebook", ref solicitud.Facebook);
            if (resultado != null)
            {
                return resultado;
            }

            resultado = ValidarRedSocial("X", ref solicitud.X);
            if (resultado != null)
            {
                return resultado;
            }

            return ValidarRedSocial("Discord", ref solicitud.Discord);
        }

        private static ResultadoOperacionDTO ValidarRedSocial(string nombre, ref string valor)
        {
            string normalizado = NormalizarTexto(valor);
            if (normalizado == null)
            {
                valor = null;
                return null;
            }

            if (normalizado.Length > LongitudMaximaTexto)
            {
                return CrearResultadoOperacion(
                    false,
                    $"El identificador de {nombre} no debe exceder {LongitudMaximaTexto} caracteres.");
            }

            valor = normalizado;
            return null;
        }

        private static ResultadoOperacionDTO CrearResultadoOperacion(bool exitoso, string mensaje = null)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = exitoso,
                Mensaje = mensaje
            };
        }
    }
}
