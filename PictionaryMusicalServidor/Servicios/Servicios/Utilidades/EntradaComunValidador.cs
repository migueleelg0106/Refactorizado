using System;
using System.Linq;
using System.Text.RegularExpressions;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Clase utilitaria para validar y normalizar entradas comunes del sistema.
    /// Proporciona validaciones de formato y longitud para texto, correo, contrasena, token y 
    /// datos de cuenta.
    /// Verifica que las contrasenas cumplan con requisitos de seguridad (mayuscula, numero, 
    /// caracter especial).
    /// </summary>
    internal static class EntradaComunValidador
    {
        internal const int LongitudMaximaTexto = 50;
        internal const int LongitudMaximaReporte = 100;
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

        /// <summary>
        /// Normaliza un texto eliminando espacios al inicio y final.
        /// Retorna null si el valor es nulo o solo espacios en blanco.
        /// </summary>
        /// <param name="valor">Texto a normalizar.</param>
        /// <returns>Texto normalizado o null.</returns>
        public static string NormalizarTexto(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        /// <summary>
        /// Verifica si un texto tiene una longitud valida segun el limite maximo.
        /// </summary>
        /// <param name="valor">Texto a validar.</param>
        /// <returns>True si el texto no es vacio y no excede la longitud maxima.</returns>
        public static bool EsLongitudValida(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor) && valor.Length <= LongitudMaximaTexto;
        }

        /// <summary>
        /// Verifica si un texto tiene una longitud valida segun el limite maximo.
        /// </summary>
        /// <param name="valor">Texto a validar.</param>
        /// <returns>True si el texto no es vacio y no excede la longitud maxima.</returns>
        public static bool EsLongitudValidaReporte(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor) && valor.Length <= LongitudMaximaReporte;
        }

        /// <summary>
        /// Verifica si un correo electronico tiene formato valido.
        /// Valida longitud y patron basico de correo electronico.
        /// </summary>
        /// <param name="valor">Correo electronico a validar.</param>
        /// <returns>True si el correo tiene formato valido.</returns>
        public static bool EsCorreoValido(string valor)
        {
            return EsLongitudValida(valor) && CorreoRegex.IsMatch(valor);
        }

        /// <summary>
        /// Verifica si una contrasena cumple con los requisitos de seguridad.
        /// Valida que contenga al menos una mayuscula, un numero, un caracter especial y longitud
        /// entre 8 y 15.
        /// </summary>
        /// <param name="valor">Contrasena a validar.</param>
        /// <returns>True si la contrasena cumple todos los requisitos.</returns>
        public static bool EsContrasenaValida(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return false;
            }

            string normalizado = valor.Trim();
            return normalizado.Length <= LongitudMaximaContrasena && 
                ContrasenaRegex.IsMatch(normalizado);
        }

        /// <summary>
        /// Verifica si un token tiene formato valido (32 caracteres hexadecimales).
        /// </summary>
        /// <param name="token">Token a validar.</param>
        /// <returns>True si el token tiene formato valido.</returns>
        public static bool EsTokenValido(string token)
        {
            string normalizado = NormalizarTexto(token);
            return normalizado != null && TokenRegex.IsMatch(normalizado);
        }

        /// <summary>
        /// Verifica si un codigo de verificacion tiene formato valido (6 digitos).
        /// </summary>
        /// <param name="codigo">Codigo de verificacion a validar.</param>
        /// <returns>True si el codigo tiene formato valido.</returns>
        public static bool EsCodigoVerificacionValido(string codigo)
        {
            string normalizado = NormalizarTexto(codigo);
            if (normalizado == null || normalizado.Length != LongitudCodigoVerificacion)
            {
                return false;
            }

            return normalizado.All(char.IsDigit);
        }

        /// <summary>
        /// Valida todos los campos de una nueva cuenta.
        /// Verifica usuario, nombre, apellido, correo, contrasena y avatar cumplan con los 
        /// requisitos.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la nueva cuenta a validar.</param>
        /// <returns>Resultado de la validacion indicando exito o errores.</returns>
        public static ResultadoOperacionDTO ValidarNuevaCuenta(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                return CrearResultadoOperacion(false, MensajesError.Cliente.DatosInvalidos);
            }

            var validaciones = new Func<ResultadoOperacionDTO>[]
            {
                () => ValidarYAsignarCampo(
                    nuevaCuenta.Usuario,
                    EsLongitudValida,
                    MensajesError.Cliente.UsuarioRegistroInvalido,
                    s => nuevaCuenta.Usuario = s),

                () => ValidarYAsignarCampo(
                    nuevaCuenta.Nombre,
                    EsLongitudValida,
                    MensajesError.Cliente.NombreRegistroInvalido,
                    s => nuevaCuenta.Nombre = s),

                () => ValidarYAsignarCampo(
                    nuevaCuenta.Apellido,
                    EsLongitudValida,
                    MensajesError.Cliente.ApellidoRegistroInvalido,
                    s => nuevaCuenta.Apellido = s),

                () => ValidarYAsignarCampo(
                    nuevaCuenta.Correo,
                    EsCorreoValido,
                    MensajesError.Cliente.CorreoRegistroInvalido,
                    s => nuevaCuenta.Correo = s),

                () => ValidarYAsignarCampo(
                    nuevaCuenta.Contrasena,
                    EsContrasenaValida,
                    MensajesError.Cliente.ContrasenaRegistroInvalida,
                    s => nuevaCuenta.Contrasena = s)
            };

            return EjecutarValidaciones(validaciones);
        }

        public static ResultadoOperacionDTO ValidarActualizacionPerfil(
            ActualizacionPerfilDTO solicitud)
        {
            if (solicitud == null || solicitud.UsuarioId <= 0)
            {
                return CrearResultadoOperacion(false, MensajesError.Cliente.DatosInvalidos);
            }

            if (solicitud.AvatarId <= 0)
            {
                return CrearResultadoOperacion(false, MensajesError.Cliente.AvatarInvalido);
            }

            var resultadoDatos = ValidarDatosPersonalesPerfil(solicitud);
            if (!resultadoDatos.OperacionExitosa)
            {
                return resultadoDatos;
            }

            return ValidarRedesSociales(solicitud);
        }

        private static ResultadoOperacionDTO ValidarDatosPersonalesPerfil(
            ActualizacionPerfilDTO solicitud)
        {
            var validaciones = new Func<ResultadoOperacionDTO>[]
            {
                () => ValidarYAsignarCampo(
                    solicitud.Nombre,
                    EsLongitudValida,
                    MensajesError.Cliente.NombreRegistroInvalido,
                    s => solicitud.Nombre = s),

                () => ValidarYAsignarCampo(
                    solicitud.Apellido,
                    EsLongitudValida,
                    MensajesError.Cliente.ApellidoRegistroInvalido,
                    s => solicitud.Apellido = s)
            };

            return EjecutarValidaciones(validaciones);
        }

        private static ResultadoOperacionDTO ValidarRedesSociales(ActualizacionPerfilDTO solicitud)
        {
            var validaciones = new Func<ResultadoOperacionDTO>[]
            {
                () => ValidarYAsignarRed("Instagram", solicitud.Instagram,
                    s => solicitud.Instagram = s),
                () => ValidarYAsignarRed("Facebook", solicitud.Facebook,
                    s => solicitud.Facebook = s),
                () => ValidarYAsignarRed("X", solicitud.X,
                    s => solicitud.X = s),
                () => ValidarYAsignarRed("Discord", solicitud.Discord,
                    s => solicitud.Discord = s)
            };

            return EjecutarValidaciones(validaciones);
        }

        /// <summary>
        /// Itera sobre una lista de funciones de validacion y retorna el primer error encontrado
        /// o exito si todas pasan.
        /// </summary>
        private static ResultadoOperacionDTO EjecutarValidaciones(
            Func<ResultadoOperacionDTO>[] validaciones)
        {
            foreach (var validacion in validaciones)
            {
                var resultado = validacion();
                if (!resultado.OperacionExitosa)
                {
                    return resultado;
                }
            }
            return CrearResultadoOperacion(true);
        }

        private static ResultadoOperacionDTO ValidarYAsignarCampo(
            string valor,
            Func<string, bool> reglaValidacion,
            string mensajeError,
            Action<string> asignador)
        {
            var resultado = ValidarCampoObligatorio(valor, reglaValidacion, mensajeError,
                out string normalizado);

            if (resultado.OperacionExitosa)
            {
                asignador(normalizado);
            }
            return resultado;
        }

        private static ResultadoOperacionDTO ValidarYAsignarRed(
            string nombreRed,
            string valor,
            Action<string> asignador)
        {
            var resultado = ValidarRedSocial(nombreRed, valor, out string normalizado);
            if (resultado.OperacionExitosa)
            {
                asignador(normalizado);
            }
            return resultado;
        }

        private static ResultadoOperacionDTO ValidarCampoObligatorio(
            string campo,
            Func<string, bool> regla,
            string mensajeError,
            out string campoNormalizado)
        {
            campoNormalizado = NormalizarTexto(campo);
            if (!regla(campoNormalizado))
            {
                campoNormalizado = null;
                return CrearResultadoOperacion(false, mensajeError);
            }

            return CrearResultadoOperacion(true);
        }

        private static ResultadoOperacionDTO ValidarRedSocial(
            string nombre,
            string valor,
            out string valorNormalizado)
        {
            valorNormalizado = NormalizarTexto(valor);
            if (valorNormalizado == null)
            {
                return CrearResultadoOperacion(true);
            }

            if (valorNormalizado.Length > LongitudMaximaTexto)
            {
                valorNormalizado = null;
                return CrearResultadoOperacion(
                    false,
                    string.Format("El identificador de {0} no debe exceder {1} caracteres.",
                        nombre, LongitudMaximaTexto));
            }

            return CrearResultadoOperacion(true);
        }

        private static ResultadoOperacionDTO CrearResultadoOperacion(bool exitoso,
            string mensaje = null)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = exitoso,
                Mensaje = mensaje
            };
        }
    }
}