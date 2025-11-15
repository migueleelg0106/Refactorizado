using System;
using System.Collections.Concurrent;
using System.Linq;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Servicios.Servicios
{

    internal static class ServicioVerificacionRegistro
    {
        private const int MinutosExpiracionCodigo = 5;

        private static readonly ConcurrentDictionary<string, SolicitudCodigoPendiente> _solicitudes =
            new ConcurrentDictionary<string, SolicitudCodigoPendiente>();

        private static readonly ConcurrentDictionary<string, byte> _verificacionesConfirmadas =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        public static ResultadoSolicitudCodigoDTO SolicitarCodigo(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                throw new ArgumentNullException(nameof(nuevaCuenta));
            }

            using (var contexto = CrearContexto())
            {
                bool usuarioRegistrado = contexto.Usuario.Any(u => u.Nombre_Usuario == nuevaCuenta.Usuario);
                bool correoRegistrado = contexto.Jugador.Any(j => j.Correo == nuevaCuenta.Correo);

                if (usuarioRegistrado || correoRegistrado)
                {
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

            bool enviado = ServicioNotificacionCodigos.EnviarNotificacion(datosCuenta.Correo, codigo, datosCuenta.Usuario);
            if (!enviado)
            {
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

            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = true,
                TokenCodigo = token
            };
        }

        public static ResultadoSolicitudCodigoDTO ReenviarCodigo(ReenvioCodigoVerificacionDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            if (!_solicitudes.TryGetValue(solicitud.TokenCodigo, out SolicitudCodigoPendiente existente))
            {
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

            bool enviado = ServicioNotificacionCodigos.EnviarNotificacion(existente.DatosCuenta.Correo, nuevoCodigo, existente.DatosCuenta.Usuario);
            if (!enviado)
            {
                existente.Codigo = codigoAnterior;
                existente.Expira = expiracionAnterior;

                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigoVerificacion
                };
            }

            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = true,
                TokenCodigo = solicitud.TokenCodigo
            };
        }

        public static ResultadoRegistroCuentaDTO ConfirmarCodigo(ConfirmacionCodigoDTO confirmacion)
        {
            if (confirmacion == null)
            {
                throw new ArgumentNullException(nameof(confirmacion));
            }

            if (!_solicitudes.TryGetValue(confirmacion.TokenCodigo, out SolicitudCodigoPendiente pendiente))
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.SolicitudVerificacionNoEncontrada
                };
            }

            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudes.TryRemove(confirmacion.TokenCodigo, out _);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.CodigoVerificacionExpirado
                };
            }

            if (!string.Equals(pendiente.Codigo, confirmacion.CodigoIngresado, StringComparison.OrdinalIgnoreCase))
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = MensajesError.Cliente.CodigoVerificacionIncorrecto
                };
            }

            _solicitudes.TryRemove(confirmacion.TokenCodigo, out _);

            string clave = ObtenerClave(pendiente.DatosCuenta.Usuario, pendiente.DatosCuenta.Correo);
            _verificacionesConfirmadas[clave] = 0;

            return new ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = true
            };
        }

        public static bool EstaVerificacionConfirmada(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                return false;
            }

            string clave = ObtenerClave(nuevaCuenta.Usuario, nuevaCuenta.Correo);
            return _verificacionesConfirmadas.ContainsKey(clave);
        }

        public static void LimpiarVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                return;
            }

            string clave = ObtenerClave(nuevaCuenta.Usuario, nuevaCuenta.Correo);
            _verificacionesConfirmadas.TryRemove(clave, out _);
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
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
                AvatarId = original.AvatarId
            };
        }

        private static string ObtenerClave(string usuario, string correo)
        {
            return ($"{usuario}|{correo}").ToLowerInvariant();
        }

        private class SolicitudCodigoPendiente
        {
            public NuevaCuentaDTO DatosCuenta { get; set; }
            public string Codigo { get; set; }
            public DateTime Expira { get; set; }
        }
    }
}