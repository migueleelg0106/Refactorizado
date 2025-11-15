using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    public class AmigoRepositorio : IAmigoRepositorio
    {
        private readonly BaseDatosPruebaEntities1 _contexto;

        public AmigoRepositorio(BaseDatosPruebaEntities1 contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public bool ExisteRelacion(int usuarioAId, int usuarioBId)
        {
            return _contexto.Amigo.Any(a =>
                (a.UsuarioEmisor == usuarioAId && a.UsuarioReceptor == usuarioBId) ||
                (a.UsuarioEmisor == usuarioBId && a.UsuarioReceptor == usuarioAId));
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
            return solicitud;
        }

        public Amigo ObtenerRelacion(int usuarioAId, int usuarioBId)
        {
            return _contexto.Amigo.FirstOrDefault(a =>
                (a.UsuarioEmisor == usuarioAId && a.UsuarioReceptor == usuarioBId) ||
                (a.UsuarioEmisor == usuarioBId && a.UsuarioReceptor == usuarioAId));
        }

        public IList<Amigo> ObtenerSolicitudesPendientes(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(usuarioId), "El identificador del usuario debe ser positivo.");
            }

            return _contexto.Amigo
                .Where(a => !a.Estado && (a.UsuarioEmisor == usuarioId || a.UsuarioReceptor == usuarioId))
                .Include(a => a.Usuario)
                .Include(a => a.Usuario1)
                .ToList();
        }

        public void ActualizarEstado(Amigo relacion, bool estado)
        {
            if (relacion == null)
            {
                throw new ArgumentNullException(nameof(relacion));
            }

            relacion.Estado = estado;
            _contexto.SaveChanges();
        }

        public void EliminarRelacion(Amigo relacion)
        {
            if (relacion == null)
            {
                throw new ArgumentNullException(nameof(relacion));
            }

            _contexto.Amigo.Remove(relacion);
            _contexto.SaveChanges();
        }

        public IList<Usuario> ObtenerAmigos(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(usuarioId), "El identificador del usuario debe ser positivo.");
            }

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
    }
}
