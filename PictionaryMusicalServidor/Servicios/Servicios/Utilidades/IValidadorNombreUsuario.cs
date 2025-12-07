namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Define las operaciones de validacion y normalizacion para nombres de usuario.
    /// </summary>
    public interface IValidadorNombreUsuario
    {
        /// <summary>
        /// Valida que el nombre de usuario cumpla con los requisitos.
        /// </summary>
        void Validar(string nombreUsuario, string parametro);

        /// <summary>
        /// Obtiene el nombre normalizado de usuario, priorizando el de la base de datos.
        /// </summary>
        string ObtenerNombreNormalizado(string nombreBaseDatos, string nombreAlterno);
    }
}