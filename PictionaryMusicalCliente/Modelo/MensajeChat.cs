namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Representa un mensaje en el chat del juego.
    /// Puede ser un mensaje de usuario o un mensaje del sistema.
    /// </summary>
    public class MensajeChat
    {
        /// <summary>
        /// Nombre del usuario que envio el mensaje.
        /// </summary>
        public string NombreUsuario { get; set; }

        /// <summary>
        /// Contenido del mensaje.
        /// </summary>
        public string Contenido { get; set; }

        /// <summary>
        /// Indica si es un mensaje del sistema (para formato especial en la UI).
        /// </summary>
        public bool EsSistema { get; set; }

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public MensajeChat()
        {
        }

        /// <summary>
        /// Constructor con parametros.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario.</param>
        /// <param name="contenido">Contenido del mensaje.</param>
        /// <param name="esSistema">Indica si es mensaje del sistema.</param>
        public MensajeChat(string nombreUsuario, string contenido, bool esSistema = false)
        {
            NombreUsuario = nombreUsuario;
            Contenido = contenido;
            EsSistema = esSistema;
        }
    }
}
