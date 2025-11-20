using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using PictionaryMusicalServidor.Datos.Modelo;
using PictionaryMusicalServidor.Datos.Utilidades;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using System.Data.Entity;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using log4net;

namespace PictionaryMusicalServidor.Servicios.Servicios
{

    internal static class ServicioRecuperacionCuenta
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ServicioRecuperacionCuenta));
        private const int MinutosExpiracionCodigo = 5;

        private static readonly ConcurrentDictionary<string, SolicitudRecuperacionPendiente> _solicitudesRecuperacion =
            new ConcurrentDictionary<string, SolicitudRecuperacionPendiente>();

        public static ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud)
        {
            if (solicitud == null)
            {
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CuentaEncontrada = false,
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosInvalidos
                };
            }

            string identificador = EntradaComunValidador.NormalizarTexto(solicitud.Identificador);
            if (!EntradaComunValidador.EsLongitudValida(identificador))
            {
                return new ResultadoSolicitudRecuperacionDTO
                {
                    CuentaEncontrada = false,
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.SolicitudRecuperacionIdentificadorObligatorio
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
                        Mensaje = MensajesError.Cliente.SolicitudRecuperacionCuentaNoEncontrada
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
                        Mensaje = MensajesError.Cliente.ErrorRecuperarCuenta
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
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.DatosInvalidos
                };
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

            if (!_solicitudesRecuperacion.TryGetValue(token, out SolicitudRecuperacionPendiente pendiente))
            {
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.SolicitudRecuperacionNoEncontrada
                };
            }

            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudesRecuperacion.TryRemove(token, out _);
                return new ResultadoSolicitudCodigoDTO
                {
                    CodigoEnviado = false,
                    Mensaje = MensajesError.Cliente.CodigoRecuperacionExpirado
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
                    Mensaje = MensajesError.Cliente.ErrorReenviarCodigoRecuperacion
                };
            }

            return new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = true,
                TokenCodigo = token
            };
        }

        public static ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmacionCodigoDTO confirmacion)
        {
            if (confirmacion == null)
            {
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.DatosInvalidos
                };
            }

            string token = EntradaComunValidador.NormalizarTexto(confirmacion.TokenCodigo);
            string codigoIngresado = EntradaComunValidador.NormalizarTexto(confirmacion.CodigoIngresado);

            if (!EntradaComunValidador.EsTokenValido(token) ||
                !EntradaComunValidador.EsCodigoVerificacionValido(codigoIngresado))
            {
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.DatosConfirmacionInvalidos
                };
            }

            if (!_solicitudesRecuperacion.TryGetValue(token, out SolicitudRecuperacionPendiente pendiente))
            {
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.SolicitudRecuperacionNoEncontrada
                };
            }

            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudesRecuperacion.TryRemove(token, out _);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.CodigoRecuperacionExpirado
                };
            }

            if (!string.Equals(pendiente.Codigo, codigoIngresado, StringComparison.OrdinalIgnoreCase))
            {
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.CodigoRecuperacionIncorrecto
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
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.DatosInvalidos
                };
            }

            string token = EntradaComunValidador.NormalizarTexto(solicitud.TokenCodigo);
            string contrasena = EntradaComunValidador.NormalizarTexto(solicitud.NuevaContrasena);

            if (!EntradaComunValidador.EsTokenValido(token) ||
                !EntradaComunValidador.EsContrasenaValida(contrasena))
            {
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.DatosActualizacionContrasena
                };
            }

            if (!_solicitudesRecuperacion.TryGetValue(token, out SolicitudRecuperacionPendiente pendiente))
            {
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.SolicitudRecuperacionNoEncontrada
                };
            }

            if (!pendiente.Confirmado)
            {
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.SolicitudRecuperacionNoVigente
                };
            }

            if (pendiente.Expira < DateTime.UtcNow)
            {
                _solicitudesRecuperacion.TryRemove(solicitud.TokenCodigo, out _);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.SolicitudRecuperacionInvalida
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
                            Mensaje = MensajesError.Cliente.UsuarioNoEncontrado
                        };
                    }

                    usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(solicitud.NuevaContrasena);
                    contexto.SaveChanges();
                }

                _solicitudesRecuperacion.TryRemove(token, out _);

                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = true
                };
            }
            catch (DbEntityValidationException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionActualizarValidacionEntidad, ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionActualizarActualizacionBD, ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionActualizarErrorBD, ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
                };
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.RecuperacionActualizarErrorDatos, ex);
                return new ResultadoOperacionDTO
                {
                    OperacionExitosa = false,
                    Mensaje = MensajesError.Cliente.ErrorActualizarContrasena
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
                .Where(u => u.Jugador.Correo == identificador)
                .ToList();

            return usuariosPorCorreo
                .FirstOrDefault(u => string.Equals(u.Jugador?.Correo, identificador, StringComparison.Ordinal));
        }

        private sealed class SolicitudRecuperacionPendiente
        {
            public int UsuarioId { get; set; }
            public string Correo { get; set; }
            public string NombreUsuario { get; set; }
            public string Codigo { get; set; }
            public DateTime Expira { get; set; }
            public bool Confirmado { get; set; }
        }
    }
}