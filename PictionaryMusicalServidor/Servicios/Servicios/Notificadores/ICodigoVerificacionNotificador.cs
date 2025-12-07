using System.Threading.Tasks;

namespace PictionaryMusicalServidor.Servicios.Servicios.Notificadores
{
    /// <summary>
    /// Define las operaciones para enviar notificaciones relacionadas con codigos de verificacion
    /// a un usuario mediante correo electronico.
    /// </summary>
    public interface ICodigoVerificacionNotificador
    {
        /// <summary>
        /// Envia una notificacion de codigo de verificacion al correo destino especificado.
        /// </summary>
        /// <param name="correoDestino">
        /// Direccion de correo electronico a la cual se enviara el codigo de verificacion.
        /// </param>
        /// <param name="codigo">
        /// Codigo de verificacion que sera enviado al usuario.
        /// </param>
        /// <param name="usuarioDestino">
        /// Nombre o identificador del usuario al que pertenece el codigo de verificacion.
        /// </param>
        /// <param name="idioma">
        /// Idioma en el que se debe generar el contenido del mensaje. Se usara para seleccionar
        /// el asunto y el cuerpo adecuados.
        /// </param>
        /// <returns>
        /// Retorna true si el mensaje fue enviado correctamente; de lo contrario, retorna false.
        /// </returns>
        Task<bool> NotificarAsync(string correoDestino, string codigo, string usuarioDestino, 
            string idioma);
    }
}
