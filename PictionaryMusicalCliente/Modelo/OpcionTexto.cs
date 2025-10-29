namespace PictionaryMusicalCliente.Modelo
{
    public class OpcionTexto(string clave, string descripcion)
    {
        public string Clave { get; } = clave;
        public string Descripcion { get; } = descripcion;
        public override string ToString() => Descripcion;
    }
}
