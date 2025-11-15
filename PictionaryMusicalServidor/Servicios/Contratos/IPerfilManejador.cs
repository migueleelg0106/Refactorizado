using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
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
