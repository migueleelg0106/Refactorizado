using System.Threading.Tasks;

namespace PictionaryMusicalServidor.Servicios.Servicios.Notificadores
{
    /// <summary>
    /// Interfaz para el servicio de envio de invitaciones por correo electronico.
    /// </summary>
    public interface ICorreoInvitacionNotificador
    {
        /// <summary>
        /// Envia una invitacion a una partida de forma asincrona.
        /// </summary>
        /// <param name="correoDestino">Direccion de correo del destinatario.</param>
        /// <param name="codigoSala">Codigo de la sala a la que se invita.</param>
        /// <param name="creador">Nombre del usuario que envia la invitacion.</param>
        /// <param name="idioma">Idioma preferido para el correo.</param>
        /// <returns>True si el envio fue exitoso, False en caso contrario.</returns>
        Task<bool> EnviarInvitacionAsync(string correoDestino, string codigoSala, string creador,
            string idioma);
    }
}