using System;
using System.Linq;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    /// <summary>
    /// Implementacion del repositorio de usuarios para acceso a datos.
    /// Proporciona operaciones de consulta y creacion de usuarios con validaciones y comparaciones exactas.
    /// Verifica que el nombre de usuario no este duplicado usando comparacion case-sensitive.
    /// </summary>
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly BaseDatosPruebaEntities1 _contexto;

        /// <summary>
        /// Inicializa una nueva instancia del repositorio de usuarios.
        /// </summary>
        /// <param name="contexto">Contexto de base de datos.</param>
        /// <exception cref="ArgumentNullException">Se lanza si contexto es null.</exception>
        public UsuarioRepositorio(BaseDatosPruebaEntities1 contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        /// <summary>
        /// Verifica si existe un usuario con el nombre de usuario especificado.
        /// Usa comparacion exacta (case-sensitive) para garantizar unicidad.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a verificar.</param>
        /// <returns>True si el nombre de usuario ya existe.</returns>
        public bool ExisteNombreUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return false;
            }

            string nombreNormalizado = nombreUsuario.Trim();

            var usuario = _contexto.Usuario.FirstOrDefault(u => u.Nombre_Usuario == nombreNormalizado);

            return usuario != null
                && string.Equals(usuario.Nombre_Usuario, nombreNormalizado, StringComparison.Ordinal);
        }

        /// <summary>
        /// Crea un nuevo usuario en la base de datos.
        /// Agrega el usuario al contexto y persiste los cambios.
        /// </summary>
        /// <param name="usuario">Entidad de usuario a crear.</param>
        /// <returns>Usuario creado con su identificador asignado.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si usuario es null.</exception>
        public Usuario CrearUsuario(Usuario usuario)
        {
            if (usuario == null)
            {
                throw new ArgumentNullException(nameof(usuario));
            }

            var entidad = _contexto.Usuario.Add(usuario);
            _contexto.SaveChanges();
            return entidad;
        }

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario.
        /// Usa comparacion exacta (case-sensitive) para buscar el usuario.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a buscar.</param>
        /// <returns>Usuario encontrado o null si no existe.</returns>
        /// <exception cref="ArgumentException">Se lanza si nombreUsuario es null o vacio.</exception>
        public Usuario ObtenerPorNombreUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));
            }

            string nombreNormalizado = nombreUsuario.Trim();

            var usuario = _contexto.Usuario.FirstOrDefault(u => u.Nombre_Usuario == nombreNormalizado);

            if (usuario != null && string.Equals(usuario.Nombre_Usuario, nombreNormalizado, StringComparison.Ordinal))
            {
                return usuario;
            }

            return null;
        }
    }
}