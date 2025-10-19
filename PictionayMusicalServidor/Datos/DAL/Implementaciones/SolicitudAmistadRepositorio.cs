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
                .Include(j => j.Usuario)
                .ToList();
        }

        private static byte[] CrearEstado(bool aceptada)
        {
            return new[] { aceptada ? (byte)1 : (byte)0 };
        }
    }
}
