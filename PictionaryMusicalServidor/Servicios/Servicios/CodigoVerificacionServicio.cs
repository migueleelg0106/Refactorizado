using System;
using System.Collections.Concurrent;
using System.Linq;
using Datos.Modelo;
using Datos.Utilidades;
using Servicios.Contratos.DTOs;
using Servicios.Servicios.Utilidades;
using System.Data.Entity;

namespace Servicios.Servicios
{
    internal static class CodigoVerificacionServicio
    {
        private const int MinutosExpiracionCodigo = 5;
        private const string MensajeErrorEnvioCodigo = "No fue posible enviar el código de verificación.";

        private static readonly ConcurrentDictionary<string, SolicitudCodigoPendiente> _solicitudes =
            new ConcurrentDictionary<string, SolicitudCodigoPendiente>();

        private static readonly ConcurrentDictionary<string, byte> _verificacionesConfirmadas =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<string, SolicitudRecuperacionPendiente> _solicitudesRecuperacion =
            new ConcurrentDictionary<string, SolicitudRecuperacionPendiente>();

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
                        UsuarioRegistrado = usuarioRegistrado,
                        CorreoRegistrado = correoRegistrado,
                        Mensaje = null
                    };
                }
            }

            string token = TokenGenerador.GenerarToken();
            string codigo = CodigoVerificacionGenerador.GenerarCodigo();
            NuevaCuentaDTO datosCuenta = CopiarCuenta(nuevaCuenta);

            bool enviado = EnviarCorreoVerificacion(datosCuenta, codigo);
            if (!enviado)
            {
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajeErrorEnvioCodigo
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
                    Mensaje = "Solicitud no encontrada"
                };
            }

            string codigoAnterior = existente.Codigo;
            DateTime expiracionAnterior = existente.Expira;

            string nuevoCodigo = CodigoVerificacionGenerador.GenerarCodigo();
            existente.Codigo = nuevoCodigo;
            existente.Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo);

            bool enviado = EnviarCorreoVerificacion(existente.DatosCuenta, nuevoCodigo);
            if (!enviado)
            {
                existente.Codigo = codigoAnterior;
                existente.Expira = expiracionAnterior;

                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajeErrorEnvioCodigo
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
                    Mensaje = "Solicitud no encontrada"
                };
            }

            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudes.TryRemove(confirmacion.TokenCodigo, out _);
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

            _solicitudes.TryRemove(confirmacion.TokenCodigo, out _);

            string clave = ObtenerClave(pendiente.DatosCuenta.Usuario, pendiente.DatosCuenta.Correo);
            _verificacionesConfirmadas[clave] = 0;

            return new ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = true
            };
        }

        public static ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            string identificador = solicitud.Identificador?.Trim();
            if (string.IsNullOrWhiteSpace(identificador))
            {
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CuentaEncontrada = false,
                    CodigoEnviado = false,
                    Mensaje = "Identificador requerido"
                };
            }

            using (var contexto = CrearContexto())
            {
                Usuario usuario = BuscarUsuarioPorIdentificador(contexto, identificador);

                if (usuario == null)
                {
                    return new ResultadoSolicitudRecuperacionDTO
                    {
                        CuentaEncontrada = false,
                        CodigoEnviado = false,
                        Mensaje = null
                    };
                }

                LimpiarSolicitudesRecuperacion(usuario.idUsuario);

                string token = TokenGenerador.GenerarToken();
                string codigo = CodigoVerificacionGenerador.GenerarCodigo();

                var pendiente = new SolicitudRecuperacionPendiente
                {
                    UsuarioId = usuario.idUsuario,
                    Correo = usuario.Jugador?.Correo,
                    NombreUsuario = usuario.Nombre_Usuario,
                    Nombre = usuario.Jugador?.Nombre,
                    Apellido = usuario.Jugador?.Apellido,
                    AvatarRutaRelativa = usuario.Jugador?.Avatar?.Avatar_Ruta,
                    Codigo = codigo,
                    Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo),
                    Confirmado = false
                };

                bool enviado = EnviarCorreoRecuperacion(pendiente, codigo);
                if (!enviado)
                {
                    return new ResultadoSolicitudRecuperacionDTO
                    {
                        CuentaEncontrada = true,
                        CodigoEnviado = false,
                        Mensaje = MensajeErrorEnvioCodigo
                    };
                }

                _solicitudesRecuperacion[token] = pendiente;

                return new ResultadoSolicitudRecuperacionDTO
                {
                    CuentaEncontrada = true,
                    CodigoEnviado = true,
                    CorreoDestino = pendiente.Correo,
                    TokenCodigo = token
                };
            }
        }

        public static ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenvioCodigoDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            if (!_solicitudesRecuperacion.TryGetValue(solicitud.TokenCodigo, out SolicitudRecuperacionPendiente pendiente))
            {
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "Solicitud no encontrada"
                };
            }

            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudesRecuperacion.TryRemove(solicitud.TokenCodigo, out _);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = "Código expirado"
                };
            }

            string codigoAnterior = pendiente.Codigo;
            DateTime expiracionAnterior = pendiente.Expira;
            bool confirmadoAnterior = pendiente.Confirmado;

            string nuevoCodigo = CodigoVerificacionGenerador.GenerarCodigo();
            pendiente.Codigo = nuevoCodigo;
            pendiente.Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo);
            pendiente.Confirmado = false;

            bool enviado = EnviarCorreoRecuperacion(pendiente, nuevoCodigo);
            if (!enviado)
            {
                pendiente.Codigo = codigoAnterior;
                pendiente.Expira = expiracionAnterior;
                pendiente.Confirmado = confirmadoAnterior;

                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajeErrorEnvioCodigo
                };
            }

            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = true,
                TokenCodigo = solicitud.TokenCodigo
            };
        }

        public static ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion)
        {
            if (confirmacion == null)
            {
                throw new ArgumentNullException(nameof(confirmacion));
            }

            if (!_solicitudesRecuperacion.TryGetValue(confirmacion.TokenCodigo, out SolicitudRecuperacionPendiente pendiente))
            {
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "Solicitud no encontrada"
                };
            }

            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudesRecuperacion.TryRemove(confirmacion.TokenCodigo, out _);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "Código expirado"
                };
            }

            if (!string.Equals(pendiente.Codigo, confirmacion.CodigoIngresado, StringComparison.OrdinalIgnoreCase))
            {
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "Código incorrecto"
                };
            }

            pendiente.Confirmado = true;
            pendiente.Codigo = null;
            pendiente.Expira = DateTime.UtcNow.AddMinutes(MinutosExpiracionCodigo);

            return new ResultadoOperacionDTO
            {
                OperacionExitosa = true
            };
        }

        public static ResultadoOperacionDTO ActualizarContrasena(ActualizacionContrasenaDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            if (!_solicitudesRecuperacion.TryGetValue(solicitud.TokenCodigo, out SolicitudRecuperacionPendiente pendiente))
            {
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "Solicitud no encontrada"
                };
            }

            if (!pendiente.Confirmado)
            {
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "Código no confirmado"
                };
            }

            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudesRecuperacion.TryRemove(solicitud.TokenCodigo, out _);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = "Solicitud expirada"
                };
            }

            try
            {
                using (var contexto = CrearContexto())
                {
                    Usuario usuario = contexto.Usuario.FirstOrDefault(u => u.idUsuario == pendiente.UsuarioId);

                    if (usuario == null)
                    {
                        return new ResultadoOperacionDTO
                        {
                            OperacionExitosa = false,
                            Mensaje = "Usuario no encontrado"
                        };
                    }

                    usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(solicitud.NuevaContrasena);
                    contexto.SaveChanges();
                }

                _solicitudesRecuperacion.TryRemove(solicitud.TokenCodigo, out _);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = true
                };
            }
            catch (Exception ex)
            {
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = ex.Message
                };
            }
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

        private static bool EnviarCorreoVerificacion(NuevaCuentaDTO nuevaCuenta, string codigo)
        {
            if (nuevaCuenta == null || string.IsNullOrWhiteSpace(codigo))
            {
                return false;
            }

            try
            {
                var tarea = _notificador?.NotificarAsincrono(nuevaCuenta.Correo, codigo, nuevaCuenta.Usuario);
                if (tarea == null)
                {
                    return false;
                }

                return tarea.GetAwaiter().GetResult();
            }
            catch
            {
                return false;
            }
        }

        private static BaseDatosPruebaEntities1 CrearContexto()
        {
            string conexion = Conexion.ObtenerConexion();
            return string.IsNullOrWhiteSpace(conexion)
                ? new BaseDatosPruebaEntities1()
                : new BaseDatosPruebaEntities1(conexion);
        }

        private static void LimpiarSolicitudesRecuperacion(int usuarioId)
        {
            var registros = _solicitudesRecuperacion
                .Where(pair => pair.Value.UsuarioId == usuarioId)
                .ToList();

            foreach (var registro in registros)
            {
                _solicitudesRecuperacion.TryRemove(registro.Key, out _);
            }
        }

        private static Usuario BuscarUsuarioPorIdentificador(BaseDatosPruebaEntities1 contexto, string identificador)
        {
            var usuariosPorNombre = contexto.Usuario
                .Include(u => u.Jugador)
                .Include(u => u.Jugador.Avatar)
                .Where(u => u.Nombre_Usuario == identificador)
                .ToList();

            Usuario usuario = usuariosPorNombre
                .FirstOrDefault(u => string.Equals(u.Nombre_Usuario, identificador, StringComparison.Ordinal));

            if (usuario != null)
            {
                return usuario;
            }

            var usuariosPorCorreo = contexto.Usuario
                .Include(u => u.Jugador)
                .Include(u => u.Jugador.Avatar)
                .Where(u => u.Jugador.Correo == identificador)
                .ToList();

            return usuariosPorCorreo
                .FirstOrDefault(u => string.Equals(u.Jugador?.Correo, identificador, StringComparison.Ordinal));
        }

        private static bool EnviarCorreoRecuperacion(SolicitudRecuperacionPendiente pendiente, string codigo)
        {
            if (pendiente == null)
            {
                return false;
            }

            var datos = new NuevaCuentaDTO
            {
                Usuario = pendiente.NombreUsuario,
                Nombre = pendiente.Nombre,
                Apellido = pendiente.Apellido,
                Correo = pendiente.Correo,
                Contrasena = string.Empty,
                AvatarRutaRelativa = pendiente.AvatarRutaRelativa
            };

            return EnviarCorreoVerificacion(datos, codigo);
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
                AvatarRutaRelativa = original.AvatarRutaRelativa
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

        private class SolicitudRecuperacionPendiente
        {
            public int UsuarioId { get; set; }

            public string Correo { get; set; }

            public string NombreUsuario { get; set; }

            public string Nombre { get; set; }

            public string Apellido { get; set; }

            public string AvatarRutaRelativa { get; set; }

            public string Codigo { get; set; }

            public DateTime Expira { get; set; }

            public bool Confirmado { get; set; }
        }
    }
}