using System;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.Modelo;
using log4net;

namespace PictionaryMusicalCliente.Sesiones
{
    /// <summary>
    /// Gestiona el acceso global a la sesion del usuario autenticado.
    /// </summary>
    public sealed class SesionUsuarioActual
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Lazy<SesionUsuarioActual> _instancia =
            new(() => new SesionUsuarioActual());

        private SesionUsuarioActual()
        {
        }

        /// <summary>
        /// Obtiene la instancia unica del administrador de sesion.
        /// </summary>
        public static SesionUsuarioActual Instancia => _instancia.Value;

        /// <summary>
        /// Obtiene el usuario actualmente autenticado en la sesión.
        /// </summary>
        public static UsuarioAutenticado Usuario => UsuarioAutenticado.Instancia;

        /// <summary>
        /// Indica si hay un usuario autenticado.
        /// </summary>
        public static bool EstaAutenticado => Usuario != null && Usuario.IdUsuario > 0;

        /// <summary>
        /// Establece los datos del usuario autenticado a partir de un DTO.
        /// </summary>
        /// <param name="usuarioDto">DTO recibido desde el servidor.</param>
        public static void EstablecerUsuario(UsuarioDTO usuarioDto)
        {
            if (usuarioDto == null)
            {
                throw new ArgumentNullException(nameof(usuarioDto));
            }

            Usuario.CargarDesdeDTO(usuarioDto);
            _logger.InfoFormat("Sesión establecida para usuario ID: {0}, Username: {1}", 
                usuarioDto.UsuarioId, usuarioDto.NombreUsuario);
        }

        /// <summary>
        /// Cierra la sesión actual del usuario.
        /// </summary>
        public static void CerrarSesion()
        {
            if (EstaAutenticado)
            {
                _logger.InfoFormat("Cerrando sesión de usuario: {0}", 
                    Usuario.NombreUsuario);
            }
            Usuario.Limpiar();
        }
    }
}