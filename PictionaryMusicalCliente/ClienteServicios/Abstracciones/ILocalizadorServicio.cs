namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para la localizacion y traduccion de mensajes provenientes del servidor.
    /// </summary>
    public interface ILocalizadorServicio
    {
        /// <summary>
        /// Intenta traducir un mensaje del servidor usando mapas de recursos o expresiones
        /// regulares.
        /// </summary>
        /// <param name="mensaje">Mensaje original recibido del servidor.</param>
        /// <param name="mensajePredeterminado">Mensaje alternativo a retornar si no se encuentra
        /// una traduccion especifica.</param>
        /// <returns>El mensaje traducido al idioma actual o el mensaje predeterminado.</returns>
        string Localizar(string mensaje, string mensajePredeterminado);
    }
}