using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using Datos.Modelo;
using System;
using System.Linq;
using log4net;
using BCryptNet = BCrypt.Net.BCrypt;
using System.Data;
using System.Data.Entity.Core;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Implementacion del servicio de autenticacion de usuarios.
    /// Valida credenciales comparando identificador (usuario o correo) y contrasena con hash BCrypt.
    /// Verifica que el identificador y contrasena sean validos antes de buscar el usuario.
    /// </summary>
    public class InicioSesionManejador : IInicioSesionManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(InicioSesionManejador));
        private readonly IContextoFactory _contextoFactory;

        public InicioSesionManejador() : this(new ContextoFactory())
        {
        }

        public InicioSesionManejador(IContextoFactory contextoFactory)
        {
            _contextoFactory = contextoFactory ?? throw new ArgumentNullException(nameof(contextoFactory));
        }

        /// <summary>
        /// Inicia sesion de un usuario validando sus credenciales.
        /// Busca el usuario por nombre de usuario o correo, verifica la contrasena con BCrypt,
        /// y retorna los datos del usuario si es exitoso.
        /// </summary>
        /// <param name="credenciales">Credenciales de inicio de sesion del usuario.</param>
        /// <returns>Resultado del inicio de sesion con datos del usuario si es exitoso.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si credenciales es null.</exception>
        /// <exception cref="EntityException">Se lanza si hay errores de conexion con la base de datos.</exception>
        /// <exception cref="DataException">Se lanza si hay errores de datos durante el inicio de sesion.</exception>
        /// <exception cref="InvalidOperationException">Se lanza si hay operaciones invalidas durante el inicio de sesion.</exception>
        public ResultadoInicioSesionDTO IniciarSesion(CredencialesInicioSesionDTO credenciales)
        {
            if (credenciales == null)
            {
                throw new ArgumentNullException(nameof(credenciales));
            }

            string identificador = EntradaComunValidador.NormalizarTexto(credenciales.Identificador);
            string contrasena = credenciales.Contrasena?.Trim();

            if (!EntradaComunValidador.EsLongitudValida(identificador) || string.IsNullOrWhiteSpace(contrasena))
            {
                _logger.WarnFormat("Intento de inicio de sesión con datos inválidos. Identificador: {0}", identificador);
                return new ResultadoInicioSesionDTO
                {
                    CuentaEncontrada = true,
                    Mensaje = MensajesError.Cliente.CredencialesInvalidas
                };
            }

            try
            {
                using (var contexto = _contextoFactory.CrearContexto())
                {
                    Usuario usuario = BuscarUsuarioPorIdentificador(contexto, identificador);

                    if (usuario == null)
                    {
                        _logger.WarnFormat("Intento de inicio de sesión fallido. Usuario no encontrado: {0}", identificador);
                        return new ResultadoInicioSesionDTO
                        {
                            CuentaEncontrada = false,
                            Mensaje = MensajesError.Cliente.CredencialesIncorrectas
                        };
                    }

                    if (!BCryptNet.Verify(contrasena, usuario.Contrasena))
                    {
                        _logger.WarnFormat("Intento de inicio de sesión fallido. Contraseña incorrecta para: {0}", usuario.Nombre_Usuario);
                        return new ResultadoInicioSesionDTO
                        {
                            ContrasenaIncorrecta = true,
                            Mensaje = MensajesError.Cliente.CredencialesIncorrectas
                        };
                    }

                    _logger.InfoFormat("Inicio de sesión exitoso. Usuario: {0}, ID: {1}", usuario.Nombre_Usuario, usuario.idUsuario);

                    return new ResultadoInicioSesionDTO
                    {
                        InicioSesionExitoso = true,
                        Usuario = MapearUsuario(usuario)
                    };
                }
            }
            catch (EntityException ex)
            {
                _logger.Error("Error de base de datos durante el inicio de sesión. Fallo en la consulta de usuario.", ex);
                return new ResultadoInicioSesionDTO
                {
                    InicioSesionExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorInicioSesion
                };
            }
            catch (DataException ex)
            {
                _logger.Error("Error de datos durante el inicio de sesión. Los datos del usuario no se pudieron recuperar.", ex);
                return new ResultadoInicioSesionDTO
                {
                    InicioSesionExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorInicioSesion
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Operación inválida durante el inicio de sesión. Estado inconsistente del contexto.", ex);
                return new ResultadoInicioSesionDTO
                {
                    InicioSesionExitoso = false,
                    Mensaje = MensajesError.Cliente.ErrorInicioSesion
                };
            }
        }

        private static Usuario BuscarUsuarioPorIdentificador(BaseDatosPruebaEntities contexto, string identificador)
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