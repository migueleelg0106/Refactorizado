namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Interfaz que define las operaciones para el envio de notificaciones de codigos.
    /// </summary>
    public interface INotificacionCodigosServicio
    {
        /// <summary>
        /// Envia un codigo de verificacion por correo electronico.
        /// </summary>
        bool EnviarNotificacion(
            string correoDestino,
            string codigo,
            string usuarioDestino,
            string idioma);
    }
}