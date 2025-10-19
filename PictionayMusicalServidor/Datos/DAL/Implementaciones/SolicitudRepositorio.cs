using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Datos.DAL.Interfaces;
using Datos.Modelo;

namespace Datos.DAL.Implementaciones
{
    public class SolicitudRepositorio : ISolicitudRepositorio
    {
        private readonly BaseDatosPruebaEntities1 contexto;

        public SolicitudRepositorio(BaseDatosPruebaEntities1 contexto)
        {
            this.contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public Solicitud ObtenerSolicitudEntre(int jugadorId, int otroJugadorId)
        {
            return contexto.Solicitud.FirstOrDefault(s =>
                (s.Jugador_idJugador == jugadorId && s.Jugador_idJugador1 == otroJugadorId) ||
                (s.Jugador_idJugador == otroJugadorId && s.Jugador_idJugador1 == jugadorId));
        }

        public Solicitud CrearSolicitud(Solicitud solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            var entidad = contexto.Solicitud.Add(solicitud);
            contexto.SaveChanges();
            return entidad;
        }

        public void ActualizarSolicitud(Solicitud solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

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

        public IList<Solicitud> ObtenerSolicitudesPorJugador(int jugadorId)
        {
            return contexto.Solicitud
                .Include(s => s.Jugador.Avatar)
                .Include(s => s.Jugador1.Avatar)
                .Where(s => s.Jugador_idJugador == jugadorId || s.Jugador_idJugador1 == jugadorId)
                .ToList();
        }
    }
}
