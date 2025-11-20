namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Representa una opcion de seleccion basada en texto (clave-valor).
    /// Util para ComboBoxes y listas de seleccion.
    /// </summary>
    public class OpcionTexto(string clave, string descripcion)
    {
        /// <summary>
        /// El valor interno o identificador de la opcion.
        /// </summary>
        public string Clave { get; } = clave;

        /// <summary>
        /// El texto visible para el usuario.
        /// </summary>
        public string Descripcion { get; } = descripcion;

        /// <inheritdoc />
        public override string ToString() => Descripcion;
    }
}