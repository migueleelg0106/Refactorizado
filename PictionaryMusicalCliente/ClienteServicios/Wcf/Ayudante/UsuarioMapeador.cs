using log4net;
using PictionaryMusicalCliente.Sesiones;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante
{
    /// <summary>
    /// Expone operaciones auxiliares para mantener sincronizada la sesion del usuario.
    /// </summary>
    public static class UsuarioMapeador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UsuarioMapeador));

        /// <summary>
        /// Actualiza la sesion del usuario actual a partir del DTO recibido del servidor.
        /// </summary>
        /// <param name="dto">Datos del usuario autenticado.</param>
        public static void ActualizarSesion(DTOs.UsuarioDTO dto)
        {
            if (dto == null)
            {
                _logger.Info("Cerrando sesión local de usuario (DTO nulo).");
                SesionUsuarioActual.CerrarSesion();
                return;
            }

            _logger.InfoFormat("Actualizando sesión local para usuario: {0}", 
                dto.NombreUsuario);
            SesionUsuarioActual.EstablecerUsuario(dto);
        }
    }
}