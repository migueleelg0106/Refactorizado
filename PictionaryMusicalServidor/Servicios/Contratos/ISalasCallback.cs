using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    public interface ISalasCallback
    {
        [OperationContract(IsOneWay = true)]
        void NotificarJugadorSeUnio(string codigoSala, string nombreJugador);

        [OperationContract(IsOneWay = true)]
        void NotificarJugadorSalio(string codigoSala, string nombreJugador);

        [OperationContract(IsOneWay = true)]
        void NotificarListaSalasActualizada(DTOs.SalaDTO[] salas);

        [OperationContract(IsOneWay = true)]
        void NotificarSalaActualizada(DTOs.SalaDTO sala);
    }
}
