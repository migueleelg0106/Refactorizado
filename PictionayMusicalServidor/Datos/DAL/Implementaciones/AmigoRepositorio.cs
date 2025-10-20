using System;
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

        public Amigo CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId)
        {
            Amigo amistad = new Amigo
            {
                UsuarioEmisor = usuarioEmisorId,
                UsuarioReceptor = usuarioReceptorId,
                Estado = false
            };

            contexto.Amigo.Add(amistad);
            contexto.SaveChanges();
            return amistad;
        }

        public Amigo ObtenerRelacion(int usuarioEmisorId, int usuarioReceptorId)
        {
            return contexto.Amigo.FirstOrDefault(a => a.UsuarioEmisor == usuarioEmisorId && a.UsuarioReceptor == usuarioReceptorId);
        }

        public Amigo ObtenerAmistadEntreUsuarios(int usuarioAId, int usuarioBId)
        {
            return contexto.Amigo.FirstOrDefault(a =>
                (a.UsuarioEmisor == usuarioAId && a.UsuarioReceptor == usuarioBId) ||
                (a.UsuarioEmisor == usuarioBId && a.UsuarioReceptor == usuarioAId));
        }

        public bool ExisteRelacion(int usuarioAId, int usuarioBId)
        {
            return contexto.Amigo.Any(a =>
                (a.UsuarioEmisor == usuarioAId && a.UsuarioReceptor == usuarioBId) ||
                (a.UsuarioEmisor == usuarioBId && a.UsuarioReceptor == usuarioAId));
        }

        public bool EliminarAmistad(int usuarioAId, int usuarioBId)
        {
            Amigo amistad = ObtenerAmistadEntreUsuarios(usuarioAId, usuarioBId);
            if (amistad == null)
            {
                return false;
            }

            contexto.Amigo.Remove(amistad);
            contexto.SaveChanges();
            return true;
        }

        public void GuardarCambios()
        {
            contexto.SaveChanges();
        }
    }
}
