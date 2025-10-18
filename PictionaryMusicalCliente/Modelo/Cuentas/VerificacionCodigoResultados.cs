namespace PictionaryMusicalCliente.Modelo.Cuentas
{
    public sealed class ConfirmacionCodigoResultado
    {
        public ConfirmacionCodigoResultado(bool operacionExitosa, string mensaje)
        {
            OperacionExitosa = operacionExitosa;
            Mensaje = mensaje;
        }

        public bool OperacionExitosa { get; }

        public string Mensaje { get; }
    }

    public sealed class ReenvioCodigoResultado
    {
        public ReenvioCodigoResultado(bool codigoEnviado, string mensaje, string tokenCodigo)
        {
            CodigoEnviado = codigoEnviado;
            Mensaje = mensaje;
            TokenCodigo = tokenCodigo;
        }

        public bool CodigoEnviado { get; }

        public string Mensaje { get; }

        public string TokenCodigo { get; }
    }
}
