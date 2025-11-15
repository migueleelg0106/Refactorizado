using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using Datos.Modelo;
using Datos.Utilidades;
using System;
using System.Data.Entity;
using System.Linq;
using log4net;
using BCryptNet = BCrypt.Net.BCrypt;
using System.Data;
using System.Data.Entity.Core;
using Servicios.Servicios.Constantes;

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
                            CuentaEncontrada = false,
                            Mensaje = "Credenciales incorrectas."
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
            catch (EntityException ex)
            {
                _logger.Error(MensajesError.Log.InicioSesionErrorBD, ex);
                return new ResultadoInicioSesionDTO
                {
                    InicioSesionExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorInicioSesion
                };
            }
            catch (DataException ex)
            {
                _logger.Error(MensajesError.Log.InicioSesionErrorDatos, ex);
                return new ResultadoInicioSesionDTO
                {
                    InicioSesionExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorInicioSesion
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.InicioSesionOperacionInvalida, ex);
                return new ResultadoInicioSesionDTO
                {
                    InicioSesionExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorInicioSesion
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
                .Where(u => u.Nombre_Usuario == identificador)
                .ToList();

            Usuario usuario = usuariosPorNombre
                .FirstOrDefault(u => string.Equals(u.Nombre_Usuario, identificador, StringComparison.Ordinal));

            if (usuario != null)
            {
                return usuario;
            }

            var usuariosPorCorreo = contexto.Usuario
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
                AvatarId = jugador?.Id_Avatar ?? 0,
            };
        }
    }
}