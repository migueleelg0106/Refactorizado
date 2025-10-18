using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Servicios.Abstracciones;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IRecuperacionCuentaDialogService
    {
        Task<ResultadoOperacion> RecuperarCuentaAsync(
            string identificador,
            ICambioContrasenaService cambioContrasenaService);
    }
}
