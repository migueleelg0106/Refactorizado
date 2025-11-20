using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de gestion de cuentas en el cliente.
    /// </summary>
    public interface ICuentaServicio
    {
        /// <summary>
        /// Registra una nueva cuenta de usuario de forma asincrona.
        /// </summary>
        /// <param name="solicitud">Datos de la nueva cuenta a registrar.</param>
        /// <returns>Resultado del registro de la cuenta.</returns>
        Task<DTOs.ResultadoRegistroCuentaDTO> RegistrarCuentaAsync(DTOs.NuevaCuentaDTO solicitud);
    }
}
