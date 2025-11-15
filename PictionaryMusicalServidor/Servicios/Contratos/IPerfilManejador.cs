using System.ServiceModel;
using PictionaryMusical.Servicios.Contratos.DTOs;

namespace PictionaryMusical.Servicios.Contratos
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
