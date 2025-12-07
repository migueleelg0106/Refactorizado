using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define las operaciones para sincronizar y gestionar la informacion de la sesion del 
    /// usuario actual.
    /// </summary>
    public interface IUsuarioMapeador
    {
        /// <summary>
        /// Actualiza la sesion global del usuario actual a partir de los datos recibidos del
        /// servidor.
        /// </summary>
        /// <param name="dto">Datos del usuario autenticado.</param>
        void ActualizarSesion(DTOs.UsuarioDTO dto);
    }
}