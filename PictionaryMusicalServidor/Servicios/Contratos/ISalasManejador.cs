using System.Collections.Generic;
using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{

    [ServiceContract(CallbackContract = typeof(ISalasCallback))]
    public interface ISalasManejador
    {
        [OperationContract]
        SalaDTO CrearSala(string nombreCreador, ConfiguracionPartidaDTO configuracion);

        [OperationContract]
        SalaDTO UnirseSala(string codigoSala, string nombreUsuario);

        [OperationContract]
        IList<SalaDTO> ObtenerSalas();

        [OperationContract]
        void AbandonarSala(string codigoSala, string nombreUsuario);
    }
}
