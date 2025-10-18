using System;
using System.Collections.Concurrent;
using System.Linq;
using Datos.Modelo;
using Datos.Utilidades;
using Servicios.Contratos.DTOs;
using Servicios.Servicios.Utilidades;

namespace Servicios.Servicios
{
    internal static class CodigoVerificacionServicio
    {
        private const int MinutosExpiracionCodigo = 10;

        private static readonly ConcurrentDictionary<string, SolicitudCodigoPendiente> Solicitudes =
            new ConcurrentDictionary<string, SolicitudCodigoPendiente>();

        private static readonly ConcurrentDictionary<string, byte> VerificacionesConfirmadas =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        private static ICodigoVerificacionNotificador _notificador = new CorreoCodigoVerificacionNotificador();

        public static void ConfigurarNotificador(ICodigoVerificacionNotificador notificador)
        {
            _notificador = notificador ?? new CorreoCodigoVerificacionNotificador();
        }

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
                        UsuarioYaRegistrado = usuarioRegistrado,
                        CorreoYaRegistrado = correoRegistrado,
                        Mensaje = usuarioRegistrado || correoRegistrado
                            ? "Usuario o correo ya registrados"
                            : null
                    };
                }
            }

            string token = TokenGenerator.GenerarToken();
            string codigo = CodigoVerificacionGenerator.GenerarCodigo();

            var solicitud = new SolicitudCodigoPendiente
            {
                DatosCuenta = CopiarCuenta(nuevaCuenta),
                Codigo = codigo,
                Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo)
            };

            Solicitudes[token] = solicitud;

            EnviarCorreoVerificacionAsync(nuevaCuenta, codigo);

            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = true,
                TokenCodigo = token
            };
        }

        public static ResultadoSolicitudCodigoDTO ReenviarCodigo(ReenviarCodigoVerificacionDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            if (!Solicitudes.TryGetValue(solicitud.TokenCodigo, out SolicitudCodigoPendiente existente))
            {
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "Solicitud no encontrada"
                };
            }

            string nuevoCodigo = CodigoVerificacionGenerator.GenerarCodigo();
            existente.Codigo = nuevoCodigo;
            existente.Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo);

            EnviarCorreoVerificacionAsync(existente.DatosCuenta, nuevoCodigo);

            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = true,
                TokenCodigo = solicitud.TokenCodigo
            };
        }

        public static ResultadoRegistroCuentaDTO ConfirmarCodigo(ConfirmarCodigoDTO confirmacion)
        {
            if (confirmacion == null)
            {
                throw new ArgumentNullException(nameof(confirmacion));
            }

            if (!Solicitudes.TryGetValue(confirmacion.TokenCodigo, out SolicitudCodigoPendiente pendiente))
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = "Solicitud no encontrada"
                };
            }

            if (pendiente.Expira < DateTime.UtcNow)
            {
                Solicitudes.TryRemove(confirmacion.TokenCodigo, out _);
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = "Código expirado"
                };
            }

            if (!string.Equals(pendiente.Codigo, confirmacion.CodigoIngresado, StringComparison.OrdinalIgnoreCase))
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = "Código incorrecto"
                };
            }

            Solicitudes.TryRemove(confirmacion.TokenCodigo, out _);

            string clave = ObtenerClave(pendiente.DatosCuenta.Usuario, pendiente.DatosCuenta.Correo);
            VerificacionesConfirmadas[clave] = 0;

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
            return VerificacionesConfirmadas.ContainsKey(clave);
        }

        public static void LimpiarVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                return;
            }

            string clave = ObtenerClave(nuevaCuenta.Usuario, nuevaCuenta.Correo);
            VerificacionesConfirmadas.TryRemove(clave, out _);
        }

        private static void EnviarCorreoVerificacionAsync(NuevaCuentaDTO nuevaCuenta, string codigo)
        {
            try
            {
                _notificador?.NotificarAsync(nuevaCuenta.Correo, codigo, nuevaCuenta.Usuario).Wait();
            }
            catch
            {
                // Si el envío falla, dejamos que la verificación continúe para permitir
                // que el cliente informe del error usando el mensaje de resultado.
            }
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
