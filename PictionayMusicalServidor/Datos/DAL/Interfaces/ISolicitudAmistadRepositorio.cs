using Datos.Modelo;

namespace Datos.DAL.Interfaces
{
    public interface ISolicitudAmistadRepositorio
    {
        Jugador ObtenerJugadorPorNombreUsuario(string nombreUsuario);

        Solicitud ObtenerSolicitudEntre(int primerJugadorId, int segundoJugadorId);

        Solicitud CrearSolicitud(int remitenteId, int receptorId);

        void ActualizarEstado(Solicitud solicitud, bool aceptada);

        void EliminarSolicitud(Solicitud solicitud);
    }
}
