using System.Collections.Generic;
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
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

        [OperationContract]
        void SuscribirListaSalas();

        [OperationContract]
        void CancelarSuscripcionListaSalas();

        [OperationContract]
        void ExpulsarJugador(string codigoSala, string nombreHost, string nombreJugadorAExpulsar);
    }
}
