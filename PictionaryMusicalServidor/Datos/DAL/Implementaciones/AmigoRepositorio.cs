using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    public class AmigoRepositorio : IAmigoRepositorio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AmigoRepositorio));
        private readonly BaseDatosPruebaEntities _contexto;

        public AmigoRepositorio(BaseDatosPruebaEntities contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public bool ExisteRelacion(int usuarioAId, int usuarioBId)
        {
            try
            {
                return _contexto.Amigo.Any(a =>
                    (a.UsuarioEmisor == usuarioAId && a.UsuarioReceptor == usuarioBId) ||
                    (a.UsuarioEmisor == usuarioBId && a.UsuarioReceptor == usuarioAId));
            }
            catch (Exception ex)
            {
                _logger.Error("Error al verificar existencia de relaci�n de amistad en la base de datos.", ex);
                throw;
            }
        }

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
            _logger.InfoFormat("Solicitud de amistad creada exitosamente entre Emisor ID: {0} y Receptor ID: {1}.", usuarioEmisorId, usuarioReceptorId);
            return solicitud;
        }

        public Amigo ObtenerRelacion(int usuarioAId, int usuarioBId)
        {
            try
            {
                return _contexto.Amigo.FirstOrDefault(a =>
                    (a.UsuarioEmisor == usuarioAId && a.UsuarioReceptor == usuarioBId) ||
                    (a.UsuarioEmisor == usuarioBId && a.UsuarioReceptor == usuarioAId));
            }
            catch (Exception ex)
            {
                _logger.Error("Error al obtener la relaci�n de amistad de la base de datos.", ex);
                throw;
            }
        }

        public IList<Amigo> ObtenerSolicitudesPendientes(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                var ex = new ArgumentOutOfRangeException(nameof(usuarioId), "El identificador del usuario debe ser positivo.");
                _logger.Error("Intento de obtener solicitudes con ID inv�lido.", ex);
                throw ex;
            }

            try
            {
                return _contexto.Amigo
                    .Where(a => !a.Estado && (a.UsuarioEmisor == usuarioId || a.UsuarioReceptor == usuarioId))
                    .Include(a => a.Usuario)
                    .Include(a => a.Usuario1)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error al obtener solicitudes pendientes para el usuario ID: {0}.", usuarioId), ex);
                throw;
            }
        }

        public void ActualizarEstado(Amigo relacion, bool estado)
        {
            if (relacion == null)
            {
                var ex = new ArgumentNullException(nameof(relacion));
                _logger.Error("Se intent� actualizar una relaci�n nula.", ex);
                throw ex;
            }

            try
            {
                relacion.Estado = estado;
                _contexto.SaveChanges();

                _logger.InfoFormat("Estado de amistad actualizado a {0} entre Emisor: {1} y Receptor: {2}.", (estado ? "Aceptado" : "Pendiente"), relacion.UsuarioEmisor, relacion.UsuarioReceptor);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error al actualizar el estado de la relaci�n entre {0} y {1}.", relacion.UsuarioEmisor, relacion.UsuarioReceptor), ex);
                throw;
            }
        }

        public void EliminarRelacion(Amigo relacion)
        {
            if (relacion == null)
            {
                var ex = new ArgumentNullException(nameof(relacion));
                _logger.Error("Se intent� eliminar una relaci�n nula.", ex);
                throw ex;
            }

            int emisorId = relacion.UsuarioEmisor;
            int receptorId = relacion.UsuarioReceptor;

            try
            {
                _contexto.Amigo.Remove(relacion);
                _contexto.SaveChanges();

                _logger.InfoFormat("Relaci�n de amistad eliminada correctamente entre {0} y {1}.", emisorId, receptorId);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error al eliminar la relaci�n de amistad entre {0} y {1}.", emisorId, receptorId), ex);
                throw;
            }
        }

        public IList<Usuario> ObtenerAmigos(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                var ex = new ArgumentOutOfRangeException(nameof(usuarioId), "El identificador del usuario debe ser positivo.");
                _logger.Error("ID de usuario inv�lido al obtener lista de amigos.", ex);
                throw ex;
            }

            try
            {
                var amigosIds = _contexto.Amigo
                    .Where(a => a.Estado && (a.UsuarioEmisor == usuarioId || a.UsuarioReceptor == usuarioId))
                    .Select(a => a.UsuarioEmisor == usuarioId ? a.UsuarioReceptor : a.UsuarioEmisor)
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
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error de base de datos al obtener amigos para el usuario ID: {0}.", usuarioId), ex);
                throw;
            }
        }
    }
}
