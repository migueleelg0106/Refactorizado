namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para mostrar notificaciones al usuario.
    /// </summary>
    public interface IAvisoServicio
    {
        /// <summary>
        /// Muestra un mensaje al usuario.
        /// </summary>
        /// <param name="mensaje">El contenido del aviso.</param>
        void Mostrar(string mensaje);
    }
}