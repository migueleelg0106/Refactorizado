using System.Windows.Input;

namespace PictionaryMusicalCliente.Modelo
{
    public class JugadorElemento
    {
        public string Nombre { get; set; }
        public bool MostrarBotonExpulsar { get; set; }
        public ICommand ExpulsarComando { get; set; }
    }
}
