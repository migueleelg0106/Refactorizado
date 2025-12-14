using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Servicio interno para la logica de negocio de verificacion de registro de cuentas.
    /// Maneja el almacenamiento temporal de solicitudes y validacion de codigos de verificacion.
    /// Verifica disponibilidad de usuario y correo antes de enviar codigos.
    /// </summary>
    public class VerificacionRegistroServicio : IVerificacionRegistroServicio
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(VerificacionRegistroServicio));

        private const int MinutosExpiracionCodigo = 5;

        private static readonly ConcurrentDictionary<string, SolicitudCodigoPendiente>
            _solicitudes = new ConcurrentDictionary<string, SolicitudCodigoPendiente>();

        private static readonly ConcurrentDictionary<string, byte> _verificacionesConfirmadas =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        private readonly IContextoFactoria _contextoFactoria;
        private readonly INotificacionCodigosServicio _notificacionCodigosServicio;

        /// <summary>
        /// Constructor con inyeccion de dependencias.
        /// </summary>
        public VerificacionRegistroServicio(IContextoFactoria contextoFactoria,
            INotificacionCodigosServicio notificacionCodigosServicio)
        {
            _contextoFactoria = contextoFactoria ??
                throw new ArgumentNullException(nameof(contextoFactoria));

            _notificacionCodigosServicio = notificacionCodigosServicio ??
                throw new ArgumentNullException(nameof(notificacionCodigosServicio));
        }

        /// <summary>
        /// Solicita un codigo de verificacion para registrar una nueva cuenta.
        /// </summary>
        public ResultadoSolicitudCodigoDTO SolicitarCodigo(NuevaCuentaDTO nuevaCuenta)
        {
            var validacionDatos = ValidarDatosSolicitud(nuevaCuenta);
            if (!validacionDatos.OperacionExitosa)
            {
                return CrearFalloSolicitud(validacionDatos.Mensaje);
            }

            var disponibilidad = VerificarDisponibilidadCuenta(nuevaCuenta);
            if (!disponibilidad.DisponibilidadExitosa)
            {
                return disponibilidad.Resultado;
            }

            var generacion = GenerarYEnviarCodigo(nuevaCuenta);
            if (!generacion.Exito)
            {
                return CrearFalloSolicitud(MensajesError.Cliente.ErrorSolicitudVerificacion);
            }

            AlmacenarSolicitud(generacion.Token, generacion.Solicitud);

            _logger.Info("Codigo de verificacion de registro generado correctamente.");

            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = true,
                TokenCodigo = generacion.Token
            };
        }

        /// <summary>
        /// Reenvia un codigo de verificacion previamente solicitado para registro.
        /// </summary>
        public ResultadoSolicitudCodigoDTO ReenviarCodigo(ReenvioCodigoVerificacionDTO solicitud)
        {
            if (!ValidarTokenReenvio(solicitud))
            {
                return CrearFalloReenvio(MensajesError.Cliente.DatosReenvioCodigo);
            }

            try
            {
                var pendiente = ObtenerSolicitudPendiente(solicitud.TokenCodigo);
                return ProcesarReenvioCodigo(solicitud.TokenCodigo, pendiente);
            }
            catch (KeyNotFoundException excepcion)
            {
                _logger.Warn(
                    "No se encontro la solicitud de verificacion para reenviar el codigo.",
                    excepcion);
                return CrearFalloReenvio(
                    MensajesError.Cliente.SolicitudVerificacionNoEncontrada);
            }
            catch (Exception excepcion)
            {
                _logger.Error(
                    "Error inesperado al reenviar codigo de verificacion.",
                    excepcion);
                return CrearFalloReenvio(
                    MensajesError.Cliente.SolicitudVerificacionNoEncontrada);
            }
        }

        /// <summary>
        /// Confirma el codigo de verificacion ingresado para registro de cuenta.
        /// </summary>
        public ResultadoRegistroCuentaDTO ConfirmarCodigo(ConfirmacionCodigoDTO confirmacion)
        {
            if (!ValidarDatosConfirmacion(confirmacion))
            {
                return CrearFalloConfirmacion(MensajesError.Cliente.DatosConfirmacionInvalidos);
            }

            try
            {
                var pendiente = ObtenerSolicitudPendiente(confirmacion.TokenCodigo);

                var verificacion = VerificarCodigoIngresado(
                    pendiente,
                    confirmacion.TokenCodigo,
                    confirmacion.CodigoIngresado);

                if (!verificacion.Exito)
                {
                    return CrearFalloConfirmacion(verificacion.MensajeError);
                }

                RegistrarConfirmacion(pendiente);
                _solicitudes.TryRemove(confirmacion.TokenCodigo, out _);

                _logger.Info("Verificacion confirmada exitosamente.");

                return new ResultadoRegistroCuentaDTO { RegistroExitoso = true };
            }
            catch (KeyNotFoundException excepcion)
            {
                _logger.Warn(
                    "No se encontro la solicitud de verificacion al confirmar codigo.",
                    excepcion);
                return CrearFalloConfirmacion(
                    MensajesError.Cliente.SolicitudVerificacionNoEncontrada);
            }
            catch (Exception excepcion)
            {
                _logger.Error(
                    "Error inesperado al confirmar codigo de verificacion.",
                    excepcion);
                return CrearFalloConfirmacion(
                    MensajesError.Cliente.SolicitudVerificacionNoEncontrada);
            }
        }

        /// <summary>
        /// Verifica si una cuenta tiene una verificacion confirmada pendiente.
        /// </summary>
        public bool EstaVerificacionConfirmada(NuevaCuentaDTO nuevaCuenta)
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
        /// </summary>
        public void LimpiarVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                return;
            }

            string clave = ObtenerClave(nuevaCuenta.Usuario, nuevaCuenta.Correo);
            _verificacionesConfirmadas.TryRemove(clave, out _);
        }

        private ResultadoOperacionDTO ValidarDatosSolicitud(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                throw new ArgumentNullException(nameof(nuevaCuenta));
            }
            return EntradaComunValidador.ValidarNuevaCuenta(nuevaCuenta);
        }

        private (bool DisponibilidadExitosa, ResultadoSolicitudCodigoDTO Resultado)
            VerificarDisponibilidadCuenta(NuevaCuentaDTO nuevaCuenta)
        {
            using (var contexto = _contextoFactoria.CrearContexto())
            {
                bool usuarioRegistrado = contexto.Usuario.Any(
                    usuario => usuario.Nombre_Usuario == nuevaCuenta.Usuario);

                bool correoRegistrado = contexto.Jugador.Any(
                    jugador => jugador.Correo == nuevaCuenta.Correo);

                if (usuarioRegistrado || correoRegistrado)
                {
                    _logger.Warn("Registro duplicado intentado (usuario o correo existente).");

                    var resultado = new ResultadoSolicitudCodigoDTO
                    {
                        CodigoEnviado = false,
                        UsuarioRegistrado = usuarioRegistrado,
                        CorreoRegistrado = correoRegistrado,
                        Mensaje = MensajesError.Cliente.UsuarioOCorreoRegistrado
                    };
                    return (false, resultado);
                }
            }
            return (true, null);
        }

        private (bool Exito, string Token, SolicitudCodigoPendiente Solicitud)
            GenerarYEnviarCodigo(NuevaCuentaDTO nuevaCuenta)
        {
            string token = TokenGenerador.GenerarToken();
            string codigo = CodigoVerificacionGenerador.GenerarCodigo();
            NuevaCuentaDTO datosCuenta = CopiarCuenta(nuevaCuenta);

            bool enviado = _notificacionCodigosServicio.EnviarNotificacion(
                datosCuenta.Correo,
                codigo,
                datosCuenta.Usuario,
                datosCuenta.Idioma);

            if (!enviado)
            {
                _logger.Error("Error al enviar codigo de verificacion.");
                return (false, null, null);
            }

            var solicitud = new SolicitudCodigoPendiente
            {
                DatosCuenta = datosCuenta,
                Codigo = codigo,
                Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo)
            };

            return (true, token, solicitud);
        }

        private void AlmacenarSolicitud(string token, SolicitudCodigoPendiente solicitud)
        {
            _solicitudes[token] = solicitud;
        }

        private ResultadoSolicitudCodigoDTO CrearFalloSolicitud(string mensaje)
        {
            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = false,
                Mensaje = mensaje
            };
        }

        private bool ValidarTokenReenvio(ReenvioCodigoVerificacionDTO solicitud)
        {
            if (solicitud == null) return false;
            string token = EntradaComunValidador.NormalizarTexto(solicitud.TokenCodigo);
            return EntradaComunValidador.EsTokenValido(token);
        }

        private SolicitudCodigoPendiente ObtenerSolicitudPendiente(string token)
        {
            if (!_solicitudes.TryGetValue(token, out SolicitudCodigoPendiente existente))
            {
                _logger.Warn("Token no encontrado o expirado en cache.");
                throw new KeyNotFoundException("La solicitud de verificacion no existe.");
            }
            return existente;
        }

        private ResultadoSolicitudCodigoDTO ProcesarReenvioCodigo(
            string token,
            SolicitudCodigoPendiente existente)
        {
            string codigoAnterior = existente.Codigo;
            DateTime expiracionAnterior = existente.Expira;

            string nuevoCodigo = CodigoVerificacionGenerador.GenerarCodigo();
            existente.Codigo = nuevoCodigo;
            existente.Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo);

            bool enviado = _notificacionCodigosServicio.EnviarNotificacion(
                existente.DatosCuenta.Correo,
                nuevoCodigo,
                existente.DatosCuenta.Usuario,
                existente.DatosCuenta.Idioma);

            if (!enviado)
            {
                existente.Codigo = codigoAnterior;
                existente.Expira = expiracionAnterior;

                _logger.Error("Error al reenviar codigo de verificacion.");

                return CrearFalloReenvio(
                    MensajesError.Cliente.ErrorReenviarCodigoVerificacion);
            }

            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = true,
                TokenCodigo = token
            };
        }

        private ResultadoSolicitudCodigoDTO CrearFalloReenvio(string mensaje)
        {
            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = false,
                Mensaje = mensaje
            };
        }

        private bool ValidarDatosConfirmacion(ConfirmacionCodigoDTO confirmacion)
        {
            if (confirmacion == null) return false;
            string token = EntradaComunValidador.NormalizarTexto(confirmacion.TokenCodigo);
            string codigo = EntradaComunValidador.NormalizarTexto(confirmacion.CodigoIngresado);

            return EntradaComunValidador.EsTokenValido(token) &&
                   EntradaComunValidador.EsCodigoVerificacionValido(codigo);
        }

        private (bool Exito, string MensajeError) VerificarCodigoIngresado(
            SolicitudCodigoPendiente pendiente,
            string token,
            string codigoIngresado)
        {
            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudes.TryRemove(token, out _);
                return (false, MensajesError.Cliente.CodigoVerificacionExpirado);
            }

            if (!string.Equals(
                pendiente.Codigo,
                codigoIngresado,
                StringComparison.OrdinalIgnoreCase))
            {
                return (false, MensajesError.Cliente.CodigoVerificacionIncorrecto);
            }

            return (true, null);
        }

        private void RegistrarConfirmacion(SolicitudCodigoPendiente pendiente)
        {
            string clave = ObtenerClave(
                pendiente.DatosCuenta.Usuario,
                pendiente.DatosCuenta.Correo);

            _verificacionesConfirmadas[clave] = 0;
        }

        private ResultadoRegistroCuentaDTO CrearFalloConfirmacion(string mensaje)
        {
            return new ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = false,
                Mensaje = mensaje
            };
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
