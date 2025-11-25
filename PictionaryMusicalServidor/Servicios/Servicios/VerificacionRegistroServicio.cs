using System;
using System.Collections.Concurrent;
using System.Linq;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Servicio interno para la logica de negocio de verificacion de registro de cuentas.
    /// Maneja el almacenamiento temporal de solicitudes de registro, generacion y validacion de codigos de verificacion.
    /// Verifica disponibilidad de usuario y correo antes de enviar codigos.
    /// </summary>
    internal static class VerificacionRegistroServicio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(VerificacionRegistroServicio));
        private static readonly IContextoFactory _contextoFactory = new ContextoFactory();
        private const int MinutosExpiracionCodigo = 5;

        private static readonly ConcurrentDictionary<string, SolicitudCodigoPendiente> _solicitudes =
            new ConcurrentDictionary<string, SolicitudCodigoPendiente>();

        private static readonly ConcurrentDictionary<string, byte> _verificacionesConfirmadas =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Solicita un codigo de verificacion para registrar una nueva cuenta.
        /// Valida datos, verifica disponibilidad, genera codigo con expiracion, lo envia por correo y almacena la solicitud.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la nueva cuenta a registrar.</param>
        /// <returns>Resultado indicando si el codigo fue enviado y posibles conflictos de usuario o correo.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si nuevaCuenta es null.</exception>
        public static ResultadoSolicitudCodigoDTO SolicitarCodigo(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                throw new ArgumentNullException(nameof(nuevaCuenta));
            }

            ResultadoOperacionDTO validacionDatos = EntradaComunValidador.ValidarNuevaCuenta(nuevaCuenta);
            if (!validacionDatos.OperacionExitosa)
            {
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = validacionDatos.Mensaje
                };
            }

            using (var contexto = _contextoFactory.CrearContexto())
            {
                bool usuarioRegistrado = contexto.Usuario.Any(u => u.Nombre_Usuario == nuevaCuenta.Usuario);
                bool correoRegistrado = contexto.Jugador.Any(j => j.Correo == nuevaCuenta.Correo);

                if (usuarioRegistrado || correoRegistrado)
                {
                    _logger.WarnFormat("Intento de registro duplicado. Usuario existe: {0}, Correo existe: {1}", usuarioRegistrado, correoRegistrado);
                    return new ResultadoSolicitudCodigoDTO
                    {
                        CodigoEnviado = false,
                        UsuarioRegistrado = usuarioRegistrado,
                        CorreoRegistrado = correoRegistrado,
                        Mensaje = MensajesError.Cliente.UsuarioOCorreoRegistrado
                    };
                }
            }

            string token = TokenGenerador.GenerarToken();
            string codigo = CodigoVerificacionGenerador.GenerarCodigo();
            NuevaCuentaDTO datosCuenta = CopiarCuenta(nuevaCuenta);

            bool enviado = NotificacionCodigosServicio.EnviarNotificacion(
                datosCuenta.Correo,
                codigo,
                datosCuenta.Usuario,
                datosCuenta.Idioma);
            if (!enviado)
            {
                _logger.ErrorFormat("Error al enviar código de verificación a '{0}'.", datosCuenta.Correo);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorSolicitudVerificacion
                };
            }

            var solicitud = new SolicitudCodigoPendiente
            {
                DatosCuenta = datosCuenta,
                Codigo = codigo,
                Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo)
            };

            _solicitudes[token] = solicitud;
            _logger.InfoFormat("Código de verificación de registro generado para '{0}'.", datosCuenta.Correo);

            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = true,
                TokenCodigo = token
            };
        }

        /// <summary>
        /// Reenvia un codigo de verificacion previamente solicitado para registro.
        /// Valida el token, genera un nuevo codigo con nueva expiracion y lo envia por correo.
        /// </summary>
        /// <param name="solicitud">Datos con el token de la sesion de verificacion.</param>
        /// <returns>Resultado indicando si el codigo fue reenviado exitosamente.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si solicitud es null.</exception>
        public static ResultadoSolicitudCodigoDTO ReenviarCodigo(ReenvioCodigoVerificacionDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            string token = EntradaComunValidador.NormalizarTexto(solicitud.TokenCodigo);
            if (!EntradaComunValidador.EsTokenValido(token))
            {
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosReenvioCodigo
                };
            }

            if (!_solicitudes.TryGetValue(token, out SolicitudCodigoPendiente existente))
            {
                _logger.Warn("Intento de reenvío de código de registro no encontrado o expirado.");
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.SolicitudVerificacionNoEncontrada
                };
            }

            string codigoAnterior = existente.Codigo;
            DateTime expiracionAnterior = existente.Expira;

            string nuevoCodigo = CodigoVerificacionGenerador.GenerarCodigo();
            existente.Codigo = nuevoCodigo;
            existente.Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo);

            bool enviado = NotificacionCodigosServicio.EnviarNotificacion(
                existente.DatosCuenta.Correo,
                nuevoCodigo,
                existente.DatosCuenta.Usuario,
                existente.DatosCuenta.Idioma);
            if (!enviado)
            {
                existente.Codigo = codigoAnterior;
                existente.Expira = expiracionAnterior;
                _logger.ErrorFormat("Error al reenviar código de verificación a '{0}'.", existente.DatosCuenta.Correo);

                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigoVerificacion
                };
            }

            _logger.InfoFormat("Código de verificación de registro reenviado a '{0}'.", existente.DatosCuenta.Correo);

            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = true,
                TokenCodigo = token
            };
        }

        /// <summary>
        /// Confirma el codigo de verificacion ingresado para registro de cuenta.
        /// Valida el token, compara el codigo ingresado con el almacenado y marca la verificacion como confirmada.
        /// </summary>
        /// <param name="confirmacion">Datos con el token y codigo ingresado.</param>
        /// <returns>Resultado indicando si el codigo fue confirmado correctamente.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si confirmacion es null.</exception>
        public static ResultadoRegistroCuentaDTO ConfirmarCodigo(ConfirmacionCodigoDTO confirmacion)
        {
            if (confirmacion == null)
            {
                throw new ArgumentNullException(nameof(confirmacion));
            }

            string token = EntradaComunValidador.NormalizarTexto(confirmacion.TokenCodigo);
            string codigoIngresado = EntradaComunValidador.NormalizarTexto(confirmacion.CodigoIngresado);

            if (!EntradaComunValidador.EsTokenValido(token) ||
                !EntradaComunValidador.EsCodigoVerificacionValido(codigoIngresado))
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.DatosConfirmacionInvalidos
                };
            }

            if (!_solicitudes.TryGetValue(token, out SolicitudCodigoPendiente pendiente))
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.SolicitudVerificacionNoEncontrada
                };
            }

            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudes.TryRemove(token, out _);
                _logger.WarnFormat("Código de verificación expirado para usuario '{0}'.", pendiente.DatosCuenta.Usuario);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.CodigoVerificacionExpirado
                };
            }

            if (!string.Equals(pendiente.Codigo, codigoIngresado, StringComparison.OrdinalIgnoreCase))
            {
                _logger.WarnFormat("Código de verificación incorrecto ingresado para usuario '{0}'.", pendiente.DatosCuenta.Usuario);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.CodigoVerificacionIncorrecto
                };
            }

            _solicitudes.TryRemove(token, out _);

            string clave = ObtenerClave(pendiente.DatosCuenta.Usuario, pendiente.DatosCuenta.Correo);
            _verificacionesConfirmadas[clave] = 0;

            _logger.InfoFormat("Verificación confirmada exitosamente para usuario '{0}'.", pendiente.DatosCuenta.Usuario);

            return new ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = true
            };
        }

        /// <summary>
        /// Verifica si una cuenta tiene una verificacion confirmada pendiente.
        /// Comprueba si existe una confirmacion de verificacion para el usuario y correo especificados.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la cuenta a verificar.</param>
        /// <returns>True si la verificacion esta confirmada, false en caso contrario o si nuevaCuenta es null.</returns>
        public static bool EstaVerificacionConfirmada(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                return false;
            }

            string clave = ObtenerClave(nuevaCuenta.Usuario, nuevaCuenta.Correo);
            return _verificacionesConfirmadas.ContainsKey(clave);
        }

        /// <summary>
        /// Limpia la verificacion confirmada de una cuenta despues de completar el registro.
        /// Elimina la confirmacion almacenada para el usuario y correo especificados.
        /// </summary>
        /// <param name="nuevaCuenta">Datos de la cuenta cuya verificacion se limpiara.</param>
        public static void LimpiarVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                return;
            }

            string clave = ObtenerClave(nuevaCuenta.Usuario, nuevaCuenta.Correo);
            _verificacionesConfirmadas.TryRemove(clave, out _);
        }

        private static NuevaCuentaDTO CopiarCuenta(NuevaCuentaDTO original)
        {
            return new NuevaCuentaDTO
            {
                Usuario = original.Usuario,
                Correo = original.Correo,
                Nombre = original.Nombre,
                Apellido = original.Apellido,
                Contrasena = original.Contrasena,
                AvatarId = original.AvatarId,
                Idioma = original.Idioma
            };
        }

        private static string ObtenerClave(string usuario, string correo)
        {
            return ($"{usuario}|{correo}").ToLowerInvariant();
        }

        private sealed class SolicitudCodigoPendiente
        {
            public NuevaCuentaDTO DatosCuenta { get; set; }
            public string Codigo { get; set; }
            public DateTime Expira { get; set; }
        }
    }
}