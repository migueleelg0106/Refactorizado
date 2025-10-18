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
            return contexto.Usuario.Any(u => u.Nombre_Usuario == nombreUsuario);
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
    }
}
