namespace PictionaryMusicalCliente.Modelo
{
    public class IdiomaOpcion(string codigo, string descripcion)
    {
        public string Codigo { get; } = codigo;

        public string Descripcion { get; } = descripcion;

        public override string ToString()
        {
            return Descripcion;
        }
    }
}
