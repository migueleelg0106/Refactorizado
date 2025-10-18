using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Cuentas;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface ICodigoVerificacionService
    {
        Task<ResultadoSolicitudCodigo> SolicitarCodigoRegistroAsync(SolicitudRegistroCuenta solicitud);

        Task<ResultadoSolicitudCodigo> ReenviarCodigoRegistroAsync(string tokenCodigo);

        Task<ResultadoRegistroCuenta> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado);
    }
}
