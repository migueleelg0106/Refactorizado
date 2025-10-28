using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IPerfilManejador
    {
        [OperationContract]
        UsuarioDTO ObtenerPerfil(int idUsuario);

        [OperationContract]
        ResultadoOperacionDTO ActualizarPerfil(ActualizacionPerfilDTO solicitud);
    }
}
