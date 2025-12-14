using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    /// <summary>
    /// Repositorio para la gestion de relaciones de amistad, solicitudes y consultas de amigos 
    /// entre usuarios.
    /// </summary>
    public class AmigoRepositorio : IAmigoRepositorio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AmigoRepositorio));
        private readonly BaseDatosPruebaEntities _contexto;

        /// <summary>
        /// Inicializa una nueva instancia del repositorio de amigos.
        /// </summary>
        /// <param name="contexto">Contexto de la base de datos.</param>
        /// <exception cref="ArgumentNullException">Se lanza si el contexto es nulo.</exception>
        public AmigoRepositorio(BaseDatosPruebaEntities contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        /// <summary>
        /// Verifica si existe una relacion (solicitud o amistad) entre dos usuarios.
        /// </summary>
        /// <param name="usuarioAId">ID del primer usuario.</param>
        /// <param name="usuarioBId">ID del segundo usuario.</param>
        /// <returns>True si existe registro en la tabla Amigo, False en caso contrario.</returns>
        public bool ExisteRelacion(int usuarioAId, int usuarioBId)
        {
            try
            {
                return _contexto.Amigo.Any(a =>
                    (a.UsuarioEmisor == usuarioAId && a.UsuarioReceptor == usuarioBId) ||
                    (a.UsuarioEmisor == usuarioBId && a.UsuarioReceptor == usuarioAId));
            }
            catch (DbUpdateException excepcion)
            {
                _logger.Error(
                    "Error al verificar existencia de relacion de amistad en la base de datos.",
                    excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.Error(
                    "Error al verificar existencia de relacion de amistad en la base de datos.",
                    excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.Error(
                    "Error al verificar existencia de relacion de amistad en la base de datos.",
                    excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.Error(
                    "Error al verificar existencia de relacion de amistad en la base de datos.",
                    excepcion);
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva solicitud de amistad entre un emisor y un receptor.
        /// </summary>
        /// <param name="usuarioEmisorId">ID del usuario que envia la solicitud.</param>
        /// <param name="usuarioReceptorId">ID del usuario que recibe la solicitud.</param>
        /// <returns>La entidad Amigo creada.</returns>
        public Amigo CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId)
        {
            var solicitud = new Amigo
            {
                UsuarioEmisor = usuarioEmisorId,
                UsuarioReceptor = usuarioReceptorId,
                Estado = false
            };

            _contexto.Amigo.Add(solicitud);
            _contexto.SaveChanges();

            return solicitud;
        }

        /// <summary>
        /// Obtiene la entidad de relacion de amistad existente entre dos usuarios.
        /// </summary>
        /// <param name="usuarioAId">ID del primer usuario.</param>
        /// <param name="usuarioBId">ID del segundo usuario.</param>
        /// <returns>La entidad Amigo encontrada o null.</returns>
        public Amigo ObtenerRelacion(int usuarioAId, int usuarioBId)
        {
            try
            {
                return _contexto.Amigo.FirstOrDefault(a =>
                    (a.UsuarioEmisor == usuarioAId && a.UsuarioReceptor == usuarioBId) ||
                    (a.UsuarioEmisor == usuarioBId && a.UsuarioReceptor == usuarioAId));
            }
            catch (DbUpdateException excepcion)
            {
                _logger.Error("Error al obtener la relación de amistad de la base de datos.", excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.Error("Error al obtener la relación de amistad de la base de datos.", excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.Error("Error al obtener la relación de amistad de la base de datos.", excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.Error("Error al obtener la relación de amistad de la base de datos.", excepcion);
                throw;
            }
        }

        /// <summary>
        /// Obtiene una lista de las solicitudes de amistad pendientes donde el usuario esta 
        /// involucrado.
        /// </summary>
        /// <param name="usuarioId">ID del usuario a consultar.</param>
        /// <returns>Lista de entidades Amigo pendientes.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Se lanza si el ID de usuario es menor o 
        /// igual a cero.</exception>
        public IList<Amigo> ObtenerSolicitudesPendientes(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                var excepcion = new ArgumentOutOfRangeException(nameof(usuarioId), 
                    "El identificador del usuario debe ser positivo.");
                _logger.Error("Intento de obtener solicitudes con ID invalido.", excepcion);
                throw excepcion;
            }

            try
            {
                return _contexto.Amigo
                    .Where(relacion => !relacion.Estado && (relacion.UsuarioEmisor == usuarioId
                    || relacion.UsuarioReceptor == usuarioId))
                    .Include(relacion => relacion.Usuario)
                    .Include(relacion => relacion.Usuario1)
                    .ToList();
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener solicitudes pendientes para el usuario ID: {0}.",
                    usuarioId, excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener solicitudes pendientes para el usuario ID: {0}.",
                    usuarioId, excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener solicitudes pendientes para el usuario ID: {0}.",
                    usuarioId, excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat(
                    "Error al obtener solicitudes pendientes para el usuario ID: {0}.",
                    usuarioId, excepcion);
                throw;
            }
        }

        /// <summary>
        /// Actualiza el estado de aceptacion de una relacion de amistad.
        /// </summary>
        /// <param name="relacion">Entidad Amigo a modificar.</param>
        /// <param name="estado">Nuevo estado (True para aceptado).</param>
        /// <exception cref="ArgumentNullException">Se lanza si la relacion proporcionada es nula.
        /// </exception>
        public void ActualizarEstado(Amigo relacion, bool estado)
        {
            if (relacion == null)
            {
                var excepcion = new ArgumentNullException(nameof(relacion));
                _logger.Error("Se intento actualizar una relacion nula.", excepcion);
                throw excepcion;
            }

            try
            {
                relacion.Estado = estado;
                _contexto.SaveChanges();
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al actualizar el estado de la relacion entre {0} y {1}.",
                    relacion.UsuarioEmisor, relacion.UsuarioReceptor, excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al actualizar el estado de la relacion entre {0} y {1}.",
                    relacion.UsuarioEmisor, relacion.UsuarioReceptor, excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat(
                    "Error al actualizar el estado de la relacion entre {0} y {1}.",
                    relacion.UsuarioEmisor, relacion.UsuarioReceptor, excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat(
                    "Error al actualizar el estado de la relacion entre {0} y {1}.",
                    relacion.UsuarioEmisor, relacion.UsuarioReceptor, excepcion);
                throw;
            }
        }

        /// <summary>
        /// Elimina permanentemente un registro de relacion de amistad de la base de datos.
        /// </summary>
        /// <param name="relacion">Entidad Amigo a eliminar.</param>
        /// <exception cref="ArgumentNullException">Se lanza si la relacion proporcionada es nula.
        /// </exception>
        public void EliminarRelacion(Amigo relacion)
        {
            if (relacion == null)
            {
                var excepcion = new ArgumentNullException(nameof(relacion));
                _logger.Error("Se intento eliminar una relaciï¿½n nula.", excepcion);
                throw excepcion;
            }

            int emisorId = relacion.UsuarioEmisor;
            int receptorId = relacion.UsuarioReceptor;

            try
            {
                _contexto.Amigo.Remove(relacion);
                _contexto.SaveChanges();
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat("Error al eliminar la relacion de amistad entre {0} y {1}.",
                    emisorId, receptorId, excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat("Error al eliminar la relacion de amistad entre {0} y {1}.",
                    emisorId, receptorId, excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat("Error al eliminar la relacion de amistad entre {0} y {1}.",
                    emisorId, receptorId, excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat("Error al eliminar la relacion de amistad entre {0} y {1}.",
                    emisorId, receptorId, excepcion);
                throw;
            }
        }

        /// <summary>
        /// Obtiene la lista de usuarios que son amigos confirmados del usuario especificado.
        /// </summary>
        /// <param name="usuarioId">ID del usuario a consultar.</param>
        /// <returns>Lista de entidades Usuario que son amigos.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Se lanza si el ID de usuario es menor o 
        /// igual a cero.</exception>
        public IList<Usuario> ObtenerAmigos(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                var excepcion = new ArgumentOutOfRangeException(nameof(usuarioId), 
                    "El identificador del usuario debe ser positivo.");
                _logger.Error("ID de usuario invï¿½lido al obtener lista de amigos.", excepcion);
                throw excepcion;
            }

            try
            {
                var amigosIds = _contexto.Amigo
                    .Where(relacion => relacion.Estado && (relacion.UsuarioEmisor == usuarioId
                        || relacion.UsuarioReceptor == usuarioId))
                    .Select(relacion => relacion.UsuarioEmisor == usuarioId ? relacion.UsuarioReceptor
                        : relacion.UsuarioEmisor)
                    .Distinct()
                    .ToList();

                if (amigosIds.Count == 0)
                {
                    return new List<Usuario>();
                }

                return _contexto.Usuario
                    .Where(u => amigosIds.Contains(u.idUsuario))
                    .ToList();
            }
            catch (DbUpdateException excepcion)
            {
                _logger.ErrorFormat(
                    "Error de base de datos al obtener amigos para el usuario ID: {0}.",
                    usuarioId, excepcion);
                throw;
            }
            catch (EntityException excepcion)
            {
                _logger.ErrorFormat(
                    "Error de base de datos al obtener amigos para el usuario ID: {0}.",
                    usuarioId, excepcion);
                throw;
            }
            catch (DataException excepcion)
            {
                _logger.ErrorFormat(
                    "Error de base de datos al obtener amigos para el usuario ID: {0}.",
                    usuarioId, excepcion);
                throw;
            }
            catch (Exception excepcion)
            {
                _logger.ErrorFormat(
                    "Error de base de datos al obtener amigos para el usuario ID: {0}.",
                    usuarioId, excepcion);
                throw;
            }
        }
    }
}
