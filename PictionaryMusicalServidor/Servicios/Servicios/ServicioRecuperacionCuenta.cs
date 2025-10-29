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
    /// <summary>
    /// Maneja la lógica y el estado en memoria para la recuperación de contraseñas.
    /// </summary>
    internal static class ServicioRecuperacionCuenta
    {
        private const int MinutosExpiracionCodigo = 5;
        private const string MensajeErrorEnvioCodigo = "No fue posible enviar el código de verificación.";

        private static readonly ConcurrentDictionary<string, SolicitudRecuperacionPendiente> _solicitudesRecuperacion =
            new ConcurrentDictionary<string, SolicitudRecuperacionPendiente>();

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

                bool enviado = ServicioNotificacionCodigos.EnviarNotificacion(pendiente.Correo, codigo, pendiente.NombreUsuario);
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

            bool enviado = ServicioNotificacionCodigos.EnviarNotificacion(pendiente.Correo, nuevoCodigo, pendiente.NombreUsuario);
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