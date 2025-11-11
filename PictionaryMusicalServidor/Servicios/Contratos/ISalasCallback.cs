using System.ServiceModel;

namespace Servicios.Contratos
{
    public interface ISalasCallback
    {
        [OperationContract(IsOneWay = true)]
        void JugadorSeUnio(string codigoSala, string nombreJugador);

        [OperationContract(IsOneWay = true)]
        void JugadorSalio(string codigoSala, string nombreJugador);
    }
}
