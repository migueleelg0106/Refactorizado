using Datos.Modelo;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    /// <summary>
    /// Implementacion del repositorio de usuarios para acceso a datos.
    /// Proporciona operaciones de consulta y creacion de usuarios con validaciones y 
    /// comparaciones exactas.
    /// Verifica que el nombre de usuario no este duplicado usando comparacion case-sensitive.
    /// </summary>
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UsuarioRepositorio));
        private readonly BaseDatosPruebaEntities _contexto;

        /// <summary>
        /// Inicializa una nueva instancia del repositorio de usuarios.
        /// </summary>
        /// <param name="contexto">Contexto de base de datos.</param>
        /// <exception cref="ArgumentNullException">Se lanza si contexto es null.</exception>
        public UsuarioRepositorio(BaseDatosPruebaEntities contexto)
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

            try
            {
                string nombreNormalizado = nombreUsuario.Trim();
                var usuario = _contexto.Usuario.FirstOrDefault(usuarioEntidad =>
                    usuarioEntidad.Nombre_Usuario == nombreNormalizado);

                return usuario != null
                    && string.Equals(usuario.Nombre_Usuario, nombreNormalizado,
                    StringComparison.Ordinal);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat("Error al verificar existencia del usuario '{0}'.",
                    nombreUsuario, excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat("Error al verificar existencia del usuario '{0}'.",
                    nombreUsuario, excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat("Error al verificar existencia del usuario '{0}'.",
                    nombreUsuario, excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat("Error al verificar existencia del usuario '{0}'.",
                    nombreUsuario, excepcion);
                throw;
            }
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
                var excepcion = new ArgumentNullException(nameof(usuario));
                _logger.Error("Intento de crear un usuario nulo.", excepcion);
                throw excepcion;
            }

            try
            {
                var entidad = _contexto.Usuario.Add(usuario);
                _contexto.SaveChanges();

                return entidad;
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al guardar el nuevo usuario '{0}' en la base de datos.",
                    usuario.Nombre_Usuario, excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al guardar el nuevo usuario '{0}' en la base de datos.",
                    usuario.Nombre_Usuario, excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al guardar el nuevo usuario '{0}' en la base de datos.",
                    usuario.Nombre_Usuario, excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat(
                    "Error al guardar el nuevo usuario '{0}' en la base de datos.",
                    usuario.Nombre_Usuario, excepcion);
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario.
        /// Usa comparacion exacta (case-sensitive) para buscar el usuario.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a buscar.</param>
        /// <returns>Usuario encontrado o null si no existe.</returns>
        /// <exception cref="ArgumentException">Se lanza si nombreUsuario es null o vacio.
        /// </exception>
        public Usuario ObtenerPorNombreUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                var excepcion = new ArgumentException("El nombre de usuario es obligatorio.",
                    nameof(nombreUsuario));
                _logger.Error("Intento de busqueda de usuario con nombre vacio o nulo.", excepcion);
                throw excepcion;
            }

            try
            {
                string nombreNormalizado = nombreUsuario.Trim();
                var usuario = _contexto.Usuario.FirstOrDefault(usuarioEntidad =>
                    usuarioEntidad.Nombre_Usuario == nombreNormalizado);

                if (usuario != null && string.Equals(usuario.Nombre_Usuario, nombreNormalizado,
                    StringComparison.Ordinal))
                {
                    return usuario;
                }

                throw new KeyNotFoundException(
                    $"El usuario '{nombreUsuario}' no existe en la base de datos.");
            }
            catch (KeyNotFoundException excepcion)
            {
                _logger.WarnFormat(
                    "El usuario '{0}' no fue encontrado en la base de datos.",
                    nombreUsuario,
                    excepcion);
                throw;
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener el usuario '{0}' de la base de datos.", nombreUsuario, excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener el usuario '{0}' de la base de datos.", nombreUsuario, excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener el usuario '{0}' de la base de datos.", nombreUsuario, excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener el usuario '{0}' de la base de datos.", nombreUsuario, excepcion);
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario buscando por el correo electronico de su jugador asociado.
        /// Realiza una busqueda exacta (case-sensitive) sobre el correo.
        /// </summary>
        /// <param name="correo">Correo electronico a buscar.</param>
        /// <returns>Usuario encontrado o null si no existe.</returns>
        /// <exception cref="ArgumentException">Se lanza si el correo es nulo o vacio.</exception>
        public Usuario ObtenerPorCorreo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                var excepcion = new ArgumentException("El correo es obligatorio.", nameof(correo));
                _logger.Error("Intento de busqueda de usuario con correo vacio.", excepcion);
                throw excepcion;
            }

            try
            {
                var usuariosCandidatos = _contexto.Usuario
                    .Include("Jugador")
                    .Where(usuarioEntidad => usuarioEntidad.Jugador.Correo == correo)
                    .ToList();

                return usuariosCandidatos.FirstOrDefault(u =>
                    string.Equals(u.Jugador?.Correo, correo, StringComparison.Ordinal));
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener usuario por correo '{0}'.", correo, excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener usuario por correo '{0}'.", correo, excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener usuario por correo '{0}'.", correo, excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener usuario por correo '{0}'.", correo, excepcion);
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario de forma asincrona buscando por el correo electronico de su jugador
        /// asociado.
        /// </summary>
        /// <param name="correo">Correo electronico a buscar.</param>
        /// <returns>Una tarea que representa la operacion asincrona. El resultado contiene el
        /// usuario encontrado o null si no existe.</returns>
        /// <exception cref="Exception">Se lanza si ocurre un error durante la consulta a la base
        /// de datos.</exception>
        public async Task<Usuario> ObtenerPorCorreoAsync(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                throw new ArgumentException(
                    "El correo electronico es obligatorio para la busqueda asincrona.",
                    nameof(correo));
            }

            try
            {
                return await _contexto.Usuario
                    .Include(usuarioEntidad => usuarioEntidad.Jugador)
                    .FirstOrDefaultAsync(usuarioEntidad => usuarioEntidad.Jugador.Correo == correo);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat(
                    "Error asincrono al obtener usuario por correo '{0}'.",
                    correo,
                    excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat(
                    "Error asincrono al obtener usuario por correo '{0}'.",
                    correo,
                    excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat(
                    "Error asincrono al obtener usuario por correo '{0}'.",
                    correo,
                    excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat(
                    "Error asincrono al obtener usuario por correo '{0}'.",
                    correo,
                    excepcion);
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario por su identificador, incluyendo la carga explicita de datos del
        /// jugador y sus redes sociales asociadas.
        /// </summary>
        /// <param name="idUsuario">Identificador unico del usuario.</param>
        /// <returns>El usuario encontrado con sus relaciones cargadas o null si no existe.
        /// </returns>
        /// <exception cref="Exception">Se lanza si ocurre un error durante la consulta a la base
        /// de datos.</exception>
        public Usuario ObtenerPorIdConRedesSociales(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idUsuario),
                    "El identificador del usuario debe ser mayor que cero.");
            }

            try
            {
                return _contexto.Usuario
                    .Include(usuarioEntidad => usuarioEntidad.Jugador.RedSocial)
                    .FirstOrDefault(usuarioEntidad => usuarioEntidad.idUsuario == idUsuario);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener usuario con redes sociales ID: {0}",
                    idUsuario,
                    excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener usuario con redes sociales ID: {0}",
                    idUsuario,
                    excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener usuario con redes sociales ID: {0}",
                    idUsuario,
                    excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener usuario con redes sociales ID: {0}",
                    idUsuario,
                    excepcion);
                throw;
            }
        }

        /// <summary>
        /// Actualiza la contrasena de un usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador unico del usuario.</param>
        /// <param name="nuevaContrasenaHash">Nueva contrasena hasheada.</param>
        /// <exception cref="Exception">Se lanza si ocurre un error durante la actualizacion 
        /// a la base de datos.</exception>
        public void ActualizarContrasena(int usuarioId, string nuevaContrasenaHash)
        {
            try
            {
                var usuario = _contexto.Usuario.FirstOrDefault(usuarioEntidad =>
                    usuarioEntidad.idUsuario == usuarioId);
                if (usuario != null)
                {
                    usuario.Contrasena = nuevaContrasenaHash;
                    _contexto.SaveChanges();
                }
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat("Error al actualizar contrasena del usuario ID {0}.",
                    usuarioId, excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat("Error al actualizar contrasena del usuario ID {0}.",
                    usuarioId, excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat("Error al actualizar contrasena del usuario ID {0}.",
                    usuarioId, excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat("Error al actualizar contrasena del usuario ID {0}.",
                    usuarioId, excepcion);
                throw;
            }
        }

        /// <summary>
        /// Busca los datos de un Jugador con el Nombre del Usuario.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del Usuario.</param>
        /// <exception cref="Exception">Se lanza si ocurre un error durante la consulta
        /// a la base de datos.</exception>
        public Usuario ObtenerPorNombreConJugador(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new ArgumentException(
                    "El nombre de usuario es obligatorio para la busqueda con jugador.",
                    nameof(nombreUsuario));
            }

            try
            {
                string nombreNormalizado = nombreUsuario.Trim();

                return _contexto.Usuario
                    .Include(usuarioEntidad => usuarioEntidad.Jugador)
                    .FirstOrDefault(usuarioEntidad => usuarioEntidad.Nombre_Usuario == nombreNormalizado);
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener usuario con jugador por nombre '{0}'.",
                    nombreUsuario,
                    excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener usuario con jugador por nombre '{0}'.",
                    nombreUsuario,
                    excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener usuario con jugador por nombre '{0}'.",
                    nombreUsuario,
                    excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener usuario con jugador por nombre '{0}'.",
                    nombreUsuario,
                    excepcion);
                throw;
            }
        }
    }
}
