using System.Globalization;

namespace PictionaryMusicalCliente.Modelo
{
    public class OpcionEntero(int valor)
    {
        public int Valor { get; } = valor;
        public string Descripcion { get; } = valor.ToString(CultureInfo.CurrentCulture);
        public override string ToString() => Descripcion;
    }
}
