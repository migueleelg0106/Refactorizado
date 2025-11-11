using System.ServiceModel;

namespace Servicios.Contratos
{
    public interface ISalasCallback
    {
        [OperationContract(IsOneWay = true)]
        void NotificarJugadorSeUnio(string codigoSala, string nombreJugador);

        [OperationContract(IsOneWay = true)]
        void NotificarJugadorSalio(string codigoSala, string nombreJugador);
    }
}
