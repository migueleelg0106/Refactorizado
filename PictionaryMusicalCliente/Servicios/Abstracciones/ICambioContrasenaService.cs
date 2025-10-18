using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Cuentas;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface ICambioContrasenaService
    {
        Task<ResultadoSolicitudRecuperacion> SolicitarCodigoRecuperacionAsync(string identificador);

        Task<ResultadoSolicitudCodigo> ReenviarCodigoRecuperacionAsync(string tokenCodigo);

        Task<ResultadoOperacion> ConfirmarCodigoRecuperacionAsync(string tokenCodigo, string codigoIngresado);

        Task<ResultadoOperacion> ActualizarContrasenaAsync(string tokenCodigo, string nuevaContrasena);
    }
}
