using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IVerificarCodigoDialogService
    {
        Task<Modelo.Cuentas.ResultadoRegistroCuenta> MostrarDialogoAsync(
            string descripcion,
            string tokenCodigo,
            ICodigoVerificacionService codigoVerificacionService);
    }
}
