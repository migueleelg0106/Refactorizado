using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Define el contrato para acceder y gestionar el estado del usuario autenticado.
    /// </summary>
    public interface IUsuarioAutenticado
    {
        /// <summary>
        /// Identificador unico de la cuenta de usuario.
        /// </summary>
        int IdUsuario { get; }

        /// <summary>
        /// Identificador del perfil de jugador asociado.
        /// </summary>
        int JugadorId { get; }

        /// <summary>
        /// Nombre de usuario (nickname) unico en el sistema.
        /// </summary>
        string NombreUsuario { get; }

        /// <summary>
        /// Nombre real del usuario.
        /// </summary>
        string Nombre { get; }

        /// <summary>
        /// Apellido real del usuario.
        /// </summary>
        string Apellido { get; }

        /// <summary>
        /// Correo electronico registrado.
        /// </summary>
        string Correo { get; }

        /// <summary>
        /// Identificador del avatar seleccionado.
        /// </summary>
        int AvatarId { get; }

        /// <summary>
        /// Ruta relativa de la imagen del avatar.
        /// </summary>
        string AvatarRutaRelativa { get; }

        /// <summary>
        /// Usuario de Instagram.
        /// </summary>
        string Instagram { get; }

        /// <summary>
        /// Usuario de Facebook.
        /// </summary>
        string Facebook { get; }

        /// <summary>
        /// Usuario de X (Twitter).
        /// </summary>
        string X { get; }

        /// <summary>
        /// Usuario de Discord.
        /// </summary>
        string Discord { get; }

        /// <summary>
        /// Actualiza los datos de la sesion local con la informacion del servidor.
        /// </summary>
        /// <param name="dto">DTO con la informacion del usuario.</param>
        void CargarDesdeDTO(DTOs.UsuarioDTO dto);

        /// <summary>
        /// Restablece los valores de la sesion (Cerrar sesion).
        /// </summary>
        void Limpiar();

        /// <summary>
        /// Indica si el usuario esta autenticado en el sistema.
        /// </summary>
        bool EstaAutenticado { get; }
    }
}