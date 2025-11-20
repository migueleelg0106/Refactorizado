using System.Globalization;

namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Representa una opcion numerica para controles de seleccion.
    /// </summary>
    public class OpcionEntero(int valor)
    {
        /// <summary>
        /// El valor numerico real de la opcion.
        /// </summary>
        public int Valor { get; } = valor;

        /// <summary>
        /// Representacion en cadena del valor formateada segun la cultura actual.
        /// </summary>
        public string Descripcion { get; } = valor.ToString(CultureInfo.CurrentCulture);

        /// <inheritdoc />
        public override string ToString() => Descripcion;
    }
}