using System.Collections.Generic;
using Datos.Modelo;

namespace Datos.DAL.Interfaces
{
    public interface ISolicitudRepositorio
    {
        Solicitud ObtenerSolicitudEntre(int jugadorId, int otroJugadorId);

        Solicitud CrearSolicitud(Solicitud solicitud);

        void ActualizarSolicitud(Solicitud solicitud);

        void EliminarSolicitud(Solicitud solicitud);

        IList<Solicitud> ObtenerSolicitudesPorJugador(int jugadorId);
    }
}
