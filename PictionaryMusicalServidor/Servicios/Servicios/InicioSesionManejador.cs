using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Datos.Modelo;
using System;
using System.Data.Entity;
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
                return new ResultadoInicioSesionDTO
                {
                    CuentaEncontrada = true,
                    Mensaje = MensajesError.Cliente.CredencialesInvalidas
                };
            }

            try
            {
                using (var contexto = ContextoFactory.CrearContexto())
                {
                    Usuario usuario = BuscarUsuarioPorIdentificador(contexto, identificador);

                    if (usuario == null)
                    {
                        return new ResultadoInicioSesionDTO
                        {
                            CuentaEncontrada = false,
                            Mensaje = MensajesError.Cliente.CredencialesIncorrectas
                        };
                    }

                    if (!BCryptNet.Verify(contrasena, usuario.Contrasena))
                    {
                        return new ResultadoInicioSesionDTO
                        {
                            ContrasenaIncorrecta = true,
                            Mensaje = MensajesError.Cliente.CredencialesIncorrectas
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

        /// <summary>
        /// Busca un usuario por su identificador (nombre de usuario o correo electronico).
        /// Realiza busqueda primero por nombre de usuario y luego por correo si no se encuentra.
        /// Usa comparacion exacta (case-sensitive) para garantizar precision.
        /// </summary>
        /// <param name="contexto">Contexto de base de datos para consultas.</param>
        /// <param name="identificador">Nombre de usuario o correo electronico a buscar.</param>
        /// <returns>Usuario encontrado o null si no existe.</returns>
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

        /// <summary>
        /// Mapea una entidad Usuario a un DTO de usuario.
        /// Incluye datos del jugador asociado y maneja valores null de forma segura.
        /// </summary>
        /// <param name="usuario">Entidad de usuario a mapear.</param>
        /// <returns>DTO con los datos del usuario.</returns>
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