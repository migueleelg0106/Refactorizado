using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Datos.DAL.Interfaces;
using Datos.Modelo;

namespace Datos.DAL.Implementaciones
{
    public class AmigoRepositorio : IAmigoRepositorio
    {
        private readonly BaseDatosPruebaEntities1 contexto;

        public AmigoRepositorio(BaseDatosPruebaEntities1 contexto)
        {
            this.contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public bool ExisteRelacion(int usuarioAId, int usuarioBId)
        {
            return contexto.Amigo.Any(a =>
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

            contexto.Amigo.Add(solicitud);
            contexto.SaveChanges();
            return solicitud;
        }

        public Amigo ObtenerRelacion(int usuarioAId, int usuarioBId)
        {
            return contexto.Amigo.FirstOrDefault(a =>
                (a.UsuarioEmisor == usuarioAId && a.UsuarioReceptor == usuarioBId) ||
                (a.UsuarioEmisor == usuarioBId && a.UsuarioReceptor == usuarioAId));
        }

        public IList<Amigo> ObtenerSolicitudesPendientes(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(usuarioId), "El identificador del usuario debe ser positivo.");
            }

            return contexto.Amigo
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
            contexto.SaveChanges();
        }

        public void EliminarRelacion(Amigo relacion)
        {
            if (relacion == null)
            {
                throw new ArgumentNullException(nameof(relacion));
            }

            contexto.Amigo.Remove(relacion);
            contexto.SaveChanges();
        }

        public IList<Usuario> ObtenerAmigos(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(usuarioId), "El identificador del usuario debe ser positivo.");
            }

            var amigosIds = contexto.Amigo
                .Where(a => a.Estado && (a.UsuarioEmisor == usuarioId || a.UsuarioReceptor == usuarioId))
                .Select(a => a.UsuarioEmisor == usuarioId ? a.UsuarioReceptor : a.UsuarioEmisor)
                .Distinct()
                .ToList();

            if (amigosIds.Count == 0)
            {
                return new List<Usuario>();
            }

            return contexto.Usuario
                .Where(u => amigosIds.Contains(u.idUsuario))
                .ToList();
        }
    }
}
