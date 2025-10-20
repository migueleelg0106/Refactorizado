using Datos.Modelo;

namespace Datos.DAL.Interfaces
{
    public interface IAmigoRepositorio
    {
        Amigo CrearSolicitud(int usuarioEmisorId, int usuarioReceptorId);

        Amigo ObtenerRelacion(int usuarioEmisorId, int usuarioReceptorId);

        Amigo ObtenerAmistadEntreUsuarios(int usuarioAId, int usuarioBId);

        bool ExisteRelacion(int usuarioAId, int usuarioBId);

        bool EliminarAmistad(int usuarioAId, int usuarioBId);

        void GuardarCambios();
    }
}
