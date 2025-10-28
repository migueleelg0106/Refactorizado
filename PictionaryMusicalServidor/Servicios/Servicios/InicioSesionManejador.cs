using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using Datos.Modelo;
using Datos.Utilidades;
using System;
using System.Data.Entity;
using System.Linq;
using log4net;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Servicios.Servicios
{
    public class InicioSesionManejador : IInicioSesionManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(InicioSesionManejador));

        public ResultadoInicioSesionDTO IniciarSesion(CredencialesInicioSesionDTO credenciales)
        {
            if (credenciales == null)
            {
                throw new ArgumentNullException(nameof(credenciales));
            }

            string identificador = credenciales.Identificador?.Trim();
            string contrasena = credenciales.Contrasena ?? string.Empty;

            if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrEmpty(contrasena))
            {
                return new ResultadoInicioSesionDTO
                {
                    CuentaEncontrada = true,
                    Mensaje = "Credenciales inválidas"
                };
            }

            try
            {
                using (var contexto = CrearContexto())
                {
                    Usuario usuario = BuscarUsuarioPorIdentificador(contexto, identificador);

                    if (usuario == null)
                    {
                        return new ResultadoInicioSesionDTO
                        {
                            CuentaEncontrada = true,
                            Mensaje = null
                        };
                    }

                    if (!BCryptNet.Verify(contrasena, usuario.Contrasena))
                    {
                        return new ResultadoInicioSesionDTO
                        {
                            ContrasenaIncorrecta = true,
                            Mensaje = "Credenciales incorrectas."
                        };
                    }

                    return new ResultadoInicioSesionDTO
                    {
                        InicioSesionExitoso = true,
                        Usuario = MapearUsuario(usuario)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error al iniciar sesión", ex);
                return new ResultadoInicioSesionDTO
                {
                    InicioSesionExitoso = false,
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

        private static Usuario BuscarUsuarioPorIdentificador(BaseDatosPruebaEntities1 contexto, string identificador)
        {
            var usuariosPorNombre = contexto.Usuario
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
                .Include(u => u.Jugador.Avatar)
                .Where(u => u.Jugador.Correo == identificador)
                .ToList();

            return usuariosPorCorreo
                .FirstOrDefault(u => string.Equals(u.Jugador?.Correo, identificador, StringComparison.Ordinal));
        }

        private static UsuarioDTO MapearUsuario(Usuario usuario)
        {
            Jugador jugador = usuario.Jugador;

            return new UsuarioDTO
            {
                UsuarioId = usuario.idUsuario,
                JugadorId = jugador?.idJugador ?? 0,
                NombreUsuario = usuario.Nombre_Usuario,
                Nombre = jugador?.Nombre,
                Apellido = jugador?.Apellido,
                Correo = jugador?.Correo,
                AvatarId = jugador?.Avatar_idAvatar ?? 0,
                AvatarRutaRelativa = jugador?.Avatar?.Avatar_Ruta
            };
        }
    }
}