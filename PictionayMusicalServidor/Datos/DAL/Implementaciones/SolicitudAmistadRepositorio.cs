using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Datos.DAL.Interfaces;
using Datos.Modelo;

namespace Datos.DAL.Implementaciones
{
    public class SolicitudAmistadRepositorio : ISolicitudAmistadRepositorio
    {
        private readonly BaseDatosPruebaEntities1 contexto;

        public SolicitudAmistadRepositorio(BaseDatosPruebaEntities1 contexto)
        {
            this.contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public Jugador ObtenerJugadorPorNombreUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return null;
            }

            return contexto.Usuario
                .Where(u => u.Nombre_Usuario == nombreUsuario)
                .Select(u => u.Jugador)
                .FirstOrDefault();
        }

        public Solicitud ObtenerSolicitudEntre(int primerJugadorId, int segundoJugadorId)
        {
            return contexto.Solicitud.FirstOrDefault(s =>
                (s.Jugador_idJugador == primerJugadorId && s.Jugador_idJugador1 == segundoJugadorId) ||
                (s.Jugador_idJugador == segundoJugadorId && s.Jugador_idJugador1 == primerJugadorId));
        }

        public Solicitud CrearSolicitud(int remitenteId, int receptorId)
        {
            var solicitud = new Solicitud
            {
                Jugador_idJugador = remitenteId,
                Jugador_idJugador1 = receptorId,
                Estado = CrearEstado(false)
            };

            contexto.Solicitud.Add(solicitud);
            contexto.SaveChanges();
            return solicitud;
        }

        public void ActualizarEstado(Solicitud solicitud, bool aceptada)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            solicitud.Estado = CrearEstado(aceptada);
            contexto.Entry(solicitud).State = EntityState.Modified;
            contexto.SaveChanges();
        }

        public void EliminarSolicitud(Solicitud solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            contexto.Solicitud.Remove(solicitud);
            contexto.SaveChanges();
        }

        public IEnumerable<Jugador> ObtenerAmigosDe(int jugadorId)
        {
            return contexto.Solicitud
                .Where(s =>
                    (s.Jugador_idJugador == jugadorId || s.Jugador_idJugador1 == jugadorId) &&
                    s.Estado != null && s.Estado.Length > 0 && s.Estado[0] != 0)
                .Select(s => s.Jugador_idJugador == jugadorId ? s.Jugador1 : s.Jugador)
                .Where(j => j != null)
                .Include(j => j.Usuario)
                .ToList();
        }

        public IEnumerable<string> ObtenerNombresAmigosDe(int jugadorId)
        {
            if (jugadorId <= 0)
            {
                return Enumerable.Empty<string>();
            }

            IQueryable<int> amigosIds = contexto.Solicitud
                .Where(s =>
                    (s.Jugador_idJugador == jugadorId || s.Jugador_idJugador1 == jugadorId)
                    && s.Estado != null
                    && s.Estado.Length > 0
                    && s.Estado[0] != 0)
                .Select(s => s.Jugador_idJugador == jugadorId ? s.Jugador_idJugador1 : s.Jugador_idJugador)
                .Where(id => id > 0 && id != jugadorId)
                .Distinct();

            return contexto.Usuario
                .Where(u =>
                    amigosIds.Contains(u.Jugador_idJugador)
                    && u.Nombre_Usuario != null
                    && u.Nombre_Usuario != "")
                .Select(u => u.Nombre_Usuario)
                .Distinct()
                .ToList();
        }

        private static byte[] CrearEstado(bool aceptada)
        {
            return new[] { aceptada ? (byte)1 : (byte)0 };
        }
    }
}
