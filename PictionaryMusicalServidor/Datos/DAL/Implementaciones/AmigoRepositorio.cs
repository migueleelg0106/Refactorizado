using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    public class AmigoRepositorio : IAmigoRepositorio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AmigoRepositorio));
        private readonly BaseDatosPruebaEntities1 _contexto;

        public AmigoRepositorio(BaseDatosPruebaEntities1 contexto)
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
                _logger.Error("Error al verificar existencia de relación de amistad en la base de datos.", ex);
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
            _logger.Info($"Solicitud de amistad creada exitosamente entre Emisor ID: {usuarioEmisorId} y Receptor ID: {usuarioReceptorId}.");
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
                _logger.Error("Error al obtener la relación de amistad de la base de datos.", ex);
                throw;
            }
        }

        public IList<Amigo> ObtenerSolicitudesPendientes(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                var ex = new ArgumentOutOfRangeException(nameof(usuarioId), "El identificador del usuario debe ser positivo.");
                _logger.Error("Intento de obtener solicitudes con ID inválido.", ex);
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
                _logger.Error($"Error al obtener solicitudes pendientes para el usuario ID: {usuarioId}.", ex);
                throw;
            }
        }

        public void ActualizarEstado(Amigo relacion, bool estado)
        {
            if (relacion == null)
            {
                var ex = new ArgumentNullException(nameof(relacion));
                _logger.Error("Se intentó actualizar una relación nula.", ex);
                throw ex;
            }

            try
            {
                relacion.Estado = estado;
                _contexto.SaveChanges();

                _logger.Info($"Estado de amistad actualizado a {(estado ? "Aceptado" : "Pendiente")} entre Emisor: {relacion.UsuarioEmisor} y Receptor: {relacion.UsuarioReceptor}.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al actualizar el estado de la relación entre {relacion.UsuarioEmisor} y {relacion.UsuarioReceptor}.", ex);
                throw;
            }
        }

        public void EliminarRelacion(Amigo relacion)
        {
            if (relacion == null)
            {
                var ex = new ArgumentNullException(nameof(relacion));
                _logger.Error("Se intentó eliminar una relación nula.", ex);
                throw ex;
            }

            int emisorId = relacion.UsuarioEmisor;
            int receptorId = relacion.UsuarioReceptor;

            try
            {
                _contexto.Amigo.Remove(relacion);
                _contexto.SaveChanges();

                _logger.Info($"Relación de amistad eliminada correctamente entre {emisorId} y {receptorId}.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al eliminar la relación de amistad entre {emisorId} y {receptorId}.", ex);
                throw;
            }
        }

        public IList<Usuario> ObtenerAmigos(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                var ex = new ArgumentOutOfRangeException(nameof(usuarioId), "El identificador del usuario debe ser positivo.");
                _logger.Error("ID de usuario inválido al obtener lista de amigos.", ex);
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
                _logger.Error($"Error de base de datos al obtener amigos para el usuario ID: {usuarioId}.", ex);
                throw;
            }
        }
    }
}
