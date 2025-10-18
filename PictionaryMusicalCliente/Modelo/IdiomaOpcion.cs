namespace PictionaryMusicalCliente.Modelo
{
    public class IdiomaOpcion
    {
        public IdiomaOpcion(string codigo, string descripcion)
        {
            Codigo = codigo;
            Descripcion = descripcion;
        }

        public string Codigo { get; }

        public string Descripcion { get; }

        public override string ToString()
        {
            return Descripcion;
        }
    }
}
