namespace PictionaryMusicalCliente.Modelo
{
    public class ResultadoOperacion
    {
        public bool Exito { get; private set; }

        public string Mensaje { get; private set; }

        private ResultadoOperacion(bool exito, string mensaje)
        {
            Exito = exito;
            Mensaje = mensaje;
        }

        public static ResultadoOperacion Exitoso()
        {
            return new ResultadoOperacion(true, null);
        }

        public static ResultadoOperacion Exitoso(string mensaje)
        {
            return new ResultadoOperacion(true, mensaje);
        }

        public static ResultadoOperacion Fallo(string mensaje)
        {
            return new ResultadoOperacion(false, mensaje);
        }
    }
}
