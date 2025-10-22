using System;
using Servicios.Contratos.DTOs;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Sesiones
{
    public sealed class SesionUsuarioActual
    {
        private static readonly Lazy<SesionUsuarioActual> _instancia =
            new(() => new SesionUsuarioActual());

        private SesionUsuarioActual() { }

        public static SesionUsuarioActual Instancia => _instancia.Value;

        /// <summary>
        /// Obtiene el usuario actualmente autenticado en la sesión.
        /// </summary>
        public UsuarioAutenticado Usuario => UsuarioAutenticado.Instancia;

        /// <summary>
        /// Indica si hay un usuario autenticado.
        /// </summary>
        public bool EstaAutenticado => Usuario != null && Usuario.IdUsuario > 0;

        /// <summary>
        /// Establece los datos del usuario autenticado a partir de un DTO.
        /// </summary>
        /// <param name="usuarioDto">DTO recibido desde el servidor.</param>
        public void EstablecerUsuario(UsuarioDTO usuarioDto)
        {
            if (usuarioDto == null)
                throw new ArgumentNullException(nameof(usuarioDto));

            Usuario.CargarDesdeDTO(usuarioDto);
        }

        /// <summary>
        /// Cierra la sesión actual del usuario.
        /// </summary>
        public void CerrarSesion()
        {
            Usuario.Limpiar();
        }
    }
}
