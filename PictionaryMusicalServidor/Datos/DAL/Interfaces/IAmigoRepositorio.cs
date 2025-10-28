using System.Collections.Generic;
using Datos.Modelo;

namespace Datos.DAL.Interfaces
{
    public interface IAmigoRepositorio
    {
        bool ExisteRelacion(int usuarioAId, int usuarioBId);

        Amigo CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId);

        Amigo ObtenerRelacion(int usuarioAId, int usuarioBId);

        IList<Amigo> ObtenerSolicitudesPendientes(int usuarioId);

        void ActualizarEstado(Amigo relacion, bool estado);

        void EliminarRelacion(Amigo relacion);

        IList<Usuario> ObtenerAmigos(int usuarioId);
    }
}
