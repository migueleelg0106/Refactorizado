using PictionaryMusicalCliente.Sesiones;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Helpers
{
    /// <summary>
    /// Expone operaciones auxiliares para mantener sincronizada la sesin del usuario.
    /// </summary>
    public static class UsuarioMapeador
    {
        /// <summary>
        /// Actualiza la sesin del usuario actual a partir del DTO recibido del servidor.
        /// </summary>
        /// <param name="dto">Datos del usuario autenticado.</param>
        public static void ActualizarSesion(DTOs.UsuarioDTO dto)
        {
            if (dto == null)
            {
                SesionUsuarioActual.Instancia.CerrarSesion();
                return;
            }

            SesionUsuarioActual.Instancia.EstablecerUsuario(dto);
        }

    }
}
