using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Gestiona el ciclo de vida de la creacion de nuevas cuentas de usuario en la base de datos.
    /// </summary>
    public interface ICuentaServicio
    {
        /// <summary>
        /// Procesa la solicitud de registro de un nuevo usuario verificando que no exista 
        /// previamente.
        /// </summary>
        /// <param name="solicitud">Los datos del nuevo usuario a registrar.</param>
        /// <returns>Confirmacion del registro o detalles del error si falla.</returns>
        Task<DTOs.ResultadoRegistroCuentaDTO> RegistrarCuentaAsync(DTOs.NuevaCuentaDTO solicitud);
    }
}