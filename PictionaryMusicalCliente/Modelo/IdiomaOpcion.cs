namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Representa un idioma disponible para la seleccion en la configuracion.
    /// </summary>
    public class IdiomaOpcion(string codigo, string descripcion)
    {
        /// <summary>
        /// Codigo estandar del idioma (ej. "es-MX", "en-US").
        /// </summary>
        public string Codigo { get; } = codigo;

        /// <summary>
        /// Nombre legible del idioma para mostrar al usuario.
        /// </summary>
        public string Descripcion { get; } = descripcion;

        /// <inheritdoc />
        public override string ToString()
        {
            return Descripcion;
        }
    }
}