using System;
using System.Linq;
using Datos.DAL.Interfaces;
using Datos.Modelo;

namespace Datos.DAL.Implementaciones
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly BaseDatosPruebaEntities1 contexto;

        public UsuarioRepositorio(BaseDatosPruebaEntities1 contexto)
        {
            this.contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public bool ExisteNombreUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return false;
            }

            string nombreNormalizado = nombreUsuario.Trim();

            var usuario = contexto.Usuario.FirstOrDefault(u => u.Nombre_Usuario == nombreNormalizado);

            return usuario != null
                && string.Equals(usuario.Nombre_Usuario, nombreNormalizado, StringComparison.Ordinal);
        }

        public Usuario CrearUsuario(Usuario usuario)
        {
            if (usuario == null)
            {
                throw new ArgumentNullException(nameof(usuario));
            }

            var entidad = contexto.Usuario.Add(usuario);
            contexto.SaveChanges();
            return entidad;
        }

        public Usuario ObtenerPorNombreUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));
            }

            string nombreNormalizado = nombreUsuario.Trim();

            var usuario = contexto.Usuario.FirstOrDefault(u => u.Nombre_Usuario == nombreNormalizado);

            if (usuario != null && string.Equals(usuario.Nombre_Usuario, nombreNormalizado, StringComparison.Ordinal))
            {
                return usuario;
            }

            return null;
        }
    }
}
